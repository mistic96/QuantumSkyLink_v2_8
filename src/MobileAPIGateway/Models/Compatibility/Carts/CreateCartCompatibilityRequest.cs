using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Carts;

/// <summary>
/// Create cart compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class CreateCartCompatibilityRequest
{
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
    public List<CartItemCompatibility>? Items { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Cart item compatibility model for compatibility with the old MobileOrchestrator
/// </summary>
public class CartItemCompatibility
{
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
    /// Gets or sets the quantity
    /// </summary>
    [Required]
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the unit price
    /// </summary>
    [Required]
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    [Required]
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
