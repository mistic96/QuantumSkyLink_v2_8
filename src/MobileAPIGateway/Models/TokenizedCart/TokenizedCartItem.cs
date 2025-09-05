using System;

namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents an item in a tokenized cart
/// </summary>
public class TokenizedCartItem
{
    /// <summary>
    /// Gets or sets the item ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the cart ID this item belongs to
    /// </summary>
    public string CartId { get; set; }
    
    /// <summary>
    /// Gets or sets the token ID
    /// </summary>
    public string TokenId { get; set; }
    
    /// <summary>
    /// Gets or sets the token name
    /// </summary>
    public string TokenName { get; set; }
    
    /// <summary>
    /// Gets or sets the token symbol
    /// </summary>
    public string TokenSymbol { get; set; }
    
    /// <summary>
    /// Gets or sets the token type
    /// </summary>
    public string TokenType { get; set; }
    
    /// <summary>
    /// Gets or sets the token quantity
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the token price per unit
    /// </summary>
    public decimal PricePerUnit { get; set; }
    
    /// <summary>
    /// Gets or sets the currency of the price
    /// </summary>
    public string Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the total value of this item
    /// </summary>
    public decimal TotalValue { get; set; }
    
    /// <summary>
    /// Gets or sets the item added date
    /// </summary>
    public DateTime AddedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the item last update date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the token metadata
    /// </summary>
    public string Metadata { get; set; }
}
