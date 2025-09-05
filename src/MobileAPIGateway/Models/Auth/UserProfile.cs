namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// User profile model
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the email
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the profile picture URL
    /// </summary>
    public string ProfilePictureUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the roles
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the email is verified
    /// </summary>
    public bool IsEmailVerified { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the phone number is verified
    /// </summary>
    public bool IsPhoneVerified { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; }
}
public class JwtConfiguration
{
    public const string SectionName = "Jwt";
    
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
    public int ClockSkewMinutes { get; set; } = 5;
    public bool ValidateAudience { get; set; } = true;
}
