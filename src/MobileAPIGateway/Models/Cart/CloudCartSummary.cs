namespace MobileAPIGateway.Models.Cart;

/// <summary>
/// Cloud cart summary
/// </summary>
public sealed class CloudCartSummary
{
    /// <summary>
    /// Gets or sets the unit price
    /// </summary>
    public decimal UntilPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the total items
    /// </summary>
    public decimal TotalItems { get; set; }
    
    /// <summary>
    /// Gets or sets the discount
    /// </summary>
    public decimal Discount { get; set; }
    
    /// <summary>
    /// Gets or sets the total discount
    /// </summary>
    public decimal TotalDiscount { get; set; }
    
    /// <summary>
    /// Gets or sets the total
    /// </summary>
    public decimal Total { get; set; }
}
