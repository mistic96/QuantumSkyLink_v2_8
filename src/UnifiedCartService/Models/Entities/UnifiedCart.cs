using LiquidStorageCloud.Core.Database;

namespace UnifiedCartService.Models.Entities;

/// <summary>
/// Represents a unified shopping cart that supports both Primary and Secondary markets
/// </summary>
public class UnifiedCart : ISurrealEntity
{
    /// <inheritdoc/>
    public string Namespace => "quantumskylink";
    
    /// <inheritdoc/>
    public string TableName => "unified_carts";
    
    /// <inheritdoc/>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <inheritdoc/>
    public bool SolidState { get; set; }
    
    /// <inheritdoc/>
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets or sets the user ID who owns this cart
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the cart name
    /// </summary>
    public string Name { get; set; } = "Shopping Cart";
    
    /// <summary>
    /// Gets or sets the currency for this cart
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Gets or sets the cart status
    /// </summary>
    public CartStatus Status { get; set; } = CartStatus.Active;
    
    /// <summary>
    /// Gets or sets when the cart was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets or sets when the cart was last updated
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets or sets when the cart expires (auto-cleanup after this time)
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddDays(7);
    
    /// <summary>
    /// Gets or sets the total value of items in the cart
    /// </summary>
    public decimal TotalValue { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of items
    /// </summary>
    public int ItemCount { get; set; }
    
    /// <summary>
    /// Gets or sets whether the cart is locked for checkout
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// Gets or sets the checkout session ID if in checkout process
    /// </summary>
    public string? CheckoutSessionId { get; set; }
}

/// <summary>
/// Cart status enumeration
/// </summary>
public enum CartStatus
{
    /// <summary>
    /// Cart is active and can be modified
    /// </summary>
    Active,
    
    /// <summary>
    /// Cart is locked for checkout processing
    /// </summary>
    CheckingOut,
    
    /// <summary>
    /// Cart has been successfully checked out
    /// </summary>
    CheckedOut,
    
    /// <summary>
    /// Cart was abandoned by user
    /// </summary>
    Abandoned,
    
    /// <summary>
    /// Cart expired and was auto-cleaned
    /// </summary>
    Expired
}