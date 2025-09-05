using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Accepted payment method
/// </summary>
public class AcceptedPaymentMethod
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method
    /// </summary>
    public ListingPaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Gets or sets the payment currency
    /// </summary>
    public CloudCheckoutCurrency PaymentCurrency { get; set; }
}
