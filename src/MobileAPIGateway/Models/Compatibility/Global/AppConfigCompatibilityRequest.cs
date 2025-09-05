namespace MobileAPIGateway.Models.Compatibility.Global;

/// <summary>
/// App configuration compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class AppConfigCompatibilityRequest
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
    
    /// <summary>
    /// Gets or sets the configuration key
    /// </summary>
    public string? ConfigKey { get; set; }
}
