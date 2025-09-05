using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Auth;

/// <summary>
/// Password reset request model for compatibility with the old MobileOrchestrator
/// </summary>
public class PasswordResetRequest
{
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
