using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Ledger market exchange rate
/// </summary>
public class LedgerMarketExchangeRate
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
    /// Gets or sets the asset
    /// </summary>
    public MarketAsset Asset { get; set; }
    
    /// <summary>
    /// Gets or sets the exchange rate
    /// </summary>
    public decimal ExchangeRate { get; set; }
    
    /// <summary>
    /// Gets or sets the date
    /// </summary>
    public DateTimeOffset Date { get; set; }
    
    /// <summary>
    /// Gets or sets the vendor
    /// </summary>
    public ExchangeRateVendor Vendor { get; set; }
    
    /// <summary>
    /// Gets or sets the quote ID
    /// </summary>
    public string? QuoteId { get; set; }
    
    /// <summary>
    /// Gets or sets the USD value
    /// </summary>
    public decimal UsdValue { get; set; }
}
