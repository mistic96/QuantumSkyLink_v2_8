using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaymentGatewayService.Models.MoonPay;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Implementation of MoonPay fiat-to-crypto and crypto-to-fiat service
/// </summary>
public class MoonPayService : IMoonPayService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MoonPayService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _apiKey;
    private readonly string _secretKey;
    private readonly string _webhookSecret;

    public MoonPayService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MoonPayService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["MoonPay:ApiKey"] ?? string.Empty;
        _secretKey = configuration["MoonPay:SecretKey"] ?? string.Empty;
        _webhookSecret = configuration["MoonPay:WebhookSecret"] ?? string.Empty;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Creates a fiat-to-crypto transaction (on-ramp)
    /// </summary>
    public async Task<MoonPayTransactionResponse> CreateBuyTransactionAsync(MoonPayBuyRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating MoonPay buy transaction. CorrelationId: {CorrelationId}, Currency: {Currency}, Amount: {Amount} {BaseCurrency}", 
            correlationId, request.CurrencyCode, request.BaseCurrencyAmount, request.BaseCurrencyCode);

        try
        {
            // Set external transaction ID if not provided
            if (string.IsNullOrEmpty(request.ExternalTransactionId))
            {
                request.ExternalTransactionId = correlationId;
            }

            // Add API key to request
            var requestWithKey = new
            {
                apiKey = _apiKey,
                currencyCode = request.CurrencyCode,
                baseCurrencyAmount = request.BaseCurrencyAmount,
                walletAddress = request.WalletAddress,
                baseCurrencyCode = request.BaseCurrencyCode,
                areFeesIncluded = request.AreFeesIncluded,
                externalTransactionId = request.ExternalTransactionId,
                email = request.Email,
                returnUrl = request.ReturnUrl,
                theme = request.Theme,
                paymentMethod = request.PaymentMethod
            };

            var json = JsonSerializer.Serialize(requestWithKey, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("transactions", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MoonPay buy transaction creation failed. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"MoonPay transaction creation failed: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<MoonPayTransactionResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("MoonPay buy transaction created successfully. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}, Status: {Status}", 
                correlationId, result?.Id, result?.Status);

            return result ?? throw new InvalidOperationException("Failed to deserialize MoonPay response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoonPay buy transaction. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Creates a crypto-to-fiat transaction (off-ramp)
    /// </summary>
    public async Task<MoonPayTransactionResponse> CreateSellTransactionAsync(MoonPaySellRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating MoonPay sell transaction. CorrelationId: {CorrelationId}, Currency: {Currency}, Amount: {Amount}", 
            correlationId, request.BaseCurrencyCode, request.BaseCurrencyAmount);

        try
        {
            // Set external transaction ID if not provided
            if (string.IsNullOrEmpty(request.ExternalTransactionId))
            {
                request.ExternalTransactionId = correlationId;
            }

            var requestWithKey = new
            {
                apiKey = _apiKey,
                baseCurrencyCode = request.BaseCurrencyCode,
                baseCurrencyAmount = request.BaseCurrencyAmount,
                quoteCurrencyCode = request.QuoteCurrencyCode,
                externalTransactionId = request.ExternalTransactionId,
                bankAccount = request.BankAccount,
                email = request.Email
            };

            var json = JsonSerializer.Serialize(requestWithKey, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("sell_transactions", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MoonPay sell transaction creation failed. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"MoonPay sell transaction failed: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<MoonPayTransactionResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("MoonPay sell transaction created successfully. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}", 
                correlationId, result?.Id);

            return result ?? throw new InvalidOperationException("Failed to deserialize MoonPay response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoonPay sell transaction. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the status of a transaction
    /// </summary>
    public async Task<MoonPayTransactionStatus> GetTransactionStatusAsync(string transactionId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting MoonPay transaction status. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}", 
            correlationId, transactionId);

        try
        {
            var response = await _httpClient.GetAsync($"transactions/{transactionId}?apiKey={_apiKey}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get MoonPay transaction status. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"Failed to get transaction status: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MoonPayTransactionStatus>(responseContent, _jsonOptions);
            
            _logger.LogInformation("MoonPay transaction status retrieved. CorrelationId: {CorrelationId}, Status: {Status}", 
                correlationId, result?.Status);

            return result ?? throw new InvalidOperationException("Failed to deserialize MoonPay response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay transaction status. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets available cryptocurrencies for trading
    /// </summary>
    public async Task<List<MoonPayCurrency>> GetSupportedCurrenciesAsync(string? fiatCurrency = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting MoonPay supported currencies. CorrelationId: {CorrelationId}, FiatCurrency: {FiatCurrency}", 
            correlationId, fiatCurrency);

        try
        {
            var url = $"currencies?apiKey={_apiKey}";
            if (!string.IsNullOrEmpty(fiatCurrency))
            {
                url += $"&baseCurrencyCode={fiatCurrency}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get MoonPay currencies. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"Failed to get currencies: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<List<MoonPayCurrency>>(responseContent, _jsonOptions);
            
            _logger.LogInformation("Retrieved {Count} MoonPay currencies. CorrelationId: {CorrelationId}", 
                result?.Count ?? 0, correlationId);

            return result ?? new List<MoonPayCurrency>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay currencies. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets current exchange rates
    /// </summary>
    public async Task<MoonPayQuote> GetQuoteAsync(string cryptoCurrency, string fiatCurrency)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting MoonPay quote. CorrelationId: {CorrelationId}, Crypto: {Crypto}, Fiat: {Fiat}", 
            correlationId, cryptoCurrency, fiatCurrency);

        try
        {
            var url = $"buy_quote?apiKey={_apiKey}&currencyCode={cryptoCurrency}&baseCurrencyCode={fiatCurrency}&baseCurrencyAmount=100";

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get MoonPay quote. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"Failed to get quote: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MoonPayQuote>(responseContent, _jsonOptions);
            
            _logger.LogInformation("MoonPay quote retrieved. CorrelationId: {CorrelationId}, Rate: {Rate}", 
                correlationId, result?.ExchangeRate);

            return result ?? throw new InvalidOperationException("Failed to deserialize MoonPay response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay quote. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Processes webhook notifications from MoonPay
    /// </summary>
    public async Task<bool> ProcessWebhookAsync(MoonPayWebhookPayload payload, string signature)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Processing MoonPay webhook. CorrelationId: {CorrelationId}, Type: {Type}, EventId: {EventId}", 
            correlationId, payload.Type, payload.EventId);

        try
        {
            // Verify webhook signature
            if (!VerifyWebhookSignature(payload, signature))
            {
                _logger.LogWarning("MoonPay webhook signature verification failed. CorrelationId: {CorrelationId}", correlationId);
                return false;
            }

            // Process based on event type
            switch (payload.Type.ToLower())
            {
                case "transaction_created":
                case "transaction_updated":
                case "transaction_failed":
                    await ProcessTransactionWebhookAsync(payload);
                    break;
                    
                case "customer_created":
                case "customer_updated":
                    await ProcessCustomerWebhookAsync(payload);
                    break;
                    
                default:
                    _logger.LogWarning("Unknown MoonPay webhook event type: {EventType}", payload.Type);
                    break;
            }

            _logger.LogInformation("MoonPay webhook processed successfully. CorrelationId: {CorrelationId}", correlationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoonPay webhook. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
    }

    /// <summary>
    /// Creates a widget session for embedded UI
    /// </summary>
    public async Task<MoonPayWidgetResponse> CreateWidgetSessionAsync(MoonPayWidgetRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating MoonPay widget session. CorrelationId: {CorrelationId}, Flow: {Flow}", 
            correlationId, request.Flow);

        try
        {
            // Build widget URL with signed parameters
            var queryParams = new Dictionary<string, string>
            {
                ["apiKey"] = _apiKey,
                ["flow"] = request.Flow
            };

            if (!string.IsNullOrEmpty(request.DefaultCurrencyCode))
                queryParams["currencyCode"] = request.DefaultCurrencyCode;
            
            if (request.DefaultBaseCurrencyAmount.HasValue)
                queryParams["baseCurrencyAmount"] = request.DefaultBaseCurrencyAmount.Value.ToString();
            
            if (!string.IsNullOrEmpty(request.WalletAddress))
                queryParams["walletAddress"] = request.WalletAddress;
            
            if (!string.IsNullOrEmpty(request.Email))
                queryParams["email"] = request.Email;
            
            if (!string.IsNullOrEmpty(request.ExternalCustomerId))
                queryParams["externalCustomerId"] = request.ExternalCustomerId;
            
            if (!string.IsNullOrEmpty(request.RedirectUrl))
                queryParams["redirectURL"] = request.RedirectUrl;

            // Create signature for URL
            var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            var signature = GenerateSignature(queryString);
            queryParams["signature"] = signature;

            var widgetUrl = $"https://buy.moonpay.com?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))}";

            var result = new MoonPayWidgetResponse
            {
                Url = widgetUrl,
                SessionId = correlationId,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _logger.LogInformation("MoonPay widget session created. CorrelationId: {CorrelationId}", correlationId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoonPay widget session. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Performs KYC/AML screening for a recipient
    /// </summary>
    public async Task<MoonPayScreeningResult> ScreenRecipientAsync(MoonPayScreeningRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Screening MoonPay recipient. CorrelationId: {CorrelationId}, Email: {Email}", 
            correlationId, request.Email);

        try
        {
            var requestWithKey = new
            {
                apiKey = _apiKey,
                email = request.Email,
                ipAddress = request.IpAddress,
                name = request.Name,
                dateOfBirth = request.DateOfBirth,
                country = request.Country,
                state = request.State
            };

            var json = JsonSerializer.Serialize(requestWithKey, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("customers/screening", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MoonPay recipient screening failed. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"Recipient screening failed: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MoonPayScreeningResult>(responseContent, _jsonOptions);
            
            _logger.LogInformation("MoonPay recipient screening completed. CorrelationId: {CorrelationId}, Passed: {Passed}", 
                correlationId, result?.IsPassed);

            return result ?? new MoonPayScreeningResult { IsPassed = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error screening MoonPay recipient. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets transaction limits for a currency pair
    /// </summary>
    public async Task<MoonPayLimits> GetTransactionLimitsAsync(string cryptoCurrency, string fiatCurrency, string paymentMethod)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting MoonPay transaction limits. CorrelationId: {CorrelationId}, Crypto: {Crypto}, Fiat: {Fiat}, Method: {Method}", 
            correlationId, cryptoCurrency, fiatCurrency, paymentMethod);

        try
        {
            var url = $"limits?apiKey={_apiKey}&currencyCode={cryptoCurrency}&baseCurrencyCode={fiatCurrency}&paymentMethod={paymentMethod}";

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get MoonPay limits. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"Failed to get limits: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<MoonPayLimits>(responseContent, _jsonOptions);
            
            _logger.LogInformation("MoonPay limits retrieved. CorrelationId: {CorrelationId}, Min: {Min}, Max: {Max}", 
                correlationId, result?.MinAmount, result?.MaxAmount);

            return result ?? throw new InvalidOperationException("Failed to deserialize MoonPay response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay limits. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Verifies webhook signature using HMAC-SHA256
    /// </summary>
    private bool VerifyWebhookSignature(MoonPayWebhookPayload payload, string signature)
    {
        if (string.IsNullOrEmpty(_webhookSecret) || string.IsNullOrEmpty(signature))
        {
            return false;
        }

        try
        {
            var payloadJson = JsonSerializer.Serialize(payload, _jsonOptions);
            var encoding = new UTF8Encoding();
            var keyBytes = encoding.GetBytes(_webhookSecret);
            var messageBytes = encoding.GetBytes(payloadJson);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            var computedSignature = Convert.ToBase64String(hashBytes);

            return signature == computedSignature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MoonPay webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Generates signature for widget URL
    /// </summary>
    private string GenerateSignature(string queryString)
    {
        var encoding = new UTF8Encoding();
        var keyBytes = encoding.GetBytes(_secretKey);
        var messageBytes = encoding.GetBytes(queryString);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Processes transaction webhook events
    /// </summary>
    private async Task ProcessTransactionWebhookAsync(MoonPayWebhookPayload payload)
    {
        await Task.CompletedTask; // Placeholder for actual implementation
        
        // Extract transaction ID and status from payload.Data
        // Update payment record in database
        // Send notifications if needed
        
        _logger.LogInformation("Processed transaction webhook for transaction: {TransactionId}", payload.Data.Id);
    }

    /// <summary>
    /// Processes customer webhook events
    /// </summary>
    private async Task ProcessCustomerWebhookAsync(MoonPayWebhookPayload payload)
    {
        await Task.CompletedTask; // Placeholder for actual implementation
        
        // Update customer verification status
        // Enable/disable features based on KYC status
        
        _logger.LogInformation("Processed customer webhook for event: {Event}", payload.Type);
    }

    #endregion
}