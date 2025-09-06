using UnifiedCartService.Models.Entities;

namespace UnifiedCartService.Services;

/// <summary>
/// Service interface for unified cart operations
/// </summary>
public interface IUnifiedCartService
{
    /// <summary>
    /// Gets all carts for a user
    /// </summary>
    Task<IEnumerable<UnifiedCart>> GetUserCartsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a cart by ID
    /// </summary>
    Task<UnifiedCart?> GetCartByIdAsync(string userId, string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new cart
    /// </summary>
    Task<UnifiedCart> CreateCartAsync(string userId, string name, string currency, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a cart
    /// </summary>
    Task<UnifiedCart> UpdateCartAsync(string userId, string cartId, string? name, string? currency, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds an item to the cart
    /// </summary>
    Task<UnifiedCart> AddItemToCartAsync(string userId, string cartId, UnifiedCartItem item, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes an item from the cart
    /// </summary>
    Task<UnifiedCart> RemoveItemFromCartAsync(string userId, string cartId, string itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an item quantity
    /// </summary>
    Task<UnifiedCart> UpdateItemQuantityAsync(string userId, string cartId, string itemId, decimal quantity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates all items in the cart
    /// </summary>
    Task<(bool isValid, List<string> errors)> ValidateCartAsync(string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Abandons a cart
    /// </summary>
    Task AbandonCartAsync(string userId, string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recalculates cart totals
    /// </summary>
    Task<UnifiedCart> RecalculateCartTotalsAsync(string cartId, CancellationToken cancellationToken = default);
}