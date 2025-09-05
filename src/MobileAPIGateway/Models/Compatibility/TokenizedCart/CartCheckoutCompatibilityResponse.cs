namespace MobileAPIGateway.Models.Compatibility.TokenizedCart;

/// <summary>
/// Cart checkout compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CartCheckoutCompatibilityResponse
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
    /// Gets or sets the payment ID
    /// </summary>
    public string? PaymentId { get; set; }
    
    /// <summary>
    /// Gets or sets the payment status
    /// </summary>
    public string? PaymentStatus { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    public string? PaymentMethodId { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method type
    /// </summary>
    public string? PaymentMethodType { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public AddressCompatibility? BillingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping address
    /// </summary>
    public AddressCompatibility? ShippingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping method
    /// </summary>
    public string? ShippingMethod { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping cost
    /// </summary>
    public decimal? ShippingCost { get; set; }
    
    /// <summary>
    /// Gets or sets the discount code
    /// </summary>
    public string? DiscountCode { get; set; }
    
    /// <summary>
    /// Gets or sets the discount amount
    /// </summary>
    public decimal? DiscountAmount { get; set; }
    
    /// <summary>
    /// Gets or sets the subtotal
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Gets or sets the tax
    /// </summary>
    public decimal Tax { get; set; }
    
    /// <summary>
    /// Gets or sets the total
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string? CurrencyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the order status
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notes
    /// </summary>
    public string? Notes { get; set; }
    
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
    /// Gets or sets the cart items
    /// </summary>
    public List<CartItemCompatibilityResponse> CartItems { get; set; } = new List<CartItemCompatibilityResponse>();
}
