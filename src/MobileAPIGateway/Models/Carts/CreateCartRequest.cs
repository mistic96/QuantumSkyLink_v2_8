using System.Collections.Generic;

namespace MobileAPIGateway.Models.Carts;

/// <summary>
/// Represents a request to create a new shopping cart
/// </summary>
public class CreateCartRequest
{
    /// <summary>
    /// Gets or sets the initial cart items
    /// </summary>
    public List<CreateCartItemRequest> Items { get; set; } = new List<CreateCartItemRequest>();
    
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
    /// Gets or sets the coupon code
    /// </summary>
    public string CouponCode { get; set; }
    
    /// <summary>
    /// Gets or sets the notes
    /// </summary>
    public string Notes { get; set; }
}

/// <summary>
/// Represents a request to create a new cart item
/// </summary>
public class CreateCartItemRequest
{
    /// <summary>
    /// Gets or sets the product ID
    /// </summary>
    public string ProductId { get; set; }
    
    /// <summary>
    /// Gets or sets the product quantity
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the item notes
    /// </summary>
    public string Notes { get; set; }
    
    /// <summary>
    /// Gets or sets the item options (JSON string)
    /// </summary>
    public string Options { get; set; }
}
