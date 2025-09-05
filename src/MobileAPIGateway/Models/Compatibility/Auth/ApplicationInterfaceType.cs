namespace MobileAPIGateway.Models.Compatibility.Auth;

/// <summary>
/// Application interface type for compatibility with the old MobileOrchestrator
/// </summary>
public enum ApplicationInterfaceType
{
    /// <summary>
    /// Mobile application
    /// </summary>
    Mobile,
    
    /// <summary>
    /// Web application
    /// </summary>
    Web,
    
    /// <summary>
    /// Desktop application
    /// </summary>
    Desktop,
    
    /// <summary>
    /// API
    /// </summary>
    Api
}
