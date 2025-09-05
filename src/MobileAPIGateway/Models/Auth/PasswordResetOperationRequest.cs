namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Request model for password reset operation
/// </summary>
public class PasswordResetOperationRequest
{
    /// <summary>
    /// Gets or sets the reset token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirm password
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
