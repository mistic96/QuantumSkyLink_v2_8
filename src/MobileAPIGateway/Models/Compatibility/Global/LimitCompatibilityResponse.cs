namespace MobileAPIGateway.Models.Compatibility.Global;

/// <summary>
/// Limit compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class LimitCompatibilityResponse
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
    /// Gets or sets the limit type
    /// </summary>
    public string? LimitType { get; set; }
    
    /// <summary>
    /// Gets or sets the limit value
    /// </summary>
    public decimal LimitValue { get; set; }
    
    /// <summary>
    /// Gets or sets the limit currency
    /// </summary>
    public string? LimitCurrency { get; set; }
    
    /// <summary>
    /// Gets or sets the limit period
    /// </summary>
    public string? LimitPeriod { get; set; }
    
    /// <summary>
    /// Gets or sets the limit reset date
    /// </summary>
    public DateTime? LimitResetDate { get; set; }
    
    /// <summary>
    /// Gets or sets the current usage
    /// </summary>
    public decimal CurrentUsage { get; set; }
    
    /// <summary>
    /// Gets or sets the remaining amount
    /// </summary>
    public decimal RemainingAmount { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the limit is exceeded
    /// </summary>
    public bool IsLimitExceeded { get; set; }
    
    /// <summary>
    /// Gets or sets the server time
    /// </summary>
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
