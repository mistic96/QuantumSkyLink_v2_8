namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Estimated profit projection
/// </summary>
public sealed class EstimatedProfitProjection
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the exchange rate
    /// </summary>
    public LedgerMarketExchangeRate? ExchangeRate { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated profit
    /// </summary>
    public decimal EstimatedProfit { get; set; }
}
