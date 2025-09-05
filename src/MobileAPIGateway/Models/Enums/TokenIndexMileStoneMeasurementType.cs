using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Token index milestone measurement type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenIndexMileStoneMeasurementType
{
    /// <summary>
    /// None
    /// </summary>
    None,
    
    /// <summary>
    /// Amount
    /// </summary>
    Amount,
    
    /// <summary>
    /// Percentage
    /// </summary>
    Percentage
}
