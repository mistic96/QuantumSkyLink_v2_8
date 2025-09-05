namespace MobileAPIGateway.Models.Markets;

/// <summary>
/// Order book model
/// </summary>
public class OrderBook
{
    /// <summary>
    /// Gets or sets the trading pair symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the last update ID
    /// </summary>
    public long LastUpdateId { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the bids
    /// </summary>
    public List<OrderBookEntry> Bids { get; set; } = new List<OrderBookEntry>();
    
    /// <summary>
    /// Gets or sets the asks
    /// </summary>
    public List<OrderBookEntry> Asks { get; set; } = new List<OrderBookEntry>();
}

/// <summary>
/// Order book entry model
/// </summary>
public class OrderBookEntry
{
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal Quantity { get; set; }
}
