using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the email
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}
