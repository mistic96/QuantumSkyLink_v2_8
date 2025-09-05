namespace MobileAPIGateway.Models.Markets;

/// <summary>
/// Trade model
/// </summary>
public class Trade
{
    /// <summary>
    /// Gets or sets the trade ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the trading pair symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the quote quantity
    /// </summary>
    public decimal QuoteQuantity { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the buyer is the market maker
    /// </summary>
    public bool IsBuyerMaker { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the trade is the best price match
    /// </summary>
    public bool IsBestMatch { get; set; }
}
