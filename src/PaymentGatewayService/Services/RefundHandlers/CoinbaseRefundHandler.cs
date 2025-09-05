using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EntityRefundStatus = PaymentGatewayService.Data.Entities.RefundStatus;

namespace PaymentGatewayService.Services;

/// <summary>
/// Coinbase-specific refund handler implementation
/// </summary>
public class CoinbaseRefundHandler : IGatewayRefundHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CoinbaseRefundHandler(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClient = httpClientFactory.CreateClient("Coinbase");
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<GatewayRefundResult> ProcessRefundAsync(Payment payment, Refund refund)
    {
        try
        {
            _logger.LogInformation("Processing Coinbase refund. PaymentId: {PaymentId}, RefundId: {RefundId}, Amount: {Amount}",
                payment.Id, refund.Id, refund.Amount);

            // Build Coinbase refund request
            var coinbaseRequest = new
            {
                charge_id = payment.GatewayTransactionId,
                amount = new
                {
                    amount = refund.Amount.ToString("F8"),
                    currency = refund.Currency.ToUpper()
                },
                reason = refund.Reason,
                refund_address = ExtractRefundAddress(payment.Metadata),
                notes = $"Refund for payment {payment.Id}"
            };

            var json = JsonSerializer.Serialize(coinbaseRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add Coinbase authentication headers
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signature = GenerateCoinbaseSignature("POST", "/v2/refunds", json, timestamp);
            
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", GetCoinbaseApiKey());
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp);
            _httpClient.DefaultRequestHeaders.Add("CB-VERSION", "2023-12-01");

            var response = await _httpClient.PostAsync("/v2/refunds", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var coinbaseResponse = JsonSerializer.Deserialize<CoinbaseRefundResponse>(responseContent, _jsonOptions);
                
                _logger.LogInformation("Coinbase refund initiated successfully. RefundId: {RefundId}, CoinbaseRefundId: {CoinbaseRefundId}",
                    refund.Id, coinbaseResponse?.Data?.Id);

                return new GatewayRefundResult
                {
                    Status = EntityRefundStatus.Processing,
                    GatewayRefundId = coinbaseResponse?.Data?.Id
                };
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<CoinbaseErrorResponse>(responseContent, _jsonOptions);
                var errorMessage = errorResponse?.Errors?.FirstOrDefault()?.Message ?? "Unknown Coinbase error";
                
                _logger.LogError("Coinbase refund failed. RefundId: {RefundId}, StatusCode: {StatusCode}, Error: {Error}",
                    refund.Id, response.StatusCode, errorMessage);

                return new GatewayRefundResult
                {
                    Status = EntityRefundStatus.Failed,
                    ErrorMessage = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Coinbase refund. RefundId: {RefundId}", refund.Id);
            return new GatewayRefundResult
            {
                Status = EntityRefundStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    private string GetCoinbaseApiKey()
    {
        // In production, this would come from secure configuration
        return Environment.GetEnvironmentVariable("COINBASE_API_KEY") ?? "test_api_key";
    }

    private string GenerateCoinbaseSignature(string method, string path, string body, string timestamp)
    {
        // In production, this would use the actual secret key
        var secretKey = Environment.GetEnvironmentVariable("COINBASE_API_SECRET") ?? "test_secret";
        var message = $"{timestamp}{method}{path}{body}";
        
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    private string ExtractRefundAddress(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return string.Empty;

        try
        {
            var metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
            if (metadataDict != null && metadataDict.TryGetValue("refund_address", out var address))
            {
                return address.ToString() ?? string.Empty;
            }
        }
        catch
        {
            // Ignore deserialization errors
        }

        return string.Empty;
    }
}

// Coinbase API response models
public class CoinbaseRefundResponse
{
    public CoinbaseRefundData? Data { get; set; }
}

public class CoinbaseRefundData
{
    public string? Id { get; set; }
    public string? Resource { get; set; }
    public string? Resource_Path { get; set; }
    public string? Status { get; set; }
    public CoinbaseMoney? Amount { get; set; }
    public string? Description { get; set; }
    public DateTime Created_At { get; set; }
}

public class CoinbaseMoney
{
    public string? Amount { get; set; }
    public string? Currency { get; set; }
}

public class CoinbaseErrorResponse
{
    public List<CoinbaseError>? Errors { get; set; }
}

public class CoinbaseError
{
    public string? Id { get; set; }
    public string? Message { get; set; }
    public string? Url { get; set; }
}
