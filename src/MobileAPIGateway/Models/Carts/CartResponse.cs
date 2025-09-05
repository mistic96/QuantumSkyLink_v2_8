using System;

namespace MobileAPIGateway.Models.Carts;

/// <summary>
/// Represents a response for cart operations
/// </summary>
public class CartResponse
{
    /// <summary>
    /// Gets or sets the status of the operation
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public required string Message { get; set; }
    
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    public required string CartId { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp of the operation
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the cart
    /// </summary>
    public ShoppingCart? Cart { get; set; }
    
    /// <summary>
    /// Gets or sets the order ID (if the cart was checked out)
    /// </summary>
    public string? OrderId { get; set; }
    
    /// <summary>
    /// Gets or sets the payment URL (if the cart was checked out)
    /// </summary>
    public string? PaymentUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the error code (if any)
    /// </summary>
    public string? ErrorCode { get; set; }
    
    /// <summary>
    /// Gets or sets the error details (if any)
    /// </summary>
    public string? ErrorDetails { get; set; }
}
