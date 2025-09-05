namespace MobileAPIGateway.Models.Users;

/// <summary>
/// User request standard response message
/// </summary>
public sealed class UserRequestStandardResponseMessage
{
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool? Successful { get; set; }
}
