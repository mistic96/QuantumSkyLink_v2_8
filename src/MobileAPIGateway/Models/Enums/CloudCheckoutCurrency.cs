using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Cloud checkout currency
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CloudCheckoutCurrency
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
    /// US Dollar
    /// </summary>
    USD,
    
    /// <summary>
    /// Euro
    /// </summary>
    EUR,
    
    /// <summary>
    /// Brazilian Real
    /// </summary>
    BRL,
    
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
    USDT
}
