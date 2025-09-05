using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Payment method
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod
{
    /// <summary>
    /// None
    /// </summary>
    None,
    
    /// <summary>
    /// Fiat card
    /// </summary>
    FiatCard,
    
    /// <summary>
    /// Fiat bank
    /// </summary>
    FiatBank,
    
    /// <summary>
    /// Cryptocurrency
    /// </summary>
    CryptoCurrency,
    
    /// <summary>
    /// Wallet
    /// </summary>
    Wallet
}
