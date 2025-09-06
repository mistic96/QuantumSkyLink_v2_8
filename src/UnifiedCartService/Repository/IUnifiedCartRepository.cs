using UnifiedCartService.Models.Entities;

namespace UnifiedCartService.Repository;

/// <summary>
/// Repository interface for unified cart operations
/// </summary>
public interface IUnifiedCartRepository
{
    /// <summary>
    /// Creates a new cart
    /// </summary>
    Task<UnifiedCart> CreateCartAsync(UnifiedCart cart, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a cart by ID
    /// </summary>
    Task<UnifiedCart?> GetCartByIdAsync(string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active carts for a user
    /// </summary>
    Task<IEnumerable<UnifiedCart>> GetUserCartsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a cart
    /// </summary>
    Task<UnifiedCart> UpdateCartAsync(UnifiedCart cart, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a cart and all its items
    /// </summary>
    Task DeleteCartAsync(string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds an item to a cart
    /// </summary>
    Task<UnifiedCartItem> AddItemAsync(UnifiedCartItem item, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all items in a cart
    /// </summary>
    Task<IEnumerable<UnifiedCartItem>> GetCartItemsAsync(string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a specific item by ID
    /// </summary>
    Task<UnifiedCartItem?> GetItemByIdAsync(string itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a cart item
    /// </summary>
    Task<UnifiedCartItem> UpdateItemAsync(UnifiedCartItem item, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes an item from a cart
    /// </summary>
    Task DeleteItemAsync(string itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Locks a cart for checkout
    /// </summary>
    Task<bool> LockCartForCheckoutAsync(string cartId, string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unlocks a cart if checkout fails
    /// </summary>
    Task UnlockCartAsync(string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cleans up expired carts
    /// </summary>
    Task<int> CleanupExpiredCartsAsync(CancellationToken cancellationToken = default);
}