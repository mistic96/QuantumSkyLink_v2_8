namespace MobileAPIGateway.Models.Compatibility.Carts;

/// <summary>
/// Create cart compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class CreateCartCompatibilityResponse
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
    public List<CartItemCompatibility>? Items { get; set; }
    
    /// <summary>
    /// Gets or sets the cart status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
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
    /// Gets or sets the cart currency
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
}
