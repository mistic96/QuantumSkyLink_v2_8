using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Item type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ItemType
{
    /// <summary>
    /// Team item type
    /// </summary>
    Team,
    
    /// <summary>
    /// Country item type
    /// </summary>
    Country,
    
    /// <summary>
    /// Currency item type
    /// </summary>
    Currency,
    
    /// <summary>
    /// Vendor token item type
    /// </summary>
    VendorToken,
    
    /// <summary>
    /// All item types
    /// </summary>
    All,
    
    /// <summary>
    /// Crypto item type
    /// </summary>
    Crypto
}
