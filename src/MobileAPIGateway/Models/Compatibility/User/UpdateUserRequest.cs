namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Request model for updating user profile
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
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
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// User's language preference
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// User's date of birth
    /// </summary>
    public string DateOfBirth { get; set; } = string.Empty;

    /// <summary>
    /// User's gender
    /// </summary>
    public string Gender { get; set; } = string.Empty;
}
