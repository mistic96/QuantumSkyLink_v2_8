namespace MobileAPIGateway.Models.Carts;

/// <summary>
/// Represents a request to update an existing shopping cart
/// </summary>
public class UpdateCartRequest
{
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string CurrencyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping address ID
    /// </summary>
    public string ShippingAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address ID
    /// </summary>
    public string BillingAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; }
    
    /// <summary>
    /// Gets or sets the coupon code
    /// </summary>
    public string CouponCode { get; set; }
    
    /// <summary>
    /// Gets or sets the notes
    /// </summary>
    public string Notes { get; set; }
}
