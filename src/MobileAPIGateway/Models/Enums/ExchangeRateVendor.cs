using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Exchange rate vendor
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExchangeRateVendor
{
    /// <summary>
    /// None
    /// </summary>
    None,
    
    /// <summary>
    /// CoinBase
    /// </summary>
    CoinBase,
    
    /// <summary>
    /// Open Exchange Rates
    /// </summary>
    OpenExchangeRates,
    
    /// <summary>
    /// Fixer
    /// </summary>
    Fixer,
    
    /// <summary>
    /// Alpha Vantage
    /// </summary>
    AlphaVantage
}
