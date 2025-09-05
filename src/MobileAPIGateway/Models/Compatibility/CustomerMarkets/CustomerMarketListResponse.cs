namespace MobileAPIGateway.Models.Compatibility.CustomerMarkets;

/// <summary>
/// Customer market list response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CustomerMarketListResponse
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
    /// Gets or sets the customer markets
    /// </summary>
    public List<CustomerMarketItem> CustomerMarkets { get; set; } = new List<CustomerMarketItem>();
    
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
/// Customer market item model for compatibility with the old MobileOrchestrator
/// </summary>
public class CustomerMarketItem
{
    /// <summary>
    /// Gets or sets the customer market ID
    /// </summary>
    public string CustomerMarketId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market ID
    /// </summary>
    public string MarketId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market name
    /// </summary>
    public string MarketName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market type
    /// </summary>
    public string MarketType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base currency code
    /// </summary>
    public string BaseCurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quote currency code
    /// </summary>
    public string QuoteCurrencyCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market status
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
    /// Gets or sets the market cap
    /// </summary>
    public decimal MarketCap { get; set; }
    
    /// <summary>
    /// Gets or sets the trading pairs
    /// </summary>
    public List<string> TradingPairs { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets a value indicating whether the market is favorited
    /// </summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription ID
    /// </summary>
    public string? SubscriptionId { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan ID
    /// </summary>
    public string? SubscriptionPlanId { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan name
    /// </summary>
    public string? SubscriptionPlanName { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription status
    /// </summary>
    public string? SubscriptionStatus { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription start date
    /// </summary>
    public DateTime? SubscriptionStartDate { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription end date
    /// </summary>
    public DateTime? SubscriptionEndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the last updated date
    /// </summary>
    public DateTime LastUpdatedDate { get; set; }
}
