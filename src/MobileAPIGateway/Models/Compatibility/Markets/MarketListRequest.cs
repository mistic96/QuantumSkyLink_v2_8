using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Markets;

/// <summary>
/// Market list request model for compatibility with the old MobileOrchestrator
/// </summary>
public class MarketListRequest
{
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the filter by market status
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Gets or sets the filter by market type
    /// </summary>
    public string? MarketType { get; set; }
    
    /// <summary>
    /// Gets or sets the filter by currency code
    /// </summary>
    public string? CurrencyCode { get; set; }
}
