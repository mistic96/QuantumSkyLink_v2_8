using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Market asset
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MarketAsset
{
    /// <summary>
    /// Bitcoin
    /// </summary>
    BTC,
    
    /// <summary>
    /// Ethereum
    /// </summary>
    ETH,
    
    /// <summary>
    /// Litecoin
    /// </summary>
    LTC,
    
    /// <summary>
    /// USD Coin
    /// </summary>
    USDC,
    
    /// <summary>
    /// Euro Coin
    /// </summary>
    EUROC,
    
    /// <summary>
    /// Tether
    /// </summary>
    USDT,
    
    /// <summary>
    /// US Dollar
    /// </summary>
    USD,
    
    /// <summary>
    /// Brazilian Real
    /// </summary>
    BRL,
    
    /// <summary>
    /// Euro
    /// </summary>
    EUR,
    
    /// <summary>
    /// None
    /// </summary>
    None
}
