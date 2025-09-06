using System;
using System.Collections.Generic;

namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents a response for unified cart operations
/// Includes market breakdown for mixed Primary and Secondary market carts
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
    
    /// <summary>
    /// Gets or sets the market breakdown showing items by market type
    /// </summary>
    public MarketBreakdown MarketBreakdown { get; set; }
    
    /// <summary>
    /// Gets or sets validation warnings if any
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// Gets or sets whether the cart is ready for checkout
    /// </summary>
    public bool IsCheckoutReady { get; set; }
    
    /// <summary>
    /// Gets or sets checkout readiness issues if any
    /// </summary>
    public List<CheckoutIssue> CheckoutIssues { get; set; } = new();
}

/// <summary>
/// Represents an issue preventing checkout
/// </summary>
public class CheckoutIssue
{
    /// <summary>
    /// Gets or sets the issue type
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Gets or sets the issue description
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Gets or sets the affected item ID if applicable
    /// </summary>
    public string? AffectedItemId { get; set; }
    
    /// <summary>
    /// Gets or sets the severity level
    /// </summary>
    public string Severity { get; set; } = "Warning";
}
