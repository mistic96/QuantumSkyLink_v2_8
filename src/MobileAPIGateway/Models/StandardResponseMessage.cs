namespace MobileAPIGateway.Models;

/// <summary>
/// Standard response message
/// </summary>
public class StandardResponseMessage
{
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Creates a problem message response
    /// </summary>
    /// <returns>The problem message response</returns>
    public StandardResponseMessage AsProblemMessageResponse()
    {
        return this;
    }
}
