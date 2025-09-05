using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentGatewayService.Models.PIXBrazil;
using PaymentGatewayService.Utils;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Implementation of PIX Brazil payment integration service
/// </summary>
public class PIXBrazilService : IPIXBrazilService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PIXBrazilService> _logger;
    private readonly ICPFValidator _cpfValidator;
    private readonly string _webhookSecret;
    private readonly JsonSerializerOptions _jsonOptions;

    public PIXBrazilService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PIXBrazilService> logger,
        ICPFValidator cpfValidator)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _cpfValidator = cpfValidator;
        _webhookSecret = _configuration["PIXBrazil:WebhookSecret"] ?? string.Empty;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Creates a PIX payout (money out) transaction
    /// </summary>
    public async Task<PIXTransactionResponse> CreatePayoutAsync(PIXPayoutRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating PIX payout. CorrelationId: {CorrelationId}, Amount: {Amount}, PixKey: {PixKey}", 
            correlationId, request.AmountInCents, request.TargetPixKey);

        try
        {
            // Validate CPF/CNPJ if provided
            if (!string.IsNullOrEmpty(request.TargetDocument))
            {
                if (!_cpfValidator.IsValid(request.TargetDocument))
                {
                    throw new ArgumentException("Invalid CPF/CNPJ document");
                }
            }

            // Validate PIX key
            if (!await ValidatePixKeyAsync(request.TargetPixKey, request.TargetPixKeyType))
            {
                throw new ArgumentException($"Invalid PIX key: {request.TargetPixKey}");
            }

            // Ensure idempotency key
            if (string.IsNullOrEmpty(request.IdempotencyKey))
            {
                request.IdempotencyKey = Guid.NewGuid().ToString();
            }

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("payouts/pix", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PIX payout creation failed. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"PIX payout creation failed: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<PIXTransactionResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("PIX payout created successfully. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}", 
                correlationId, result?.Id);

            return result ?? throw new InvalidOperationException("Failed to deserialize PIX response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PIX payout. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Creates a PIX charge (money in) transaction with QR code
    /// </summary>
    public async Task<PIXTransactionResponse> CreateChargeAsync(PIXChargeRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating PIX charge. CorrelationId: {CorrelationId}, Amount: {Amount}, PayerDocument: {PayerDocument}", 
            correlationId, request.Amount, request.Payer?.Document);

        try
        {
            // Validate payer CPF/CNPJ
            if (request.Payer != null && !string.IsNullOrEmpty(request.Payer.Document))
            {
                if (!_cpfValidator.IsValid(request.Payer.Document))
                {
                    throw new ArgumentException("Invalid payer CPF/CNPJ document");
                }
            }

            // Set default expiration if not provided
            if (request.ExpirationInfo == null)
            {
                var defaultExpiration = _configuration.GetValue<int>("PIXBrazil:DefaultExpirationSeconds", 86400);
                request.ExpirationInfo = new PIXExpirationInfo { Seconds = defaultExpiration };
            }

            // Validate description length
            if (!string.IsNullOrEmpty(request.Description))
            {
                var maxLength = _configuration.GetValue<int>("PIXBrazil:MaxDescriptionLength", 43);
                if (request.Description.Length > maxLength)
                {
                    request.Description = request.Description.Substring(0, maxLength);
                }
            }

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("charges", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PIX charge creation failed. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"PIX charge creation failed: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<PIXTransactionResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("PIX charge created successfully. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}, QRCode: {HasQRCode}", 
                correlationId, result?.Id, !string.IsNullOrEmpty(result?.QRCode));

            return result ?? throw new InvalidOperationException("Failed to deserialize PIX response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PIX charge. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the current status of a PIX transaction
    /// </summary>
    public async Task<PIXTransactionResponse> GetTransactionStatusAsync(string transactionId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting PIX transaction status. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}", 
            correlationId, transactionId);

        try
        {
            var response = await _httpClient.GetAsync($"transactions/{transactionId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get PIX transaction status. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"Failed to get transaction status: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<PIXTransactionResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("PIX transaction status retrieved. CorrelationId: {CorrelationId}, Status: {Status}", 
                correlationId, result?.Status);

            return result ?? throw new InvalidOperationException("Failed to deserialize PIX response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PIX transaction status. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Processes webhook notifications from PIX provider
    /// </summary>
    public async Task<bool> ProcessWebhookAsync(PIXWebhookPayload payload)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Processing PIX webhook. CorrelationId: {CorrelationId}, Type: {Type}, TransactionId: {TransactionId}", 
            correlationId, payload.Type, payload.Data?.Id);

        try
        {
            // Verify webhook signature
            if (!VerifyWebhookSignature(payload))
            {
                _logger.LogWarning("Invalid webhook signature. CorrelationId: {CorrelationId}", correlationId);
                return false;
            }

            // Check timestamp to prevent replay attacks
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeDifference = Math.Abs(currentTimestamp - payload.Timestamp);
            
            if (timeDifference > 300) // 5 minutes tolerance
            {
                _logger.LogWarning("Webhook timestamp too old. CorrelationId: {CorrelationId}, TimeDifference: {TimeDifference}", 
                    correlationId, timeDifference);
                return false;
            }

            // Process based on webhook type
            switch (payload.Type?.ToLower())
            {
                case "payment.completed":
                case "payment.success":
                    _logger.LogInformation("PIX payment completed. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}", 
                        correlationId, payload.Data?.Id);
                    break;
                    
                case "payment.failed":
                case "payment.cancelled":
                    _logger.LogInformation("PIX payment failed/cancelled. CorrelationId: {CorrelationId}, TransactionId: {TransactionId}", 
                        correlationId, payload.Data?.Id);
                    break;
                    
                default:
                    _logger.LogInformation("Unhandled webhook type. CorrelationId: {CorrelationId}, Type: {Type}", 
                        correlationId, payload.Type);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PIX webhook. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
    }

    /// <summary>
    /// Validates a PIX key (CPF, email, phone, or random)
    /// </summary>
    public async Task<bool> ValidatePixKeyAsync(string pixKey, string keyType)
    {
        if (string.IsNullOrWhiteSpace(pixKey) || string.IsNullOrWhiteSpace(keyType))
        {
            return false;
        }

        switch (keyType.ToLower())
        {
            case "cpf":
            case "cnpj":
                return _cpfValidator.IsValid(pixKey);
                
            case "email":
                return IsValidEmail(pixKey);
                
            case "phone":
                return IsValidPhoneNumber(pixKey);
                
            case "random":
                return IsValidRandomKey(pixKey);
                
            default:
                return false;
        }
    }

    /// <summary>
    /// Generates a static PIX QR code for receiving payments
    /// </summary>
    public async Task<PIXQRCodeResponse> GenerateStaticQRCodeAsync(int? amount = null, string description = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Generating static PIX QR code. CorrelationId: {CorrelationId}, Amount: {Amount}", 
            correlationId, amount);

        try
        {
            var request = new
            {
                paymentMethod = "PIX_STATIC_QR",
                amount = amount,
                description = description?.Substring(0, Math.Min(description.Length, 43))
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("qrcodes/static", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Static QR code generation failed. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"QR code generation failed: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<PIXQRCodeResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("Static QR code generated successfully. CorrelationId: {CorrelationId}", correlationId);

            return result ?? throw new InvalidOperationException("Failed to deserialize QR code response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating static QR code. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    #region Private Helper Methods

    private bool VerifyWebhookSignature(PIXWebhookPayload payload)
    {
        if (string.IsNullOrEmpty(payload.Signature) || string.IsNullOrEmpty(_webhookSecret))
        {
            return false;
        }

        try
        {
            // Create the signature payload
            var signaturePayload = $"{payload.Timestamp}.{payload.Type}.{JsonSerializer.Serialize(payload.Data, _jsonOptions)}";
            
            // Calculate HMAC-SHA256
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signaturePayload));
            var computedSignature = Convert.ToBase64String(computedHash);
            
            return payload.Signature == computedSignature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhoneNumber(string phone)
    {
        // Brazilian phone format: +55 XX XXXXX-XXXX or variations
        var cleanPhone = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");
        
        // Check if it's a valid Brazilian phone number (with or without country code)
        if (cleanPhone.StartsWith("55"))
        {
            cleanPhone = cleanPhone.Substring(2);
        }
        
        // Should be 10 or 11 digits (with area code)
        return cleanPhone.Length >= 10 && cleanPhone.Length <= 11;
    }

    private bool IsValidRandomKey(string key)
    {
        // PIX random keys are UUIDs
        return Guid.TryParse(key, out _);
    }

    #endregion
}