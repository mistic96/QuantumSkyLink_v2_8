using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Cart;

/// <summary>
/// Quick buy item
/// </summary>
public sealed class QuickBuyItem
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the token index ID
    /// </summary>
    public string? TokenIndexId { get; set; }
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the logo
    /// </summary>
    public string? Logo { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the item is a service
    /// </summary>
    public bool IsService { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the rate
    /// </summary>
    public CloudExchangeRate? Rate { get; set; }
    
    /// <summary>
    /// Gets or sets the date created
    /// </summary>
    public DateTimeOffset? DateCreated { get; set; }
    
    /// <summary>
    /// Gets or sets the status
    /// </summary>
    public SaleStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the total price
    /// </summary>
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public QuickItemType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public MarketAsset? Currency { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the item is contracted
    /// </summary>
    public bool IsContracted { get; set; }
    
    /// <summary>
    /// Gets or sets the market symbol
    /// </summary>
    public string? MarketSymbol { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the currency requires rate
    /// </summary>
    public bool IsCurrencyRequiresRate { get; set; }
}
