namespace MobileAPIGateway.Models.Cart;

/// <summary>
/// Cloud cart summary section
/// </summary>
public sealed class CloudCartSummarySection
{
    /// <summary>
    /// Gets or sets the current exchange rates
    /// </summary>
    public List<ExchangeRate>? CurrentExchangeRates { get; set; }
    
    /// <summary>
    /// Gets or sets the fees
    /// </summary>
    public List<CloudFee>? Fees { get; set; }
    
    /// <summary>
    /// Gets or sets the primary market summary
    /// </summary>
    public CloudCartSummary? PrimaryMarket { get; set; }
    
    /// <summary>
    /// Gets or sets the secondary market summary
    /// </summary>
    public CloudCartSummary? SecondaryMarket { get; set; }
    
    /// <summary>
    /// Gets or sets the total discount
    /// </summary>
    public decimal TotalDiscount { get; set; }
    
    /// <summary>
    /// Gets or sets the number of items
    /// </summary>
    public decimal NumberOfItems { get; set; }
    
    /// <summary>
    /// Gets or sets the total fees
    /// </summary>
    public decimal TotalFees { get; set; }
    
    /// <summary>
    /// Gets or sets the markets total
    /// </summary>
    public decimal MarketsTotal { get; set; }
}
