using System.ComponentModel.DataAnnotations.Schema;
using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.Cards;

/// <summary>
/// Charge request service fee
/// </summary>
public sealed class ChargeRequestServiceFee
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the team ID
    /// </summary>
    public long TeamId { get; set; }
    
    /// <summary>
    /// Gets or sets the cart type
    /// </summary>
    public CloudCartType? CartType { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public CloudCheckoutCurrency Currency { get; set; } = CloudCheckoutCurrency.USD;
}
