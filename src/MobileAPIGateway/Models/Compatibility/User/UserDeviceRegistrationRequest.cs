using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.User;

/// <summary>
/// User device registration request model for compatibility with the old MobileOrchestrator
/// </summary>
public class UserDeviceRegistrationRequest
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
    /// Gets or sets the device name
    /// </summary>
    [Required]
    public string DeviceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the device type
    /// </summary>
    [Required]
    public string DeviceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the device operating system
    /// </summary>
    [Required]
    public string DeviceOS { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the device operating system version
    /// </summary>
    [Required]
    public string DeviceOSVersion { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    [Required]
    public string AppVersion { get; set; } = string.Empty;
}
