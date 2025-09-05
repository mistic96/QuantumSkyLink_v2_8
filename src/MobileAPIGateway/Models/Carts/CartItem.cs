using System;

namespace MobileAPIGateway.Models.Carts;

/// <summary>
/// Represents an item in a shopping cart
/// </summary>
public class CartItem
{
    /// <summary>
    /// Gets or sets the item ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    public string CartId { get; set; }
    
    /// <summary>
    /// Gets or sets the product ID
    /// </summary>
    public string ProductId { get; set; }
    
    /// <summary>
    /// Gets or sets the product name
    /// </summary>
    public string ProductName { get; set; }
    
    /// <summary>
    /// Gets or sets the product SKU
    /// </summary>
    public string Sku { get; set; }
    
    /// <summary>
    /// Gets or sets the product image URL
    /// </summary>
    public string ImageUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the product description
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Gets or sets the product category
    /// </summary>
    public string Category { get; set; }
    
    /// <summary>
    /// Gets or sets the product price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the product quantity
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the product unit (e.g., each, kg, lb, etc.)
    /// </summary>
    public string Unit { get; set; }
    
    /// <summary>
    /// Gets or sets the product discount
    /// </summary>
    public decimal Discount { get; set; }
    
    /// <summary>
    /// Gets or sets the product tax
    /// </summary>
    public decimal Tax { get; set; }
    
    /// <summary>
    /// Gets or sets the product subtotal (price * quantity)
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Gets or sets the product total (subtotal - discount + tax)
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the item was added to the cart
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the item notes
    /// </summary>
    public string Notes { get; set; }
    
    /// <summary>
    /// Gets or sets whether the item is in stock
    /// </summary>
    public bool IsInStock { get; set; }
    
    /// <summary>
    /// Gets or sets the item options (JSON string)
    /// </summary>
    public string Options { get; set; }
}
