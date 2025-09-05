namespace MobileAPIGateway.Models.Markets;

/// <summary>
/// Price tier model for tiered pricing strategy
/// </summary>
public class PriceTier
{
    /// <summary>
    /// Gets or sets the tier ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the minimum quantity for this tier
    /// </summary>
    public decimal MinQuantity { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum quantity for this tier
    /// </summary>
    public decimal? MaxQuantity { get; set; }
    
    /// <summary>
    /// Gets or sets the price for this tier
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the discount percentage for this tier
    /// </summary>
    public decimal? DiscountPercentage { get; set; }
    
    /// <summary>
    /// Gets or sets the tier name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the tier description
    /// </summary>
    public string? Description { get; set; }
}
