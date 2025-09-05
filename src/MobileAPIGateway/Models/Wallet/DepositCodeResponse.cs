using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Deposit code generation response model
/// </summary>
public class DepositCodeGenerationResponse
{
    /// <summary>
    /// Gets or sets the generated deposit code
    /// </summary>
    public string DepositCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the expiration time
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets whether the code was successfully generated
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the QR code data URL for mobile scanning
    /// </summary>
    public string? QrCodeDataUrl { get; set; }
    
    /// <summary>
    /// Gets or sets mobile-specific features
    /// </summary>
    public MobileFeatures? MobileFeatures { get; set; }
}

/// <summary>
/// Deposit code validation response model
/// </summary>
public class DepositCodeValidationResponse
{
    /// <summary>
    /// Gets or sets whether the code is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets validation details
    /// </summary>
    public DepositCodeValidationDetails? ValidationDetails { get; set; }
    
    /// <summary>
    /// Gets or sets any validation error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the validation timestamp
    /// </summary>
    public DateTime ValidatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets suggested next steps for the user
    /// </summary>
    public List<string>? SuggestedActions { get; set; }
}

/// <summary>
/// Deposit code validation details
/// </summary>
public class DepositCodeValidationDetails
{
    /// <summary>
    /// Gets or sets the expected amount for this code
    /// </summary>
    public decimal? ExpectedAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the expected currency
    /// </summary>
    public string? ExpectedCurrency { get; set; }
    
    /// <summary>
    /// Gets or sets the code expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets whether the code is user-specific
    /// </summary>
    public bool IsUserSpecific { get; set; }
    
    /// <summary>
    /// Gets or sets the remaining time until expiration
    /// </summary>
    public TimeSpan? TimeUntilExpiration { get; set; }
}

/// <summary>
/// Mobile-specific features for deposit codes
/// </summary>
public class MobileFeatures
{
    /// <summary>
    /// Gets or sets whether offline validation is supported
    /// </summary>
    public bool OfflineValidationSupported { get; set; } = false;
    
    /// <summary>
    /// Gets or sets biometric verification requirement
    /// </summary>
    public bool RequiresBiometric { get; set; } = false;
    
    /// <summary>
    /// Gets or sets push notification settings
    /// </summary>
    public PushNotificationSettings? PushSettings { get; set; }
    
    /// <summary>
    /// Gets or sets the code display format for better UX
    /// </summary>
    public string DisplayFormat { get; set; } = "XXXX-XXXX";
}

/// <summary>
/// Push notification settings for deposit code events
/// </summary>
public class PushNotificationSettings
{
    /// <summary>
    /// Gets or sets whether to send expiration warnings
    /// </summary>
    public bool SendExpirationWarnings { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to send usage confirmations
    /// </summary>
    public bool SendUsageConfirmations { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the warning time before expiration (in minutes)
    /// </summary>
    public int ExpirationWarningMinutes { get; set; } = 60;
}

/// <summary>
/// Deposit code generation request model
/// </summary>
public class DepositCodeGenerationRequest
{
    /// <summary>
    /// Gets or sets the deposit amount (optional, can be validated later)
    /// </summary>
    public decimal? Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    [StringLength(3, MinimumLength = 3)]
    public string? Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration hours (default: 24)
    /// </summary>
    [Range(1, 168, ErrorMessage = "Expiration must be between 1 and 168 hours (7 days)")]
    public int ExpirationHours { get; set; } = 24;
    
    /// <summary>
    /// Gets or sets whether to generate QR code
    /// </summary>
    public bool GenerateQrCode { get; set; } = true;
    
    /// <summary>
    /// Gets or sets mobile-specific metadata
    /// </summary>
    public DepositMetadata? Metadata { get; set; }
}

/// <summary>
/// Deposit code validation request model
/// </summary>
public class DepositCodeValidationRequest
{
    /// <summary>
    /// Gets or sets the deposit code to validate
    /// </summary>
    [Required]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "Deposit code must be exactly 8 characters")]
    [RegularExpression("^[A-Z0-9]{8}$", ErrorMessage = "Deposit code must contain only uppercase letters and numbers")]
    public string DepositCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the intended deposit amount
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether this is a real-time validation check
    /// </summary>
    public bool IsRealTimeValidation { get; set; } = true;
}

/// <summary>
/// Quick validation response for mobile UX optimization
/// </summary>
public class QuickValidationResponse
{
    /// <summary>
    /// Gets or sets the deposit code
    /// </summary>
    public string DepositCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether the code is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Gets or sets the current status
    /// </summary>
    public DepositCodeStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration timestamp
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the expected amount constraint
    /// </summary>
    public decimal? ExpectedAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the expected currency constraint
    /// </summary>
    public string? ExpectedCurrency { get; set; }
    
    /// <summary>
    /// Gets or sets the formatted code for display
    /// </summary>
    public string FormattedCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the time remaining until expiration
    /// </summary>
    public TimeSpan TimeRemaining { get; set; }
}

/// <summary>
/// Deposit pre-validation request for quick checks
/// </summary>
public class DepositPreValidationRequest
{
    /// <summary>
    /// Gets or sets the deposit code to pre-validate
    /// </summary>
    [Required]
    [StringLength(8, MinimumLength = 8)]
    public string DepositCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the intended amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = string.Empty;
}
