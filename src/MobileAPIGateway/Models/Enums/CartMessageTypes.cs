using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Cart message types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CartMessageTypes
{
    /// <summary>
    /// Okay message
    /// </summary>
    Okay,
    
    /// <summary>
    /// Notification message
    /// </summary>
    Notification,
    
    /// <summary>
    /// Bad request message
    /// </summary>
    BadRequest,
    
    /// <summary>
    /// Error message
    /// </summary>
    Error
}
