using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Refresh token request model
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
