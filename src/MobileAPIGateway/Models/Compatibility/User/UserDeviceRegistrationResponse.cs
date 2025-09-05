namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// User device registration response model for compatibility with the old MobileOrchestrator
/// </summary>
public class UserDeviceRegistrationResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the device registration was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the device identifier
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;
}
