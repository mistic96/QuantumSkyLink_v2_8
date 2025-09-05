namespace MobileAPIGateway.Models.Markets;

/// <summary>
/// Trading pair model
/// </summary>
public class TradingPair
{
    /// <summary>
    /// Gets or sets the trading pair ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
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
    /// Gets or sets the current price
    /// </summary>
    public decimal CurrentPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour price change
    /// </summary>
    public decimal PriceChange24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour price change percentage
    /// </summary>
    public decimal PriceChangePercent24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour high price
    /// </summary>
    public decimal HighPrice24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour low price
    /// </summary>
    public decimal LowPrice24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour volume
    /// </summary>
    public decimal Volume24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour quote volume
    /// </summary>
    public decimal QuoteVolume24h { get; set; }
    
    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
