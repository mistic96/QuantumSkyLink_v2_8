namespace MobileAPIGateway.Models.Carts;

/// <summary>
/// Represents a request to checkout a shopping cart
/// </summary>
public class CheckoutCartRequest
{
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
    
    /// <summary>
    /// Gets or sets the shipping method
    /// </summary>
    public string ShippingMethod { get; set; }
    
    /// <summary>
    /// Gets or sets whether to save payment method for future use
    /// </summary>
    public bool SavePaymentMethod { get; set; }
    
    /// <summary>
    /// Gets or sets whether to save shipping address for future use
    /// </summary>
    public bool SaveShippingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets whether to save billing address for future use
    /// </summary>
    public bool SaveBillingAddress { get; set; }
}
