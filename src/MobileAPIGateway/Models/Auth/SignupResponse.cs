namespace MobileAPIGateway.Models.Auth;

/// <summary>
/// Signup response model
/// </summary>
public class SignupResponse
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the Logto user ID
    /// </summary>
    public string? LogtoUserId { get; set; }

    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the signup was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the success message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether email verification is required
    /// </summary>
    public bool RequiresEmailVerification { get; set; } = false;

    /// <summary>
    /// Gets or sets any additional metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
