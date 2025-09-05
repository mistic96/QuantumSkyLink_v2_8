namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Options for Logto authentication configuration
/// </summary>
public class LogtoAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the claim type for email
    /// </summary>
    public string EmailClaimType { get; set; } = "email";

    /// <summary>
    /// Gets or sets the claim type for roles
    /// </summary>
    public string RoleClaimType { get; set; } = "role";

    /// <summary>
    /// Gets or sets the claim type for username
    /// </summary>
    public string UsernameClaimType { get; set; } = "preferred_username";

    /// <summary>
    /// Gets or sets the claim type for name
    /// </summary>
    public string NameClaimType { get; set; } = "name";
}
