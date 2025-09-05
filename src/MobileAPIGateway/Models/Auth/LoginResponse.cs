namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Login response model
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the token type
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
    
    /// <summary>
    /// Gets or sets the expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user email
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
