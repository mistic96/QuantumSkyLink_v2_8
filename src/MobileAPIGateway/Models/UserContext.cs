using System.Security.Claims;

namespace MobileAPIGateway.Models;

/// <summary>
/// Represents the context of the authenticated user
/// </summary>
public class UserContext
{
    /// <summary>
    /// Gets or sets the user's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's roles
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the user's claims
    /// </summary>
    public IEnumerable<Claim> Claims { get; set; } = Array.Empty<Claim>();

    /// <summary>
    /// Gets a value indicating whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);

    /// <summary>
    /// Gets a value indicating whether the user has the specified role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user has the role, otherwise false</returns>
    public bool HasRole(string role)
    {
        return Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a claim value by type
    /// </summary>
    /// <param name="claimType">The claim type</param>
    /// <returns>The claim value, or null if not found</returns>
    public string? GetClaimValue(string claimType)
    {
        return Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
