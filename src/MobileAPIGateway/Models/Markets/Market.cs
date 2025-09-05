namespace MobileAPIGateway.Models.Markets;

/// <summary>
/// Market model
/// </summary>
public class Market
{
    /// <summary>
    /// Gets or sets the market ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the market logo URL
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the market status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market type
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market category
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market tags
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the pricing strategy
    /// </summary>
    public PricingStrategy PricingStrategy { get; set; }
    
    /// <summary>
    /// Gets or sets the price tiers (for tiered pricing strategy)
    /// </summary>
    public List<PriceTier>? PriceTiers { get; set; }
    
    /// <summary>
    /// Gets or sets the base price
    /// </summary>
    public decimal BasePrice { get; set; }
    
    /// <summary>
    /// Gets or sets the market creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the market last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the market trading pairs
    /// </summary>
    public List<TradingPair> TradingPairs { get; set; } = new List<TradingPair>();
}
