using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Market payment method
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MarketPaymentMethod
{
    /// <summary>
    /// None (not supported)
    /// </summary>
    None,
    
    /// <summary>
    /// Fiat (not supported)
    /// </summary>
    Fiat,
    
    /// <summary>
    /// External wallet
    /// </summary>
    ExternalWallet,
    
    /// <summary>
    /// Internal wallet
    /// </summary>
    InternalWallet
}
