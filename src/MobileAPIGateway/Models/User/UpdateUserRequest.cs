using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.User;

/// <summary>
/// Update user request model
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    [StringLength(100)]
    public string? LastName { get; set; }
    
    /// <summary>
    /// Gets or sets the phone number
    /// </summary>
    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the address
    /// </summary>
    [StringLength(200)]
    public string? Address { get; set; }
    
    /// <summary>
    /// Gets or sets the city
    /// </summary>
    [StringLength(100)]
    public string? City { get; set; }
    
    /// <summary>
    /// Gets or sets the state
    /// </summary>
    [StringLength(100)]
    public string? State { get; set; }
    
    /// <summary>
    /// Gets or sets the country
    /// </summary>
    [StringLength(100)]
    public string? Country { get; set; }
    
    /// <summary>
    /// Gets or sets the postal code
    /// </summary>
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Gets or sets the date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// Gets or sets the profile picture URL
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? ProfilePictureUrl { get; set; }
}
