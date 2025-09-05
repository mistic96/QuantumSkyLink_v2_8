using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Request model for lite mobile registration
/// </summary>
public class LiteMobileRegistrationRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's preferred name
    /// </summary>
    public string PreferredName { get; set; } = string.Empty;

    /// <summary>
    /// User's phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// User's country
    /// </summary>
    [Required]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// User's language preference
    /// </summary>
    [Required]
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// User's date of birth
    /// </summary>
    public string DateOfBirth { get; set; } = string.Empty;

    /// <summary>
    /// User's gender
    /// </summary>
    public string Gender { get; set; } = string.Empty;

    /// <summary>
    /// Whether user accepted terms and conditions
    /// </summary>
    public bool AcceptedTerms { get; set; }

    /// <summary>
    /// Device information
    /// </summary>
    public DeviceInfo DeviceInfo { get; set; } = new();
}

/// <summary>
/// Device information for registration
/// </summary>
public class DeviceInfo
{
    /// <summary>
    /// Device identifier
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Device operating system
    /// </summary>
    public string DeviceOs { get; set; } = string.Empty;

    /// <summary>
    /// Application version
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;
}
