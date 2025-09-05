namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Response model for getting user details
/// </summary>
public class GetUserResponse
{
    /// <summary>
    /// User identifier
    /// </summary>
    public string UserId { get; set; } = string.Empty;

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
    /// Whether user is verified
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// KYC status
    /// </summary>
    public string KycStatus { get; set; } = string.Empty;

    /// <summary>
    /// Account status
    /// </summary>
    public string AccountStatus { get; set; } = string.Empty;

    /// <summary>
    /// Registration date
    /// </summary>
    public DateTime RegistrationDate { get; set; }
}
