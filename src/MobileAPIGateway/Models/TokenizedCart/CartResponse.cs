using System;

namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents a response for cart operations
/// </summary>
public class CartResponse
{
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the operation status
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the operation message
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Gets or sets the transaction ID if applicable
    /// </summary>
    public string TransactionId { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of the operation
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the cart data
    /// </summary>
    public TokenizedCart Cart { get; set; }
}
