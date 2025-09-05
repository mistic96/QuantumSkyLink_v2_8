namespace MobileAPIGateway.Models.Responses;

/// <summary>
/// Error response
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trace ID
    /// </summary>
    public string TraceId { get; set; } = string.Empty;
}
