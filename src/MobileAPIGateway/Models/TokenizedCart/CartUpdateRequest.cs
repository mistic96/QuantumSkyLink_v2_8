namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents a request to update a tokenized cart
/// </summary>
public class CartUpdateRequest
{
    /// <summary>
    /// Gets or sets the cart name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the cart description
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Gets or sets the currency for the cart
    /// </summary>
    public string Currency { get; set; }
}
