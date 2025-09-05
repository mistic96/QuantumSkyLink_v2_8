using System.Text;
using System.Text.Json;
using MobileAPIGateway.Models.Wallet;
using MobileAPIGateway.Authentication;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Payment Gateway Service client implementation for mobile operations
/// </summary>
public class PaymentGatewayClient : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentGatewayClient> _logger;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public PaymentGatewayClient(
        HttpClient httpClient,
        ILogger<PaymentGatewayClient> logger,
        IUserContextAccessor userContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _userContextAccessor = userContextAccessor;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Generates a new deposit code for the user
    /// </summary>
    public async Task<DepositCodeGenerationResponse> GenerateDepositCodeAsync(DepositCodeGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var userContext = _userContextAccessor.GetUserContext();
        
        _logger.LogInformation("Generating deposit code for user. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userContext?.UserId);

        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payment/deposit-codes/generate", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<DepositCodeGenerationResponse>(responseContent, _jsonOptions);
                
                if (result != null)
                {
                    // Add mobile-specific features
                    result.MobileFeatures ??= new MobileFeatures
                    {
                        OfflineValidationSupported = true,
                        RequiresBiometric = request.Amount > 1000, // Require biometric for large amounts
                        DisplayFormat = FormatDepositCodeForDisplay(result.DepositCode),
                        PushSettings = new PushNotificationSettings
                        {
                            SendExpirationWarnings = true,
                            SendUsageConfirmations = true,
                            ExpirationWarningMinutes = 60
                        }
                    };
                    
                    _logger.LogInformation("Deposit code generated successfully. CorrelationId: {CorrelationId}", correlationId);
                    return result;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to generate deposit code. Status: {StatusCode}, Content: {Content}, CorrelationId: {CorrelationId}", 
                response.StatusCode, errorContent, correlationId);

            return new DepositCodeGenerationResponse
            {
                Success = false,
                ErrorMessage = $"Failed to generate deposit code: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating deposit code. CorrelationId: {CorrelationId}", correlationId);
            return new DepositCodeGenerationResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while generating the deposit code"
            };
        }
    }

    /// <summary>
    /// Validates a deposit code in real-time
    /// </summary>
    public async Task<DepositCodeValidationResponse> ValidateDepositCodeAsync(DepositCodeValidationRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var userContext = _userContextAccessor.GetUserContext();
        
        _logger.LogInformation("Validating deposit code. CorrelationId: {CorrelationId}, UserId: {UserId}, DepositCode: {DepositCode}", 
            correlationId, userContext?.UserId, request.DepositCode);

        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payment/deposit-codes/validate", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<DepositCodeValidationResponse>(responseContent, _jsonOptions);
                
                if (result != null)
                {
                    // Add mobile-specific suggested actions
                    if (result.IsValid)
                    {
                        result.SuggestedActions = new List<string>
                        {
                            "Proceed with deposit",
                            "Double-check amount and currency",
                            "Complete biometric verification if required"
                        };
                    }
                    else
                    {
                        result.SuggestedActions = GetValidationErrorSuggestions(result.ErrorMessage);
                    }
                    
                    _logger.LogInformation("Deposit code validation completed. CorrelationId: {CorrelationId}, IsValid: {IsValid}", 
                        correlationId, result.IsValid);
                    return result;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to validate deposit code. Status: {StatusCode}, Content: {Content}, CorrelationId: {CorrelationId}", 
                response.StatusCode, errorContent, correlationId);

            return new DepositCodeValidationResponse
            {
                IsValid = false,
                ErrorMessage = $"Validation service unavailable: {response.StatusCode}",
                ValidatedAt = DateTime.UtcNow,
                SuggestedActions = new List<string> { "Try again later", "Contact support if problem persists" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating deposit code. CorrelationId: {CorrelationId}", correlationId);
            return new DepositCodeValidationResponse
            {
                IsValid = false,
                ErrorMessage = "An error occurred during validation",
                ValidatedAt = DateTime.UtcNow,
                SuggestedActions = new List<string> { "Check your connection", "Try again", "Contact support" }
            };
        }
    }

    /// <summary>
    /// Processes a deposit with deposit code validation
    /// </summary>
    public async Task<DepositProcessingResponse> ProcessDepositWithCodeAsync(EnhancedDepositRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var userContext = _userContextAccessor.GetUserContext();
        
        _logger.LogInformation("Processing deposit with code. CorrelationId: {CorrelationId}, UserId: {UserId}, DepositCode: {DepositCode}", 
            correlationId, userContext?.UserId, request.DepositCode);

        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payment/deposit-codes/process", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<DepositProcessingResponse>(responseContent, _jsonOptions);
                
                if (result != null)
                {
                    // Add mobile-specific processing details
                    result.MobileDetails ??= new MobileProcessingDetails
                    {
                        OfflineProcessingAvailable = request.PaymentMethod == DepositPaymentMethod.Crypto,
                        NotificationSettings = new PushNotificationSettings
                        {
                            SendExpirationWarnings = true,
                            SendUsageConfirmations = true
                        },
                        OfflineSyncTime = request.PaymentMethod == DepositPaymentMethod.Crypto ? TimeSpan.FromMinutes(5) : null
                    };
                    
                    _logger.LogInformation("Deposit processing completed. CorrelationId: {CorrelationId}, Success: {Success}, Status: {Status}", 
                        correlationId, result.Success, result.Status);
                    return result;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Failed to process deposit. Status: {StatusCode}, Content: {Content}, CorrelationId: {CorrelationId}", 
                response.StatusCode, errorContent, correlationId);

            return new DepositProcessingResponse
            {
                Success = false,
                Status = DepositProcessingStatus.Failed,
                ErrorMessage = $"Processing failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit with code. CorrelationId: {CorrelationId}", correlationId);
            return new DepositProcessingResponse
            {
                Success = false,
                Status = DepositProcessingStatus.Failed,
                ErrorMessage = "An error occurred during deposit processing"
            };
        }
    }

    /// <summary>
    /// Gets deposit code status and details
    /// </summary>
    public async Task<DepositCodeStatusResponse> GetDepositCodeStatusAsync(string depositCode, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var userContext = _userContextAccessor.GetUserContext();
        
        _logger.LogInformation("Getting deposit code status. CorrelationId: {CorrelationId}, UserId: {UserId}, DepositCode: {DepositCode}", 
            correlationId, userContext?.UserId, depositCode);

        try
        {
            var response = await _httpClient.GetAsync($"/api/payment/deposit-codes/{depositCode}/status", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<DepositCodeStatusResponse>(responseContent, _jsonOptions);
                
                if (result != null)
                {
                    _logger.LogInformation("Deposit code status retrieved. CorrelationId: {CorrelationId}, Status: {Status}", 
                        correlationId, result.Status);
                    return result;
                }
            }

            _logger.LogWarning("Failed to get deposit code status. Status: {StatusCode}, CorrelationId: {CorrelationId}", 
                response.StatusCode, correlationId);

            return new DepositCodeStatusResponse
            {
                DepositCode = depositCode,
                Status = DepositCodeStatus.Suspended // Default to suspended if we can't get status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deposit code status. CorrelationId: {CorrelationId}", correlationId);
            return new DepositCodeStatusResponse
            {
                DepositCode = depositCode,
                Status = DepositCodeStatus.Suspended
            };
        }
    }

    /// <summary>
    /// Gets user's active deposit codes
    /// </summary>
    public async Task<IEnumerable<UserDepositCode>> GetUserActiveDepositCodesAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var userContext = _userContextAccessor.GetUserContext();
        
        _logger.LogInformation("Getting user active deposit codes. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userContext?.UserId);

        try
        {
            var response = await _httpClient.GetAsync("/api/payment/deposit-codes/active", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<IEnumerable<UserDepositCode>>(responseContent, _jsonOptions);
                
                if (result != null)
                {
                    _logger.LogInformation("Retrieved {Count} active deposit codes. CorrelationId: {CorrelationId}", 
                        result.Count(), correlationId);
                    return result;
                }
            }

            _logger.LogWarning("Failed to get active deposit codes. Status: {StatusCode}, CorrelationId: {CorrelationId}", 
                response.StatusCode, correlationId);

            return new List<UserDepositCode>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active deposit codes. CorrelationId: {CorrelationId}", correlationId);
            return new List<UserDepositCode>();
        }
    }

    /// <summary>
    /// Revokes/expires a deposit code
    /// </summary>
    public async Task<DepositCodeRevocationResponse> RevokeDepositCodeAsync(string depositCode, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var userContext = _userContextAccessor.GetUserContext();
        
        _logger.LogInformation("Revoking deposit code. CorrelationId: {CorrelationId}, UserId: {UserId}, DepositCode: {DepositCode}", 
            correlationId, userContext?.UserId, depositCode);

        try
        {
            var response = await _httpClient.DeleteAsync($"/api/payment/deposit-codes/{depositCode}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<DepositCodeRevocationResponse>(responseContent, _jsonOptions);
                
                if (result != null)
                {
                    _logger.LogInformation("Deposit code revoked successfully. CorrelationId: {CorrelationId}", correlationId);
                    return result;
                }
            }

            _logger.LogWarning("Failed to revoke deposit code. Status: {StatusCode}, CorrelationId: {CorrelationId}", 
                response.StatusCode, correlationId);

            return new DepositCodeRevocationResponse
            {
                Success = false,
                DepositCode = depositCode,
                ErrorMessage = $"Revocation failed: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking deposit code. CorrelationId: {CorrelationId}", correlationId);
            return new DepositCodeRevocationResponse
            {
                Success = false,
                DepositCode = depositCode,
                ErrorMessage = "An error occurred during revocation"
            };
        }
    }

    // Provider helpers (Square)

    public async Task<Dictionary<string, object>> GetSquareClientParamsAsync(decimal amount, string currency, string? referenceId = null, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            var url = $"/api/providers/square/client-params?amount={Uri.EscapeDataString(amount.ToString(System.Globalization.CultureInfo.InvariantCulture))}&currency={Uri.EscapeDataString(currency)}";
            if (!string.IsNullOrWhiteSpace(referenceId))
            {
                url += $"&referenceId={Uri.EscapeDataString(referenceId)}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetSquareClientParams failed. Status: {Status}, Body: {Body}, CorrelationId: {CorrelationId}", response.StatusCode, content, correlationId);
                return new Dictionary<string, object>();
            }

            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(content, _jsonOptions) ?? new Dictionary<string, object>();
            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSquareClientParamsAsync error. CorrelationId: {CorrelationId}", correlationId);
            return new Dictionary<string, object>();
        }
    }

    public async Task<(string? CheckoutUrl, DateTime? ExpiresAt, string? ReferenceId, string? Error)> CreateSquarePaymentLinkAsync(decimal amount, string currency, string? referenceId = null, string? email = null, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            var body = new
            {
                amount,
                currency,
                referenceId,
                email
            };

            var json = JsonSerializer.Serialize(body, _jsonOptions);
            var req = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/providers/square/payment-link", req, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CreateSquarePaymentLink failed. Status: {Status}, Body: {Body}, CorrelationId: {CorrelationId}", response.StatusCode, content, correlationId);
                return (null, null, referenceId, $"Upstream status {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var url = root.TryGetProperty("checkoutUrl", out var urlEl) ? urlEl.GetString() : null;
            var refId = root.TryGetProperty("referenceId", out var refEl) ? refEl.GetString() : referenceId;
            DateTime? expires = null;
            if (root.TryGetProperty("expiresAt", out var expEl) && expEl.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(expEl.GetString(), out var parsed)) expires = parsed;
            }

            return (url, expires, refId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSquarePaymentLinkAsync error. CorrelationId: {CorrelationId}", correlationId);
            return (null, null, referenceId, ex.Message);
        }
    }

    /// <summary>
    /// Formats deposit code for better mobile display
    /// </summary>
    private static string FormatDepositCodeForDisplay(string depositCode)
    {
        if (string.IsNullOrEmpty(depositCode) || depositCode.Length != 8)
            return depositCode;

        return $"{depositCode.Substring(0, 4)}-{depositCode.Substring(4, 4)}";
    }

    /// <summary>
    /// Gets suggested actions based on validation error message
    /// </summary>
    private static List<string> GetValidationErrorSuggestions(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return new List<string> { "Check the deposit code and try again" };

        var suggestions = new List<string>();
        var lowerError = errorMessage.ToLowerInvariant();

        if (lowerError.Contains("expired"))
        {
            suggestions.AddRange(new[]
            {
                "Generate a new deposit code",
                "Contact support if you need to extend the code"
            });
        }
        else if (lowerError.Contains("invalid") || lowerError.Contains("not found"))
        {
            suggestions.AddRange(new[]
            {
                "Double-check the deposit code",
                "Make sure you entered all 8 characters",
                "Scan the QR code again if available"
            });
        }
        else if (lowerError.Contains("amount"))
        {
            suggestions.AddRange(new[]
            {
                "Check the expected deposit amount",
                "Verify the currency matches"
            });
        }
        else if (lowerError.Contains("used"))
        {
            suggestions.AddRange(new[]
            {
                "This code has already been used",
                "Generate a new deposit code if needed"
            });
        }
        else
        {
            suggestions.Add("Contact support for assistance");
        }

        return suggestions;
    }
}
