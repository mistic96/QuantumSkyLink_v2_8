using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Carts;

/// <summary>
/// Update cart compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class UpdateCartCompatibilityRequest
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
    public List<CartItemCompatibility>? Items { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
