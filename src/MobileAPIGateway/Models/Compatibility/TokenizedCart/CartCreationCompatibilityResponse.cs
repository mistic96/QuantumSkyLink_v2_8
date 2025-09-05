namespace MobileAPIGateway.Models.Compatibility.TokenizedCart;

/// <summary>
/// Cart creation compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CartCreationCompatibilityResponse
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
    /// Gets or sets the cart ID
    /// </summary>
    public string CartId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
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
    public List<CartItemCompatibilityResponse> CartItems { get; set; } = new List<CartItemCompatibilityResponse>();
    
    /// <summary>
    /// Gets or sets the cart subtotal
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Gets or sets the cart tax
    /// </summary>
    public decimal Tax { get; set; }
    
    /// <summary>
    /// Gets or sets the cart total
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code
    /// </summary>
    public string? CurrencyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the cart status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the cart metadata
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
    /// Gets or sets the expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
}

/// <summary>
/// Cart item compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CartItemCompatibilityResponse
{
    /// <summary>
    /// Gets or sets the cart item ID
    /// </summary>
    public string CartItemId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the product ID
    /// </summary>
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
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the product currency code
    /// </summary>
    public string? CurrencyCode { get; set; }
    
    /// <summary>
    /// Gets or sets the product metadata
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
}
