using System.Text.Json;
using UnifiedCartService.Models.Entities;
using UnifiedCartService.Repository;

namespace UnifiedCartService.Services;

/// <summary>
/// Service implementation for unified cart operations with fail-fast approach
/// </summary>
public class UnifiedCartService : IUnifiedCartService
{
    private readonly IUnifiedCartRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UnifiedCartService> _logger;

    public UnifiedCartService(
        IUnifiedCartRepository repository,
        IHttpClientFactory httpClientFactory,
        ILogger<UnifiedCartService> logger)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<UnifiedCart>> GetUserCartsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        return await _repository.GetUserCartsAsync(userId, cancellationToken);
    }

    public async Task<UnifiedCart?> GetCartByIdAsync(string userId, string cartId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID is required", nameof(userId));
        if (string.IsNullOrEmpty(cartId))
            throw new ArgumentException("Cart ID is required", nameof(cartId));

        var cart = await _repository.GetCartByIdAsync(cartId, cancellationToken);
        
        // Verify ownership
        if (cart != null && cart.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to access cart {CartId} owned by {OwnerId}", 
                userId, cartId, cart.UserId);
            return null;
        }

        return cart;
    }

    public async Task<UnifiedCart> CreateCartAsync(string userId, string name, string currency, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("User ID is required", nameof(userId));

        var cart = new UnifiedCart
        {
            UserId = userId,
            Name = name ?? "Shopping Cart",
            Currency = currency ?? "USD",
            Status = CartStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        return await _repository.CreateCartAsync(cart, cancellationToken);
    }

    public async Task<UnifiedCart> UpdateCartAsync(string userId, string cartId, string? name, string? currency, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartByIdAsync(userId, cartId, cancellationToken);
        if (cart == null)
            throw new InvalidOperationException($"Cart {cartId} not found or access denied");

        if (cart.Status != CartStatus.Active)
            throw new InvalidOperationException($"Cannot update cart in {cart.Status} status");

        if (!string.IsNullOrEmpty(name))
            cart.Name = name;

        if (!string.IsNullOrEmpty(currency))
            cart.Currency = currency;

        return await _repository.UpdateCartAsync(cart, cancellationToken);
    }

    public async Task<UnifiedCart> AddItemToCartAsync(string userId, string cartId, UnifiedCartItem item, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartByIdAsync(userId, cartId, cancellationToken);
        if (cart == null)
            throw new InvalidOperationException($"Cart {cartId} not found or access denied");

        if (cart.Status != CartStatus.Active)
            throw new InvalidOperationException($"Cannot add items to cart in {cart.Status} status");

        if (cart.IsLocked)
            throw new InvalidOperationException("Cart is locked for checkout");

        // Validate item with marketplace service (fail fast)
        var isValid = await ValidateItemWithMarketplaceAsync(item, cancellationToken);
        if (!isValid)
            throw new InvalidOperationException($"Item {item.ListingId} is not available or price has changed");

        item.CartId = cartId;
        item.AddedAt = DateTimeOffset.UtcNow;
        item.IsValidated = true;
        item.LastValidatedAt = DateTimeOffset.UtcNow;

        await _repository.AddItemAsync(item, cancellationToken);

        // Recalculate totals
        return await RecalculateCartTotalsAsync(cartId, cancellationToken);
    }

    public async Task<UnifiedCart> RemoveItemFromCartAsync(string userId, string cartId, string itemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartByIdAsync(userId, cartId, cancellationToken);
        if (cart == null)
            throw new InvalidOperationException($"Cart {cartId} not found or access denied");

        if (cart.Status != CartStatus.Active)
            throw new InvalidOperationException($"Cannot remove items from cart in {cart.Status} status");

        if (cart.IsLocked)
            throw new InvalidOperationException("Cart is locked for checkout");

        await _repository.DeleteItemAsync(itemId, cancellationToken);

        // Recalculate totals
        return await RecalculateCartTotalsAsync(cartId, cancellationToken);
    }

    public async Task<UnifiedCart> UpdateItemQuantityAsync(string userId, string cartId, string itemId, decimal quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        var cart = await GetCartByIdAsync(userId, cartId, cancellationToken);
        if (cart == null)
            throw new InvalidOperationException($"Cart {cartId} not found or access denied");

        if (cart.Status != CartStatus.Active)
            throw new InvalidOperationException($"Cannot update cart in {cart.Status} status");

        if (cart.IsLocked)
            throw new InvalidOperationException("Cart is locked for checkout");

        var item = await _repository.GetItemByIdAsync(itemId, cancellationToken);
        if (item == null || item.CartId != cartId)
            throw new InvalidOperationException($"Item {itemId} not found in cart");

        item.Quantity = quantity;
        await _repository.UpdateItemAsync(item, cancellationToken);

        return await RecalculateCartTotalsAsync(cartId, cancellationToken);
    }

    public async Task<(bool isValid, List<string> errors)> ValidateCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var items = await _repository.GetCartItemsAsync(cartId, cancellationToken);

        if (!items.Any())
        {
            errors.Add("Cart is empty");
            return (false, errors);
        }

        // Validate each item with marketplace
        foreach (var item in items)
        {
            try
            {
                var isValid = await ValidateItemWithMarketplaceAsync(item, cancellationToken);
                if (!isValid)
                {
                    errors.Add($"Item {item.TokenName} (ID: {item.ListingId}) is no longer available or price has changed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate item {ItemId}", item.Id);
                errors.Add($"Failed to validate item {item.TokenName}");
            }
        }

        return (errors.Count == 0, errors);
    }

    public async Task AbandonCartAsync(string userId, string cartId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartByIdAsync(userId, cartId, cancellationToken);
        if (cart == null)
            throw new InvalidOperationException($"Cart {cartId} not found or access denied");

        if (cart.Status == CartStatus.CheckedOut)
            throw new InvalidOperationException("Cannot abandon a checked out cart");

        cart.Status = CartStatus.Abandoned;
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        
        await _repository.UpdateCartAsync(cart, cancellationToken);
    }

    public async Task<UnifiedCart> RecalculateCartTotalsAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var cart = await _repository.GetCartByIdAsync(cartId, cancellationToken);
        if (cart == null)
            throw new InvalidOperationException($"Cart {cartId} not found");

        var items = await _repository.GetCartItemsAsync(cartId, cancellationToken);
        
        cart.ItemCount = items.Count();
        cart.TotalValue = items.Sum(i => i.TotalValue);
        cart.UpdatedAt = DateTimeOffset.UtcNow;

        return await _repository.UpdateCartAsync(cart, cancellationToken);
    }

    private async Task<bool> ValidateItemWithMarketplaceAsync(UnifiedCartItem item, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MarketplaceService");
            var marketPath = item.MarketType == MarketType.Primary ? "primary" : "secondary";
            
            var response = await client.GetAsync($"/api/{marketPath}/listings/{item.ListingId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var listing = JsonSerializer.Deserialize<MarketplaceListing>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            // Validate price hasn't changed
            if (listing?.Price != item.PricePerUnit)
            {
                _logger.LogWarning("Price mismatch for listing {ListingId}: expected {Expected}, got {Actual}",
                    item.ListingId, item.PricePerUnit, listing?.Price);
                return false;
            }

            // Validate availability
            if (listing?.AvailableQuantity < item.Quantity)
            {
                _logger.LogWarning("Insufficient quantity for listing {ListingId}: requested {Requested}, available {Available}",
                    item.ListingId, item.Quantity, listing?.AvailableQuantity);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate item {ListingId} with marketplace", item.ListingId);
            return false;
        }
    }

    // Simple DTO for marketplace validation
    private class MarketplaceListing
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public decimal AvailableQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}