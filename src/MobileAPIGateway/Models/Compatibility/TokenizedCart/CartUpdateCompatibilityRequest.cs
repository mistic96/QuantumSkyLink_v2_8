using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.TokenizedCart;

/// <summary>
/// Cart update compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class CartUpdateCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    [Required]
    public string CartId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the cart name
    /// </summary>
    public string? CartName { get; set; }
    
    /// <summary>
    /// Gets or sets the cart description
    /// </summary>
    public string? CartDescription { get; set; }
    
    /// <summary>
    /// Gets or sets the cart items
    /// </summary>
    public List<CartItemCompatibility>? CartItems { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string? CurrencyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the cart metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Cart item update compatibility model for compatibility with the old MobileOrchestrator
/// </summary>
public class CartItemUpdateCompatibility
{
    /// <summary>
    /// Gets or sets the cart item ID
    /// </summary>
    public string? CartItemId { get; set; }
    
    /// <summary>
    /// Gets or sets the product ID
    /// </summary>
    [Required]
    public string ProductId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the product name
    /// </summary>
    public string? ProductName { get; set; }
    
    /// <summary>
    /// Gets or sets the product description
    /// </summary>
    public string? ProductDescription { get; set; }
    
    /// <summary>
    /// Gets or sets the product image URL
    /// </summary>
    public string? ProductImageUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the product price
    /// </summary>
    public decimal ProductPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the product quantity
    /// </summary>
    public int Quantity { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the product currency code
    /// </summary>
    public string? CurrencyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the product metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
