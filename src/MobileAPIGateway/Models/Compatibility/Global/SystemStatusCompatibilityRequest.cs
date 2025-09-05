namespace MobileAPIGateway.Models.Compatibility.Global;

/// <summary>
/// System status compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class SystemStatusCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the client version
    /// </summary>
    public string? ClientVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the device type
    /// </summary>
    public string? DeviceType { get; set; }
    
    /// <summary>
    /// Gets or sets the operating system
    /// </summary>
    public string? OperatingSystem { get; set; }
}
