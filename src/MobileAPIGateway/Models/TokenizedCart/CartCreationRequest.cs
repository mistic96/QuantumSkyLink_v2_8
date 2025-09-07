using System.Collections.Generic;

namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents a request to create a new tokenized cart
/// </summary>
public class CartCreationRequest
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
    /// Gets or sets the initial items to add to the cart
    /// </summary>
    public List<CartItemRequest> Items { get; set; } = new List<CartItemRequest>();
    
    /// <summary>
    /// Gets or sets the currency for the cart
    /// </summary>
    public string Currency { get; set; }
}
