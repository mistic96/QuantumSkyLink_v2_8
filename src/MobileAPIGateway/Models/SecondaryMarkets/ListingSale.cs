using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Listing sale
/// </summary>
public sealed class ListingSale
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the listing ID
    /// </summary>
    public Guid ListingId { get; set; }
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction set ID
    /// </summary>
    public Guid TransactionSetId { get; set; }
    
    /// <summary>
    /// Gets or sets the block reference
    /// </summary>
    public string? BlockReference { get; set; }
    
    /// <summary>
    /// Gets or sets the asset
    /// </summary>
    public CloudCheckoutCurrency Asset { get; set; }
    
    /// <summary>
    /// Gets or sets the exchange rate
    /// </summary>
    public LedgerMarketExchangeRate? ExchangeRate { get; set; }
    
    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the total price
    /// </summary>
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the total fees
    /// </summary>
    public decimal TotalFees { get; set; }
}
