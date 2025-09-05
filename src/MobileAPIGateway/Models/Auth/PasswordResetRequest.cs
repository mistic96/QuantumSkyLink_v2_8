namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Request model for password reset
/// </summary>
public class PasswordResetRequest
{
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
