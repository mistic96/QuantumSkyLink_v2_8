namespace MobileAPIGateway.Models.CustomerMarkets;

/// <summary>
/// Customer market model
/// </summary>
public class CustomerMarket
{
    /// <summary>
    /// Gets or sets the customer market ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
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
    public string? MarketDescription { get; set; }
    
    /// <summary>
    /// Gets or sets the market logo URL
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the customer market status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer market type
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer market category
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer market tags
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();
    
    /// <summary>
    /// Gets or sets the customer market creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the customer market last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the customer market favorites
    /// </summary>
    public bool IsFavorite { get; set; }
    
    /// <summary>
    /// Gets or sets the customer market subscription status
    /// </summary>
    public bool IsSubscribed { get; set; }
    
    /// <summary>
    /// Gets or sets the customer market subscription expiration date
    /// </summary>
    public DateTime? SubscriptionExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the customer market trading pairs
    /// </summary>
    public List<CustomerTradingPair> TradingPairs { get; set; } = new List<CustomerTradingPair>();

    public string? MarketType { get; set; }
    public string BaseCurrencyCode { get; set; }
}
