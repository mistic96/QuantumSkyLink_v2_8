using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// User status
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserStatus
{
    /// <summary>
    /// User is active
    /// </summary>
    Active = 1,
    
    /// <summary>
    /// User is disabled
    /// </summary>
    Disabled = 0
}
