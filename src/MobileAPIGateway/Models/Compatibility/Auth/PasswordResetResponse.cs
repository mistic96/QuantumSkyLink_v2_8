namespace MobileAPIGateway.Models.Compatibility.Auth;

/// <summary>
/// Password reset response model for compatibility with the old MobileOrchestrator
/// </summary>
public class PasswordResetResponse
{
    /// <summary>
    /// Gets or sets the status of the password reset operation
    /// </summary>
    public PasswordResetRequestOperationStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Password reset request operation status for compatibility with the old MobileOrchestrator
/// </summary>
public enum PasswordResetRequestOperationStatus
{
    /// <summary>
    /// The operation was successful
    /// </summary>
    Success,
    
    /// <summary>
    /// The operation failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// The operation was blocked
    /// </summary>
    Blocked
}
