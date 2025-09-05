using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PaymentGatewayService.Services.Interfaces;

namespace PaymentGatewayService.Services;

/// <summary>
/// HTTP client implementation for communicating with QuantumLedger.Hub
/// </summary>
public class QuantumLedgerHubClient : IQuantumLedgerHubClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuantumLedgerHubClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public QuantumLedgerHubClient(HttpClient httpClient, ILogger<QuantumLedgerHubClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> RecordDepositCodeCreationAsync(DepositCodeLedgerEntry entry)
    {
        try
        {
            var request = new
            {
                Id = $"DEPOSIT_CODE_CREATE_{entry.DepositCode}_{DateTime.UtcNow.Ticks}",
                Type = "DepositCodeCreation",
                Amount = entry.Amount,
                FromAddress = "SYSTEM",
                ToAddress = entry.UserId?.ToString() ?? "SYSTEM",
                Data = new Dictionary<string, object>
                {
                    ["depositCode"] = entry.DepositCode,
                    ["currency"] = entry.Currency,
                    ["createdAt"] = entry.CreatedAt,
                    ["expiresAt"] = entry.ExpiresAt,
                    ["metadata"] = entry.Metadata
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/v1/ledger/transactions", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully recorded deposit code creation in ledger: {DepositCode}", entry.DepositCode);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to record deposit code creation in ledger. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording deposit code creation in ledger: {DepositCode}", entry.DepositCode);
            return false;
        }
    }

    public async Task<bool> RecordDepositCodeUsageAsync(DepositCodeUsageLedgerEntry entry)
    {
        try
        {
            var request = new
            {
                Id = $"DEPOSIT_CODE_USE_{entry.DepositCode}_{DateTime.UtcNow.Ticks}",
                Type = "DepositCodeUsage",
                Amount = entry.Amount,
                FromAddress = entry.UserId?.ToString() ?? "SYSTEM",
                ToAddress = "PAYMENT_GATEWAY",
                Data = new Dictionary<string, object>
                {
                    ["depositCode"] = entry.DepositCode,
                    ["paymentId"] = entry.PaymentId,
                    ["currency"] = entry.Currency,
                    ["usedAt"] = entry.UsedAt,
                    ["metadata"] = entry.Metadata
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/v1/ledger/transactions", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully recorded deposit code usage in ledger: {DepositCode}", entry.DepositCode);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to record deposit code usage in ledger. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording deposit code usage in ledger: {DepositCode}", entry.DepositCode);
            return false;
        }
    }

    public async Task<bool> RecordDepositCodeRejectionAsync(DepositCodeRejectionLedgerEntry entry)
    {
        try
        {
            var request = new
            {
                Id = $"DEPOSIT_CODE_REJECT_{entry.DepositCode}_{DateTime.UtcNow.Ticks}",
                Type = "DepositCodeRejection",
                Amount = entry.RefundAmount,
                FromAddress = "PAYMENT_GATEWAY",
                ToAddress = "REFUND_POOL",
                Data = new Dictionary<string, object>
                {
                    ["depositCode"] = entry.DepositCode,
                    ["paymentId"] = entry.PaymentId,
                    ["rejectionReason"] = entry.RejectionReason,
                    ["feeAmount"] = entry.FeeAmount,
                    ["currency"] = entry.Currency,
                    ["rejectedAt"] = entry.RejectedAt,
                    ["metadata"] = entry.Metadata
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/v1/ledger/transactions", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully recorded deposit code rejection in ledger: {DepositCode}", entry.DepositCode);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to record deposit code rejection in ledger. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording deposit code rejection in ledger: {DepositCode}", entry.DepositCode);
            return false;
        }
    }

    public async Task<DepositCodeLedgerValidation?> ValidateDepositCodeAsync(string depositCode)
    {
        try
        {
            // Query the ledger for deposit code transactions
            var transactionId = $"DEPOSIT_CODE_CREATE_{depositCode}";
            var response = await _httpClient.GetAsync($"api/v1/ledger/transactions/{transactionId}");
            
            if (!response.IsSuccessStatusCode)
            {
                return new DepositCodeLedgerValidation { Exists = false };
            }

            var content = await response.Content.ReadAsStringAsync();
            var transaction = JsonSerializer.Deserialize<TransactionResponse>(content, _jsonOptions);
            
            if (transaction == null)
            {
                return new DepositCodeLedgerValidation { Exists = false };
            }

            var validation = new DepositCodeLedgerValidation
            {
                Exists = true,
                CreatedAt = transaction.CreatedAt,
                Status = transaction.Status
            };

            // Extract additional data from transaction metadata
            if (transaction.Data != null)
            {
                if (transaction.Data.TryGetValue("expiresAt", out var expiresAt) && 
                    DateTime.TryParse(expiresAt.ToString(), out var expiry))
                {
                    validation.ExpiresAt = expiry;
                    validation.IsExpired = expiry < DateTime.UtcNow;
                }

                if (transaction.Data.TryGetValue("usedAt", out var usedAt) && 
                    DateTime.TryParse(usedAt.ToString(), out var used))
                {
                    validation.UsedAt = used;
                    validation.IsUsed = true;
                }
            }

            validation.IsActive = validation.Exists && !validation.IsExpired && !validation.IsUsed;
            
            return validation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating deposit code in ledger: {DepositCode}", depositCode);
            return null;
        }
    }

    public async Task<List<DepositCodeLedgerHistory>> GetDepositCodeHistoryAsync(string depositCode)
    {
        try
        {
            var history = new List<DepositCodeLedgerHistory>();
            
            // Get all transactions related to this deposit code
            // This would need a more sophisticated query in a real implementation
            var transactionTypes = new[] { "Create", "Use", "Reject" };
            
            foreach (var type in transactionTypes)
            {
                var transactionId = $"DEPOSIT_CODE_{type.ToUpper()}_{depositCode}";
                var response = await _httpClient.GetAsync($"api/v1/ledger/transactions/{transactionId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var transaction = JsonSerializer.Deserialize<TransactionResponse>(content, _jsonOptions);
                    
                    if (transaction != null)
                    {
                        history.Add(new DepositCodeLedgerHistory
                        {
                            TransactionId = transactionId,
                            EventType = $"DepositCode{type}",
                            Timestamp = transaction.CreatedAt,
                            Data = transaction.Data ?? new Dictionary<string, object>()
                        });
                    }
                }
            }

            return history.OrderBy(h => h.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deposit code history from ledger: {DepositCode}", depositCode);
            return new List<DepositCodeLedgerHistory>();
        }
    }

    public async Task<string?> RecordPaymentTransactionAsync(PaymentLedgerEntry entry)
    {
        try
        {
            var request = new
            {
                Id = $"PAYMENT_{entry.PaymentId}_{DateTime.UtcNow.Ticks}",
                Type = entry.PaymentType,
                Amount = entry.Amount,
                FromAddress = entry.UserId?.ToString() ?? "SYSTEM",
                ToAddress = "PAYMENT_GATEWAY",
                Data = new Dictionary<string, object>
                {
                    ["paymentId"] = entry.PaymentId,
                    ["feeAmount"] = entry.FeeAmount,
                    ["netAmount"] = entry.NetAmount,
                    ["currency"] = entry.Currency,
                    ["status"] = entry.Status,
                    ["depositCode"] = entry.DepositCode ?? "",
                    ["createdAt"] = entry.CreatedAt,
                    ["metadata"] = entry.Metadata
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/v1/ledger/transactions", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TransactionResponse>(responseContent, _jsonOptions);
                
                _logger.LogInformation("Successfully recorded payment transaction in ledger: {PaymentId}", entry.PaymentId);
                return result?.TransactionId;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to record payment transaction in ledger. Status: {StatusCode}, Error: {Error}", 
                response.StatusCode, errorContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment transaction in ledger: {PaymentId}", entry.PaymentId);
            return null;
        }
    }

    public async Task<decimal> GetAccountBalanceAsync(string accountAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/ledger/accounts/{accountAddress}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get account balance. Status: {StatusCode}", response.StatusCode);
                return 0;
            }

            var content = await response.Content.ReadAsStringAsync();
            var balanceResult = JsonSerializer.Deserialize<BalanceResponse>(content, _jsonOptions);
            
            return balanceResult?.Balance ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account balance from ledger: {AccountAddress}", accountAddress);
            return 0;
        }
    }
}

// Response models for QuantumLedger.Hub
internal class TransactionResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

internal class BalanceResponse
{
    public string AccountAddress { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime AsOf { get; set; }
}