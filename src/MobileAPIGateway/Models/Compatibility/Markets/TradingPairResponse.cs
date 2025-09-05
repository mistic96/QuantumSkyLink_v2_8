namespace MobileAPIGateway.Models.Compatibility.Markets;

/// <summary>
/// Trading pair response model for compatibility with the old MobileOrchestrator
/// </summary>
public class TradingPairResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market ID
    /// </summary>
    public string MarketId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trading pairs
    /// </summary>
    public List<TradingPairItem> TradingPairs { get; set; } = new List<TradingPairItem>();
    
    /// <summary>
    /// Gets or sets the total count
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Trading pair item model for compatibility with the old MobileOrchestrator
/// </summary>
public class TradingPairItem
{
    /// <summary>
    /// Gets or sets the trading pair ID
    /// </summary>
    public string TradingPairId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trading pair symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base currency code
    /// </summary>
    public string BaseCurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quote currency code
    /// </summary>
    public string QuoteCurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trading pair status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the current price
    /// </summary>
    public decimal CurrentPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour high price
    /// </summary>
    public decimal High24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour low price
    /// </summary>
    public decimal Low24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour volume
    /// </summary>
    public decimal Volume24h { get; set; }
    
    /// <summary>
    /// Gets or sets the 24-hour price change percentage
    /// </summary>
    public decimal PriceChangePercentage24h { get; set; }
    
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
    /// Gets or sets the created date
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the last updated date
    /// </summary>
    public DateTime LastUpdatedDate { get; set; }
}
