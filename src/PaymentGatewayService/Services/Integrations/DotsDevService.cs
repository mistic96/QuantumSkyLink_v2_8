using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaymentGatewayService.Models.DotsDev;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Implementation of Dots.dev payout service supporting 190+ countries
/// </summary>
public class DotsDevService : IDotsDevService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DotsDevService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _webhookSecret;

    public DotsDevService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<DotsDevService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _webhookSecret = configuration["DotsDev:WebhookSecret"] ?? string.Empty;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Creates a payout to recipients in 190+ countries
    /// </summary>
    public async Task<DotsDevPayoutResponse> CreatePayoutAsync(DotsDevPayoutRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating Dots.dev payout. CorrelationId: {CorrelationId}, Amount: {Amount} {Currency}, Country: {Country}", 
            correlationId, request.AmountInCents, request.Currency, request.Recipient.Country);

        try
        {
            // Set idempotency key if not provided
            if (string.IsNullOrEmpty(request.IdempotencyKey))
            {
                request.IdempotencyKey = correlationId;
            }

            // Auto-select payment method based on country if not specified
            if (string.IsNullOrEmpty(request.Recipient.PaymentMethod))
            {
                request.Recipient.PaymentMethod = GetDefaultPaymentMethod(request.Recipient.Country);
            }

            // Prepare request
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add idempotency header
            _httpClient.DefaultRequestHeaders.Add("Idempotency-Key", request.IdempotencyKey);

            // Send request
            var response = await _httpClient.PostAsync("payouts", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Remove idempotency header for next request
            _httpClient.DefaultRequestHeaders.Remove("Idempotency-Key");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Dots.dev payout creation failed. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"Dots.dev payout creation failed: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<DotsDevPayoutResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("Dots.dev payout created successfully. CorrelationId: {CorrelationId}, PayoutId: {PayoutId}, Method: {Method}", 
                correlationId, result?.Id, result?.PaymentMethod);

            return result ?? throw new InvalidOperationException("Failed to deserialize Dots.dev response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Dots.dev payout. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Creates an onboarding flow for recipient verification
    /// </summary>
    public async Task<DotsDevFlowResponse> CreateOnboardingFlowAsync(DotsDevFlowRequest request)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating Dots.dev onboarding flow. CorrelationId: {CorrelationId}, FlowType: {FlowType}", 
            correlationId, request.FlowType);

        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("flows", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Dots.dev flow creation failed. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"Dots.dev flow creation failed: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<DotsDevFlowResponse>(responseContent, _jsonOptions);
            
            _logger.LogInformation("Dots.dev flow created successfully. CorrelationId: {CorrelationId}, FlowId: {FlowId}", 
                correlationId, result?.FlowId);

            return result ?? throw new InvalidOperationException("Failed to deserialize Dots.dev response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Dots.dev flow. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets the status of an onboarding flow
    /// </summary>
    public async Task<DotsDevFlowStatus> GetFlowStatusAsync(string flowId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting Dots.dev flow status. CorrelationId: {CorrelationId}, FlowId: {FlowId}", 
            correlationId, flowId);

        try
        {
            var response = await _httpClient.GetAsync($"flows/{flowId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Dots.dev flow status. CorrelationId: {CorrelationId}, Status: {Status}, Response: {Response}", 
                    correlationId, response.StatusCode, responseContent);
                
                throw new HttpRequestException($"Failed to get flow status: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<DotsDevFlowStatus>(responseContent, _jsonOptions);
            
            _logger.LogInformation("Dots.dev flow status retrieved. CorrelationId: {CorrelationId}, Status: {Status}, Completion: {Completion}%", 
                correlationId, result?.Status, result?.CompletionPercentage);

            return result ?? throw new InvalidOperationException("Failed to deserialize Dots.dev response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Dots.dev flow status. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Processes webhook notifications from Dots.dev
    /// </summary>
    public async Task<bool> ProcessWebhookAsync(DotsDevWebhookPayload payload, string signature)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Processing Dots.dev webhook. CorrelationId: {CorrelationId}, Event: {Event}, EventId: {EventId}", 
            correlationId, payload.Event, payload.EventId);

        try
        {
            // Verify webhook signature
            if (!VerifyWebhookSignature(payload, signature))
            {
                _logger.LogWarning("Dots.dev webhook signature verification failed. CorrelationId: {CorrelationId}", correlationId);
                return false;
            }

            // Process based on event type
            switch (payload.Event.ToLower())
            {
                case "payout.completed":
                case "payout.processing":
                case "payout.failed":
                    await ProcessPayoutWebhookAsync(payload);
                    break;
                    
                case "flow.completed":
                case "flow.abandoned":
                    await ProcessFlowWebhookAsync(payload);
                    break;
                    
                default:
                    _logger.LogWarning("Unknown Dots.dev webhook event type: {EventType}", payload.Event);
                    break;
            }

            _logger.LogInformation("Dots.dev webhook processed successfully. CorrelationId: {CorrelationId}", correlationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Dots.dev webhook. CorrelationId: {CorrelationId}", correlationId);
            return false;
        }
    }

    /// <summary>
    /// Gets the list of supported countries and payment methods
    /// </summary>
    public async Task<List<DotsDevCountrySupport>> GetSupportedCountriesAsync()
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting Dots.dev supported countries. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            var response = await _httpClient.GetAsync("countries");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get Dots.dev supported countries. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"Failed to get supported countries: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<List<DotsDevCountrySupport>>(responseContent, _jsonOptions);
            
            _logger.LogInformation("Retrieved {Count} supported countries from Dots.dev. CorrelationId: {CorrelationId}", 
                result?.Count ?? 0, correlationId);

            return result ?? new List<DotsDevCountrySupport>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Dots.dev supported countries. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Validates recipient details for a specific country
    /// </summary>
    public async Task<DotsDevValidationResult> ValidateRecipientAsync(string country, Dictionary<string, object> recipientData)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Validating recipient for country {Country}. CorrelationId: {CorrelationId}", country, correlationId);

        try
        {
            var request = new
            {
                country = country,
                recipient_data = recipientData
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("validate/recipient", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Recipient validation failed. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, response.StatusCode);
                
                throw new HttpRequestException($"Recipient validation failed: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<DotsDevValidationResult>(responseContent, _jsonOptions);
            
            _logger.LogInformation("Recipient validation completed. CorrelationId: {CorrelationId}, IsValid: {IsValid}", 
                correlationId, result?.IsValid);

            return result ?? new DotsDevValidationResult { IsValid = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating recipient. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Gets the default payment method for a country
    /// </summary>
    private string GetDefaultPaymentMethod(string country)
    {
        return country.ToUpper() switch
        {
            "US" => "ACH",
            "GB" or "DE" or "FR" or "IT" or "ES" => "SEPA",
            "IN" => "UPI",
            "PH" => "GCASH",
            "BR" => "PIX",
            "MX" => "SPEI",
            "CA" => "INTERAC",
            "AU" => "BPAY",
            "JP" => "BANK_TRANSFER",
            _ => "BANK_TRANSFER" // Default fallback
        };
    }

    /// <summary>
    /// Verifies webhook signature
    /// </summary>
    private bool VerifyWebhookSignature(DotsDevWebhookPayload payload, string signature)
    {
        if (string.IsNullOrEmpty(_webhookSecret) || string.IsNullOrEmpty(signature))
        {
            return false;
        }

        try
        {
            // Dots.dev uses HMAC-SHA256 for webhook signatures
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
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Processes payout webhook events
    /// </summary>
    private async Task ProcessPayoutWebhookAsync(DotsDevWebhookPayload payload)
    {
        await Task.CompletedTask; // Placeholder for actual implementation
        
        // Extract payout ID and status from payload.Data
        // Update payment record in database
        // Send notifications if needed
        
        _logger.LogInformation("Processed payout webhook for event: {Event}", payload.Event);
    }

    /// <summary>
    /// Processes flow webhook events
    /// </summary>
    private async Task ProcessFlowWebhookAsync(DotsDevWebhookPayload payload)
    {
        await Task.CompletedTask; // Placeholder for actual implementation
        
        // Extract flow ID and completion data from payload.Data
        // Update user verification status
        // Trigger any follow-up actions
        
        _logger.LogInformation("Processed flow webhook for event: {Event}", payload.Event);
    }

    #endregion
}