using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Wallet;

/// <summary>
/// Enhanced deposit request with deposit code support
/// </summary>
public class EnhancedDepositRequest : DepositRequest
{
    /// <summary>
    /// Gets or sets whether to use real-time validation
    /// </summary>
    public bool UseRealTimeValidation { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the client IP address for security
    /// </summary>
    public string? ClientIpAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the user agent string
    /// </summary>
    public string? UserAgent { get; set; }
}

/// <summary>
/// Deposit processing response
/// </summary>
public class DepositProcessingResponse
{
    /// <summary>
    /// Gets or sets whether the deposit was successfully processed
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the payment transaction ID
    /// </summary>
    public string? TransactionId { get; set; }
    
    /// <summary>
    /// Gets or sets the deposit address (for crypto deposits)
    /// </summary>
    public string? DepositAddress { get; set; }
    
    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the expected confirmation time
    /// </summary>
    public TimeSpan? ExpectedConfirmationTime { get; set; }
    
    /// <summary>
    /// Gets or sets the processing status
    /// </summary>
    public DepositProcessingStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets mobile-specific processing details
    /// </summary>
    public MobileProcessingDetails? MobileDetails { get; set; }
}

/// <summary>
/// Deposit processing status enumeration
/// </summary>
public enum DepositProcessingStatus
{
    /// <summary>
    /// Deposit is pending processing
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Deposit is being processed
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Deposit has been confirmed
    /// </summary>
    Confirmed = 2,
    
    /// <summary>
    /// Deposit has failed
    /// </summary>
    Failed = 3,
    
    /// <summary>
    /// Deposit requires additional verification
    /// </summary>
    RequiresVerification = 4
}

/// <summary>
/// Mobile-specific processing details
/// </summary>
public class MobileProcessingDetails
{
    /// <summary>
    /// Gets or sets whether offline processing is available
    /// </summary>
    public bool OfflineProcessingAvailable { get; set; } = false;
    
    /// <summary>
    /// Gets or sets push notification preferences
    /// </summary>
    public PushNotificationSettings? NotificationSettings { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated sync time for offline mode
    /// </summary>
    public TimeSpan? OfflineSyncTime { get; set; }
}

/// <summary>
/// Deposit code status response
/// </summary>
public class DepositCodeStatusResponse
{
    /// <summary>
    /// Gets or sets the deposit code
    /// </summary>
    public string DepositCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the current status
    /// </summary>
    public DepositCodeStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration timestamp
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the usage timestamp (if used)
    /// </summary>
    public DateTime? UsedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the associated payment ID (if used)
    /// </summary>
    public string? PaymentId { get; set; }
    
    /// <summary>
    /// Gets or sets the amount constraint (if any)
    /// </summary>
    public decimal? AmountConstraint { get; set; }
    
    /// <summary>
    /// Gets or sets the currency constraint (if any)
    /// </summary>
    public string? CurrencyConstraint { get; set; }
    
    /// <summary>
    /// Gets or sets whether the code is user-specific
    /// </summary>
    public bool IsUserSpecific { get; set; }
}

/// <summary>
/// Deposit code status enumeration
/// </summary>
public enum DepositCodeStatus
{
    /// <summary>
    /// Code is active and can be used
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// Code has been used
    /// </summary>
    Used = 1,
    
    /// <summary>
    /// Code has expired
    /// </summary>
    Expired = 2,
    
    /// <summary>
    /// Code has been revoked
    /// </summary>
    Revoked = 3,
    
    /// <summary>
    /// Code is suspended pending investigation
    /// </summary>
    Suspended = 4
}

/// <summary>
/// User deposit code information
/// </summary>
public class UserDepositCode
{
    /// <summary>
    /// Gets or sets the deposit code
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration timestamp
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the amount constraint (if any)
    /// </summary>
    public decimal? Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency constraint (if any)
    /// </summary>
    public string? Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the current status
    /// </summary>
    public DepositCodeStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the time remaining until expiration
    /// </summary>
    public TimeSpan TimeRemaining => ExpiresAt > DateTime.UtcNow ? ExpiresAt - DateTime.UtcNow : TimeSpan.Zero;
    
    /// <summary>
    /// Gets whether the code is still active
    /// </summary>
    public bool IsActive => Status == DepositCodeStatus.Active && ExpiresAt > DateTime.UtcNow;
}

/// <summary>
/// Deposit code revocation response
/// </summary>
public class DepositCodeRevocationResponse
{
    /// <summary>
    /// Gets or sets whether the revocation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the revoked deposit code
    /// </summary>
    public string DepositCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the revocation timestamp
    /// </summary>
    public DateTime RevokedAt { get; set; }
    
    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the reason for revocation
    /// </summary>
    public string? RevocationReason { get; set; }
}