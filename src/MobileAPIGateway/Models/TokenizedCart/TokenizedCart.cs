using System;
using System.Collections.Generic;

namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents a tokenized cart containing tokenized assets
/// </summary>
public class TokenizedCart
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
    /// Gets or sets the cart name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the cart description
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Gets or sets the cart items
    /// </summary>
    public List<TokenizedCartItem> Items { get; set; } = new List<TokenizedCartItem>();
    
    /// <summary>
    /// Gets or sets the cart status
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the cart creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the cart last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the cart expiration date
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the total value of the cart
    /// </summary>
    public decimal TotalValue { get; set; }
    
    /// <summary>
    /// Gets or sets the currency of the total value
    /// </summary>
    public string Currency { get; set; }
}
