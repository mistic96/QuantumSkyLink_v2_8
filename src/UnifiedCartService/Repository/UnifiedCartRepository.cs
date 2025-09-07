using SurrealDb.Net;
using UnifiedCartService.Models.Entities;

namespace UnifiedCartService.Repository;

/// <summary>
/// SurrealDB repository implementation for unified cart operations
/// </summary>
public class UnifiedCartRepository : IUnifiedCartRepository
{
    private readonly ISurrealDbClient _surrealDb;
    private readonly ILogger<UnifiedCartRepository> _logger;

    public UnifiedCartRepository(ISurrealDbClient surrealDb, ILogger<UnifiedCartRepository> logger)
    {
        _surrealDb = surrealDb;
        _logger = logger;
    }

    public async Task<UnifiedCart> CreateCartAsync(UnifiedCart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _surrealDb.Create<UnifiedCart>("unified_carts", cart);
            _logger.LogInformation("Created cart {CartId} for user {UserId}", cart.Id, cart.UserId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create cart for user {UserId}", cart.UserId);
            throw;
        }
    }

    public async Task<UnifiedCart?> GetCartByIdAsync(string cartId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _surrealDb.Select<UnifiedCart>($"unified_carts:{cartId}");
            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cart {CartId}", cartId);
            return null;
        }
    }

    public async Task<IEnumerable<UnifiedCart>> GetUserCartsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = "SELECT * FROM unified_carts WHERE user_id = $userId AND status = 'Active' ORDER BY updated_at DESC";
            var result = await _surrealDb.RawQuery(query, new Dictionary<string, object?> { { "userId", userId } });
            
            var carts = result.GetValue<List<UnifiedCart>>(0) ?? [];
            return carts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get carts for user {UserId}", userId);
            return [];
        }
    }

    public async Task<UnifiedCart> UpdateCartAsync(UnifiedCart cart, CancellationToken cancellationToken = default)
    {
        try
        {
            cart.UpdatedAt = DateTimeOffset.UtcNow;
            cart.LastModified = DateTimeOffset.UtcNow;
            
            var result = await _surrealDb.Update<UnifiedCart>($"unified_carts:{cart.Id}", cart);
            _logger.LogInformation("Updated cart {CartId}", cart.Id);
            return result.FirstOrDefault() ?? cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update cart {CartId}", cart.Id);
            throw;
        }
    }

    public async Task DeleteCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Delete all items first
            await _surrealDb.RawQuery("DELETE cart_items WHERE cart_id = $cartId", new Dictionary<string, object?> { { "cartId", cartId } });
            
            // Delete the cart
            await _surrealDb.Delete($"unified_carts:{cartId}");
            
            _logger.LogInformation("Deleted cart {CartId} and all its items", cartId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete cart {CartId}", cartId);
            throw;
        }
    }

    public async Task<UnifiedCartItem> AddItemAsync(UnifiedCartItem item, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _surrealDb.Create<UnifiedCartItem>("cart_items", item);
            _logger.LogInformation("Added item {ItemId} to cart {CartId}", item.Id, item.CartId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add item to cart {CartId}", item.CartId);
            throw;
        }
    }

    public async Task<IEnumerable<UnifiedCartItem>> GetCartItemsAsync(string cartId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = "SELECT * FROM cart_items WHERE cart_id = $cartId ORDER BY added_at ASC";
            var result = await _surrealDb.RawQuery(query, new Dictionary<string, object?> { { "cartId", cartId } });
            
            var items = result.GetValue<List<UnifiedCartItem>>(0) ?? [];
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get items for cart {CartId}", cartId);
            return [];
        }
    }

    public async Task<UnifiedCartItem?> GetItemByIdAsync(string itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _surrealDb.Select<UnifiedCartItem>($"cart_items:{itemId}");
            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get item {ItemId}", itemId);
            return null;
        }
    }

    public async Task<UnifiedCartItem> UpdateItemAsync(UnifiedCartItem item, CancellationToken cancellationToken = default)
    {
        try
        {
            item.LastModified = DateTimeOffset.UtcNow;
            
            var result = await _surrealDb.Upsert<UnifiedCartItem>($"cart_items:{item.Id}", item);
            _logger.LogInformation("Updated item {ItemId} in cart {CartId}", item.Id, item.CartId);
            return result.FirstOrDefault() ?? item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update item {ItemId}", item.Id);
            throw;
        }
    }

    public async Task DeleteItemAsync(string itemId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _surrealDb.Delete($"cart_items:{itemId}");
            _logger.LogInformation("Deleted item {ItemId}", itemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete item {ItemId}", itemId);
            throw;
        }
    }

    public async Task<bool> LockCartForCheckoutAsync(string cartId, string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = @"
                UPDATE unified_carts:{cartId} 
                SET is_locked = true, 
                    checkout_session_id = $sessionId, 
                    status = 'CheckingOut',
                    updated_at = $now
                WHERE is_locked = false AND status = 'Active'
                RETURN AFTER";
            
            var result = await _surrealDb.RawQuery(query.Replace("{cartId}", cartId), new Dictionary<string, object?> 
            { 
                { "sessionId", sessionId }, 
                { "now", DateTimeOffset.UtcNow } 
            });
            
            var updatedCart = result.GetValue<UnifiedCart>(0);
            return updatedCart != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to lock cart {CartId} for checkout", cartId);
            return false;
        }
    }

    public async Task UnlockCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = @"
                UPDATE unified_carts:{cartId} 
                SET is_locked = false, 
                    checkout_session_id = null, 
                    status = 'Active',
                    updated_at = $now";
            
            await _surrealDb.RawQuery(query.Replace("{cartId}", cartId), new Dictionary<string, object?> 
            { 
                { "now", DateTimeOffset.UtcNow } 
            });
            
            _logger.LogInformation("Unlocked cart {CartId}", cartId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unlock cart {CartId}", cartId);
            throw;
        }
    }

    public async Task<int> CleanupExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = @"
                LET $expired_carts = (SELECT id FROM unified_carts WHERE expires_at < $now AND status = 'Active');
                DELETE cart_items WHERE cart_id IN $expired_carts;
                DELETE unified_carts WHERE id IN $expired_carts;
                RETURN count($expired_carts)";
            
            var result = await _surrealDb.RawQuery(query, new Dictionary<string, object?> { { "now", DateTimeOffset.UtcNow } });
            var count = result.GetValue<int>(0);
            
            if (count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired carts", count);
            }
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired carts");
            return 0;
        }
    }
}
