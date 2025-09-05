namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Response model for password reset
/// </summary>
public class PasswordResetResponse
{
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public PasswordResetRequestOperationStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Password reset request operation status
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
