using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Token index milestone measurement type parameter
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenIndexMileStoneMeasurementTypeParameter
{
    /// <summary>
    /// Token sales
    /// </summary>
    TokenSales,
    
    /// <summary>
    /// Vote
    /// </summary>
    Vote,
    
    /// <summary>
    /// Market price
    /// </summary>
    MarketPrice,
    
    /// <summary>
    /// Market cap
    /// </summary>
    MarketCap,
    
    /// <summary>
    /// Circulating supply
    /// </summary>
    CirculatingSupply,
    
    /// <summary>
    /// Duration
    /// </summary>
    Duration
}
