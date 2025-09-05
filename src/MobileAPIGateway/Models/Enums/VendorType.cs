using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Vendor type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VendorType
{
    /// <summary>
    /// Coinbase vendor
    /// </summary>
    CoinBase,
    
    /// <summary>
    /// Square vendor
    /// </summary>
    Square,
    
    /// <summary>
    /// Circle vendor
    /// </summary>
    Circle,
    
    /// <summary>
    /// Stripe vendor
    /// </summary>
    Stripe
}
