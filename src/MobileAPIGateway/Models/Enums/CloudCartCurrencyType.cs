using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Cloud cart currency type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CloudCartCurrencyType
{
    /// <summary>
    /// US Dollar
    /// </summary>
    USD,
    
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
    /// All cryptocurrencies
    /// </summary>
    Cryptos,
    
    /// <summary>
    /// UC tokens
    /// </summary>
    UCTokens,
    
    /// <summary>
    /// All currencies
    /// </summary>
    All,
    
    /// <summary>
    /// Euro Coin
    /// </summary>
    EUROC,
    
    /// <summary>
    /// Tether
    /// </summary>
    USDT
}
