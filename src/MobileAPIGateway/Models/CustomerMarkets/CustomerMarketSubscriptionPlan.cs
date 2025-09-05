namespace MobileAPIGateway.Models.CustomerMarkets;

/// <summary>
/// Customer market subscription plan model
/// </summary>
public class CustomerMarketSubscriptionPlan
{
    /// <summary>
    /// Gets or sets the subscription plan ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the market ID
    /// </summary>
    public string MarketId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription plan name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription plan description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription plan price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan currency
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription plan duration in days
    /// </summary>
    public int DurationDays { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan features
    /// </summary>
    public List<string> Features { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the subscription plan status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription plan creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets whether the subscription plan is featured
    /// </summary>
    public bool IsFeatured { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan sort order
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan maximum number of trading pairs
    /// </summary>
    public int? MaxTradingPairs { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription plan maximum number of alerts
    /// </summary>
    public int? MaxAlerts { get; set; }
    
    /// <summary>
    /// Gets or sets whether the subscription plan includes real-time data
    /// </summary>
    public bool IncludesRealTimeData { get; set; }
    
    /// <summary>
    /// Gets or sets whether the subscription plan includes advanced analytics
    /// </summary>
    public bool IncludesAdvancedAnalytics { get; set; }
    
    /// <summary>
    /// Gets or sets whether the subscription plan includes API access
    /// </summary>
    public bool IncludesApiAccess { get; set; }
}
