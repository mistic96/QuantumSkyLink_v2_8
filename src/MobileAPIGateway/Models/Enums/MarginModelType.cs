using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Margin model type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MarginModelType
{
    /// <summary>
    /// Fixed margin model
    /// </summary>
    Fixed,
    
    /// <summary>
    /// Percentage margin model
    /// </summary>
    Percentage
}
