namespace MobileAPIGateway.Models.Users;

/// <summary>
/// User response
/// </summary>
public sealed class UserResponse
{
    /// <summary>
    /// Gets or sets the user
    /// </summary>
    public User? User { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the response has a message
    /// </summary>
    public bool HasResponseMessage { get; set; }
}
