using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Enhanced deposit response with validation results and mobile features
/// </summary>
public class EnhancedDepositResponse : DepositResponse
{
    /// <summary>
    /// Gets or sets whether the request was successful
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the correlation ID for tracking
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the request timestamp
    /// </summary>
    public DateTime RequestTimestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the deposit code validation result
    /// </summary>
    public DepositCodeValidationResponse? DepositCodeValidation { get; set; }
    
    /// <summary>
    /// Gets or sets the generated deposit code (if auto-generated)
    /// </summary>
    public DepositCodeGenerationResponse? GeneratedDepositCode { get; set; }
    
    /// <summary>
    /// Gets or sets the deposit processing result
    /// </summary>
    public DepositProcessingResponse? DepositProcessing { get; set; }
    
    /// <summary>
    /// Gets or sets mobile-specific features
    /// </summary>
    public DepositMobileFeatures? MobileFeatures { get; set; }

    /// <summary>
    /// Gets or sets provider details for client capture or hosted payment flows (e.g., Square).
    /// </summary>
    public ProviderInfo? Provider { get; set; }
}

/// <summary>
/// Mobile-specific features for deposit operations
/// </summary>
public class DepositMobileFeatures
{
    /// <summary>
    /// Gets or sets the QR code data URL for scanning
    /// </summary>
    public string? QrCodeDataUrl { get; set; }
    
    /// <summary>
    /// Gets or sets whether offline processing is supported
    /// </summary>
    public bool OfflineSupported { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether biometric authentication is required
    /// </summary>
    public bool BiometricRequired { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether push notifications are enabled
    /// </summary>
    public bool PushNotificationsEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the deep link URL for mobile app integration
    /// </summary>
    public string? DeepLinkUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated sync time for offline operations
    /// </summary>
    public TimeSpan? OfflineSyncTime { get; set; }
}


/// <summary>
/// Deposit pre-validation response
/// </summary>
public class DepositPreValidationResponse
{
    /// <summary>
    /// Gets or sets the correlation ID
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether the deposit code is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets the validation details
    /// </summary>
    public DepositCodeValidationDetails? ValidationDetails { get; set; }
    
    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets suggested actions for the user
    /// </summary>
    public List<string> SuggestedActions { get; set; } = new();
    
    /// <summary>
    /// Gets or sets whether the user can proceed with the deposit
    /// </summary>
    public bool CanProceedWithDeposit { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated processing time
    /// </summary>
    public TimeSpan? EstimatedProcessingTime { get; set; }
    
    /// <summary>
    /// Gets or sets required verifications
    /// </summary>
    public List<string> RequiredVerifications { get; set; } = new();
    
    /// <summary>
    /// Gets or sets mobile-specific UI optimizations
    /// </summary>
    public MobileValidationOptimizations? MobileOptimizations { get; set; }
}

/// <summary>
/// Mobile-specific validation optimizations for UI
/// </summary>
public class MobileValidationOptimizations
{
    /// <summary>
    /// Gets or sets whether to show the amount input field
    /// </summary>
    public bool ShowAmountField { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to show the currency selector
    /// </summary>
    public bool ShowCurrencySelector { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to enable biometric authentication
    /// </summary>
    public bool EnableBiometricAuth { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to suggest offline mode
    /// </summary>
    public bool SuggestOfflineMode { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether to auto-focus the deposit code field
    /// </summary>
    public bool AutoFocusDepositCode { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to show QR scanner button
    /// </summary>
    public bool ShowQrScanner { get; set; } = true;
}

/// <summary>
/// Provider info returned to the mobile app for client-side capture or hosted payment flows.
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// Logical provider identifier (e.g., "SQUARE")
    /// </summary>
    public string ProviderId { get; set; } = string.Empty;

    /// <summary>
    /// Capture mode: "client" (SDK tokenization) or "payment_link" (hosted link).
    /// </summary>
    public string CaptureMode { get; set; } = "client";

    /// <summary>
    /// Arbitrary provider parameters (e.g., applicationId, amountMoney, environment, checkoutUrl).
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}
