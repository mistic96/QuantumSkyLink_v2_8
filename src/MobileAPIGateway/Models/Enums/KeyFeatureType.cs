using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Key feature type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum KeyFeatureType
{
    /// <summary>
    /// Product feature
    /// </summary>
    Product,
    
    /// <summary>
    /// Token feature
    /// </summary>
    Token
}
