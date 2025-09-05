namespace MobileAPIGateway.Models.Compatibility.Global;

/// <summary>
/// System status compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class SystemStatusCompatibilityResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the system is online
    /// </summary>
    public bool IsSystemOnline { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether maintenance is in progress
    /// </summary>
    public bool IsMaintenanceInProgress { get; set; }
    
    /// <summary>
    /// Gets or sets the maintenance message
    /// </summary>
    public string? MaintenanceMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the maintenance end time
    /// </summary>
    public DateTime? MaintenanceEndTime { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether an update is required
    /// </summary>
    public bool IsUpdateRequired { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum required version
    /// </summary>
    public string? MinimumRequiredVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the latest version
    /// </summary>
    public string? LatestVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the update URL
    /// </summary>
    public string? UpdateUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the server time
    /// </summary>
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
