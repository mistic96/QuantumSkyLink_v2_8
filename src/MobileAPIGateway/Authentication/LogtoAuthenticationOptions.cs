namespace MobileAPIGateway.Authentication;

/// <summary>
/// Options for Logto authentication configuration
/// </summary>
public class LogtoAuthenticationOptions
{
    /// <summary>
    /// The Logto endpoint URL
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// The audience for the JWT token
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The issuer for the JWT token
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The client ID for the application
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret for the application
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The name of the claim that contains the user's email
    /// </summary>
    public string EmailClaimType { get; set; } = "email";

    /// <summary>
    /// The name of the claim that contains the user's roles
    /// </summary>
    public string RoleClaimType { get; set; } = "role";
}
