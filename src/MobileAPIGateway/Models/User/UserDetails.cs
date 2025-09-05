namespace MobileAPIGateway.Models.User;

/// <summary>
/// User details model
/// </summary>
public class UserDetails
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
    /// Gets or sets the address
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the city
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the state
    /// </summary>
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the country
    /// </summary>
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// Gets or sets the profile picture URL
    /// </summary>
    public string ProfilePictureUrl { get; set; } = string.Empty;
    
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
    
    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
