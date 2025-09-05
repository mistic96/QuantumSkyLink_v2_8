using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// User device synchronization request model for compatibility with the old MobileOrchestrator
/// </summary>
public class UserDeviceSynchronizationRequest
{
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the device identifier
    /// </summary>
    [Required]
    public string DeviceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    [Required]
    public string AppVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the last synchronization timestamp
    /// </summary>
    public DateTime? LastSyncTimestamp { get; set; }
}
