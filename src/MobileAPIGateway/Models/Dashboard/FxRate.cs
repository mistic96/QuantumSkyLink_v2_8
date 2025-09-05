using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// FX rate
/// </summary>
public sealed class FxRate
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the base currency
    /// </summary>
    public CloudCheckoutCurrency BaseCurrency { get; set; }
    
    /// <summary>
    /// Gets or sets the payout currency
    /// </summary>
    public CloudCheckoutCurrency PayoutCurrency { get; set; }
    
    /// <summary>
    /// Gets or sets the rate
    /// </summary>
    public decimal Rate { get; set; }
    
    /// <summary>
    /// Gets or sets the quote ID
    /// </summary>
    public string QuoteId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base exchange rate
    /// </summary>
    public decimal BaseExchangeRate { get; set; }
    
    /// <summary>
    /// Gets or sets the payout exchange rate
    /// </summary>
    public decimal PayoutExchangeRate { get; set; }
}
