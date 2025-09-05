namespace MobileAPIGateway.Models.Compatibility.Global;

/// <summary>
/// App configuration compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class AppConfigCompatibilityResponse
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
    /// Gets or sets the configuration key
    /// </summary>
    public string? ConfigKey { get; set; }
    
    /// <summary>
    /// Gets or sets the configuration value
    /// </summary>
    public string? ConfigValue { get; set; }
    
    /// <summary>
    /// Gets or sets the configuration description
    /// </summary>
    public string? ConfigDescription { get; set; }
    
    /// <summary>
    /// Gets or sets the configuration group
    /// </summary>
    public string? ConfigGroup { get; set; }
    
    /// <summary>
    /// Gets or sets the configuration type
    /// </summary>
    public string? ConfigType { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the configuration is enabled
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the server time
    /// </summary>
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
