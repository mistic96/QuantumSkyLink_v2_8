namespace MobileAPIGateway.Models.Compatibility.Global;

/// <summary>
/// Limit compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class LimitCompatibilityRequest
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
    /// Gets or sets the limit type
    /// </summary>
    public string? LimitType { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public string? UserId { get; set; }
}
