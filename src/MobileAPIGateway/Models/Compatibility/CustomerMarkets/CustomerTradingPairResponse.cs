namespace MobileAPIGateway.Models.Compatibility.CustomerMarkets;

/// <summary>
/// Customer trading pair response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CustomerTradingPairResponse
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
    /// Gets or sets the customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer market ID
    /// </summary>
    public string CustomerMarketId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer trading pairs
    /// </summary>
    public List<CustomerTradingPairItem> CustomerTradingPairs { get; set; } = new List<CustomerTradingPairItem>();
    
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
/// Customer trading pair item model for compatibility with the old MobileOrchestrator
/// </summary>
public class CustomerTradingPairItem
{
    /// <summary>
    /// Gets or sets the customer trading pair ID
    /// </summary>
    public string CustomerTradingPairId { get; set; } = string.Empty;
    
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
    /// Gets or sets a value indicating whether the trading pair is favorited
    /// </summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>
    /// Gets or sets the alert ID
    /// </summary>
    public string? AlertId { get; set; }
    
    /// <summary>
    /// Gets or sets the alert price
    /// </summary>
    public decimal? AlertPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the alert condition
    /// </summary>
    public string? AlertCondition { get; set; }
    
    /// <summary>
    /// Gets or sets the alert status
    /// </summary>
    public string? AlertStatus { get; set; }
    
    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the last updated date
    /// </summary>
    public DateTime LastUpdatedDate { get; set; }
}
