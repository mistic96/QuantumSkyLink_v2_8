namespace MobileAPIGateway.Models.CustomerMarkets;

/// <summary>
/// Customer market subscription model
/// </summary>
public class CustomerMarketSubscription
{
    /// <summary>
    /// Gets or sets the subscription ID
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
    /// Gets or sets the subscription plan ID
    /// </summary>
    public string SubscriptionPlanId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription plan name
    /// </summary>
    public string SubscriptionPlanName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription start date
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription end date
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription renewal date
    /// </summary>
    public DateTime? RenewalDate { get; set; }
    
    /// <summary>
    /// Gets or sets whether the subscription auto-renews
    /// </summary>
    public bool AutoRenew { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription currency
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription payment method
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription payment status
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the subscription creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription cancellation date
    /// </summary>
    public DateTime? CancelledAt { get; set; }
    
    /// <summary>
    /// Gets or sets the subscription cancellation reason
    /// </summary>
    public string? CancellationReason { get; set; }
}
