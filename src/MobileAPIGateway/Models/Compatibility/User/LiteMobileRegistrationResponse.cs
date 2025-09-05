namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Response model for lite mobile registration
/// </summary>
public class LiteMobileRegistrationResponse
{
    /// <summary>
    /// Registration identifier
    /// </summary>
    public string RegistrationId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// User's preferred language
    /// </summary>
    public string PreferredLanguage { get; set; } = string.Empty;

    /// <summary>
    /// User's preferred name
    /// </summary>
    public string PreferredName { get; set; } = string.Empty;

    /// <summary>
    /// Registration message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
