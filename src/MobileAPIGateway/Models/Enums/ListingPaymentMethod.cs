using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Listing payment method
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ListingPaymentMethod
{
    /// <summary>
    /// Fiat payment
    /// </summary>
    Fiat,
    
    /// <summary>
    /// Fiat card payment
    /// </summary>
    FiatCard,
    
    /// <summary>
    /// Fiat bank payment
    /// </summary>
    FiatBank,
    
    /// <summary>
    /// Fiat mobile wallet payment
    /// </summary>
    FiatMobileWallet,
    
    /// <summary>
    /// Cryptocurrency payment
    /// </summary>
    Crypto,
    
    /// <summary>
    /// Internal wallet payment
    /// </summary>
    InternalWallet,
    
    /// <summary>
    /// XL wallet payment
    /// </summary>
    XLWallet,
    
    /// <summary>
    /// External wallet payment
    /// </summary>
    ExternalWallet
}
