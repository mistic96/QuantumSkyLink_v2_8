namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// Generic response model for user operations
/// </summary>
public class UserOperationResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Operation message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
