namespace MobileAPIGateway.Models.CustomerMarkets;

/// <summary>
/// Customer trading pair alert model
/// </summary>
public class CustomerTradingPairAlert
{
    /// <summary>
    /// Gets or sets the alert ID
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
    /// Gets or sets the alert type
    /// </summary>
    public string AlertType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the alert condition
    /// </summary>
    public string Condition { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the alert value
    /// </summary>
    public decimal Value { get; set; }
    
    /// <summary>
    /// Gets or sets the alert status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the alert notification method
    /// </summary>
    public string NotificationMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the alert notification destination
    /// </summary>
    public string NotificationDestination { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the alert creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the alert last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the alert last triggered date
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }
    
    /// <summary>
    /// Gets or sets the alert expiration date
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets whether the alert is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether the alert is repeatable
    /// </summary>
    public bool IsRepeatable { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the alert repeat interval in minutes
    /// </summary>
    public int? RepeatIntervalMinutes { get; set; }
}
