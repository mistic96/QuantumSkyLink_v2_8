using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Micro service charge request fee
/// </summary>
public sealed class MicroServiceChargeRequestFee
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the cart type
    /// </summary>
    public CloudCartType? CartType { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public CloudCheckoutCurrency Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public FeeType Type { get; set; }
}
