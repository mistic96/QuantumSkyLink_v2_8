namespace MobileAPIGateway.Models.Global;

/// <summary>
/// Represents the system status
/// </summary>
public class SystemStatus
{
    /// <summary>
    /// Gets or sets the status of the system
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the version of the system
    /// </summary>
    public string Version { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of the status
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the service statuses
    /// </summary>
    public Dictionary<string, ServiceStatus> Services { get; set; } = new Dictionary<string, ServiceStatus>();
}

/// <summary>
/// Represents the status of a service
/// </summary>
public class ServiceStatus
{
    /// <summary>
    /// Gets or sets the name of the service
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the status of the service
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the version of the service
    /// </summary>
    public string Version { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of the status
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; }
}
