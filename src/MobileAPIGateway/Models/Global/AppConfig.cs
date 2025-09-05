namespace MobileAPIGateway.Models.Global;

/// <summary>
/// Represents the application configuration
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Gets or sets the application name
    /// </summary>
    public required string AppName { get; set; }
    
    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    public required string AppVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the API version
    /// </summary>
    public required string ApiVersion { get; set; }
    
    /// <summary>
    /// Gets or sets the environment
    /// </summary>
    public required string Environment { get; set; }
    
    /// <summary>
    /// Gets or sets the build number
    /// </summary>
    public required string BuildNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the build date
    /// </summary>
    public DateTime BuildDate { get; set; }
    
    /// <summary>
    /// Gets or sets the feature flags
    /// </summary>
    public Dictionary<string, bool> FeatureFlags { get; set; } = new Dictionary<string, bool>();
    
    /// <summary>
    /// Gets or sets the configuration settings
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
}
