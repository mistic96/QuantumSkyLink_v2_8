using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// Asset backed currency
/// </summary>
public sealed class AssetBackedCurrency
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public MarketAsset Type { get; set; }
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    public decimal Amount { get; set; }
}
