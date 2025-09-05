using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Cart;

/// <summary>
/// Exchange rate
/// </summary>
public class ExchangeRate
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the currency type
    /// </summary>
    public MarketAsset CurrencyType { get; set; }
    
    /// <summary>
    /// Gets or sets the currency name
    /// </summary>
    public string? CurrencyName { get; set; }
    
    /// <summary>
    /// Gets or sets the index date
    /// </summary>
    public DateTime? IndexDate { get; set; }
    
    /// <summary>
    /// Gets or sets the currency rate
    /// </summary>
    public decimal? CurrencyRate { get; set; }
    
    /// <summary>
    /// Gets or sets the USD value
    /// </summary>
    public decimal UsdValue { get; set; }
    
    /// <summary>
    /// Gets or sets the quote ID
    /// </summary>
    public string? QuoteId { get; set; }
    
    /// <summary>
    /// Gets or sets the store date
    /// </summary>
    public DateTime StoreDate { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the rate is stale
    /// </summary>
    public bool IsStale { get; set; }
}
