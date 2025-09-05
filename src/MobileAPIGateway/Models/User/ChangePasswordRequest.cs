using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.User;

/// <summary>
/// Change password request model
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Gets or sets the current password
    /// </summary>
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the new password
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the confirm password
    /// </summary>
    [Required]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
