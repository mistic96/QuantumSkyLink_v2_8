using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Quick item type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuickItemType
{
    /// <summary>
    /// Market purchase
    /// </summary>
    MarketPurchase,
    
    /// <summary>
    /// Market profile activation
    /// </summary>
    MarketProfileActivation,
    
    /// <summary>
    /// Deposit
    /// </summary>
    Deposit,
    
    /// <summary>
    /// Vendor purchase
    /// </summary>
    VendorPurchase,
    
    /// <summary>
    /// Other
    /// </summary>
    Other
}
