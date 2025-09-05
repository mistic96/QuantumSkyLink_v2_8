using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Auth;

/// <summary>
/// Password reset operation request model for compatibility with the old MobileOrchestrator
/// </summary>
public class PasswordResetOperationRequest
{
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the reset token
    /// </summary>
    [Required]
    public string ResetToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the new password
    /// </summary>
    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
