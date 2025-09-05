using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Cloud cart type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CloudCartType
{
    /// <summary>
    /// UC cart type
    /// </summary>
    UC,
    
    /// <summary>
    /// XL cart type
    /// </summary>
    XL,
    
    /// <summary>
    /// WF cart type
    /// </summary>
    WF
}
