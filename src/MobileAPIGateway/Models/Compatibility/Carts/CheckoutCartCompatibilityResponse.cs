namespace MobileAPIGateway.Models.Compatibility.Carts;

/// <summary>
/// Checkout cart compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CheckoutCartCompatibilityResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the order ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    public string CartId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the payment method type
    /// </summary>
    public string? PaymentMethodType { get; set; }
    
    /// <summary>
    /// Gets or sets the payment status
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the order status
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the cart items
    /// </summary>
    public List<CartItemCompatibility>? Items { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping address
    /// </summary>
    public ShippingAddressCompatibility? ShippingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public BillingAddressCompatibility? BillingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the subtotal
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Gets or sets the tax
    /// </summary>
    public decimal Tax { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping cost
    /// </summary>
    public decimal ShippingCost { get; set; }
    
    /// <summary>
    /// Gets or sets the total
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
    
    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the last updated date
    /// </summary>
    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the payment transaction ID
    /// </summary>
    public string? PaymentTransactionId { get; set; }
    
    /// <summary>
    /// Gets or sets the payment receipt URL
    /// </summary>
    public string? PaymentReceiptUrl { get; set; }
}
