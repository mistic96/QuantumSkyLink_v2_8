using System.Text.Json.Serialization;

namespace MobileAPIGateway.Models.Enums;

/// <summary>
/// Sale status
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SaleStatus
{
    /// <summary>
    /// Pending status
    /// </summary>
    Pending,
    
    /// <summary>
    /// Consolidated status
    /// </summary>
    Consolidated,
    
    /// <summary>
    /// Sold status
    /// </summary>
    Sold,
    
    /// <summary>
    /// Canceled status
    /// </summary>
    Canceled,
    
    /// <summary>
    /// Expired status
    /// </summary>
    Expired,
    
    /// <summary>
    /// Pending coin or payment transfer status
    /// </summary>
    PendingCoinOrPaymentTransfer
}
