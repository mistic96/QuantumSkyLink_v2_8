using System;
using System.Collections.Generic;

namespace MobileAPIGateway.Models.Carts;

/// <summary>
/// Represents a shopping cart in the system
/// </summary>
public class ShoppingCart
{
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID who owns the cart
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the cart status (Active, Abandoned, Completed, etc.)
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the cart was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the cart was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the cart expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the cart items
    /// </summary>
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    
    /// <summary>
    /// Gets or sets the cart subtotal
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Gets or sets the cart tax
    /// </summary>
    public decimal Tax { get; set; }
    
    /// <summary>
    /// Gets or sets the cart shipping cost
    /// </summary>
    public decimal ShippingCost { get; set; }
    
    /// <summary>
    /// Gets or sets the cart discount
    /// </summary>
    public decimal Discount { get; set; }
    
    /// <summary>
    /// Gets or sets the cart total
    /// </summary>
    public decimal Total { get; set; }
    
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
