using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.CustomerMarkets;

/// <summary>
/// Customer trading pair request model for compatibility with the old MobileOrchestrator
/// </summary>
public class CustomerTradingPairRequest
{
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer market ID
    /// </summary>
    [Required]
    public string CustomerMarketId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the filter by trading pair status
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Gets or sets the filter by base currency code
    /// </summary>
    public string? BaseCurrencyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the filter by quote currency code
    /// </summary>
    public string? QuoteCurrencyCode { get; set; }
}
