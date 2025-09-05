using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Listing message type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ListingMessageType
{
    /// <summary>
    /// None
    /// </summary>
    None,
    
    /// <summary>
    /// User message
    /// </summary>
    User,
    
    /// <summary>
    /// Error message
    /// </summary>
    Error
}
