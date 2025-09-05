using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Percentage flux types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PercentageFluxTypes
{
    /// <summary>
    /// Increase
    /// </summary>
    Increase,
    
    /// <summary>
    /// Decrease
    /// </summary>
    Decrease,
    
    /// <summary>
    /// Neutral
    /// </summary>
    Neutral
}
