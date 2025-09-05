namespace MobileAPIGateway.Models.CustomerMarkets;

/// <summary>
/// Customer trading pair model
/// </summary>
public class CustomerTradingPair
{
    public decimal High24h;
    public decimal Low24h;
    public decimal Volume24h;
    public decimal PriceChangePercentage24h;

    /// <summary>
    /// Gets or sets the customer trading pair ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trading pair ID
    /// </summary>
    public string TradingPairId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trading pair symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base asset
    /// </summary>
    public string BaseAsset { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quote asset
    /// </summary>
    public string QuoteAsset { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trading pair status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the minimum order size
    /// </summary>
    public decimal MinOrderSize { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum order size
    /// </summary>
    public decimal MaxOrderSize { get; set; }
    
    /// <summary>
    /// Gets or sets the price precision
    /// </summary>
    public int PricePrecision { get; set; }
    
    /// <summary>
    /// Gets or sets the quantity precision
    /// </summary>
    public int QuantityPrecision { get; set; }
    
    /// <summary>
    /// Gets or sets the base asset precision
    /// </summary>
    public int BaseAssetPrecision { get; set; }
    
    /// <summary>
    /// Gets or sets the quote asset precision
    /// </summary>
    public int QuoteAssetPrecision { get; set; }
    
    /// <summary>
    /// Gets or sets the trading fee
    /// </summary>
    public decimal TradingFee { get; set; }
    
    /// <summary>
    /// Gets or sets the customer trading pair favorites
    /// </summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>
    /// Gets or sets the customer trading pair alerts
    /// </summary>
    public List<CustomerTradingPairAlert> Alerts { get; set; } = new List<CustomerTradingPairAlert>();
    
    /// <summary>
    /// Gets or sets the customer trading pair creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the customer trading pair last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public string CustomerMarketId { get; set; }
    public string? BaseCurrencyCode { get; set; }
    public string QuoteCurrencyCode { get; set; }
    public decimal CurrentPrice { get; set; }
    public object Alert { get; set; }
}
