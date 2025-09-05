using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.PrimaryMarkets;

/// <summary>
/// Market exchange rate
/// </summary>
public sealed class MarketExchangeRate
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the currency type
    /// </summary>
    public MarketAsset CurrencyType { get; set; }
    
    /// <summary>
    /// Gets or sets the index date
    /// </summary>
    public DateTime IndexDate { get; set; }
    
    /// <summary>
    /// Gets or sets the currency rate
    /// </summary>
    public decimal? CurrencyRate { get; set; }
    
    /// <summary>
    /// Gets or sets the price in USD
    /// </summary>
    public decimal PriceInUsd { get; set; }
    
    /// <summary>
    /// Gets or sets the quote ID
    /// </summary>
    public string? QuoteId { get; set; }
}
