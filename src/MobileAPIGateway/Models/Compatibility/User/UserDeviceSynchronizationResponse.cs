namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// User device synchronization response model for compatibility with the old MobileOrchestrator
/// </summary>
public class UserDeviceSynchronizationResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the device synchronization was successful
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
    
    /// <summary>
    /// Gets or sets the synchronization timestamp
    /// </summary>
    public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets a value indicating whether the device needs to be updated
    /// </summary>
    public bool NeedsUpdate { get; set; }
    
    /// <summary>
    /// Gets or sets the latest application version
    /// </summary>
    public string LatestAppVersion { get; set; } = string.Empty;
}
