namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Standard response message for user authentication
/// </summary>
public class UserAuthStandardResponseMessage
{
    /// <summary>
    /// Gets or sets a value indicating whether the user is allowed to login
    /// </summary>
    public bool IsAllowedToLogin { get; set; }

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
