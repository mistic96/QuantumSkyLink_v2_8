using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service implementation for tokenized cart operations
/// </summary>
public class TokenizedCartService : ITokenizedCartService
{
    private readonly ITokenizedCartClient _tokenizedCartClient;
    private readonly ILogger<TokenizedCartService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenizedCartService"/> class
    /// </summary>
    /// <param name="tokenizedCartClient">Tokenized cart client</param>
    /// <param name="logger">Logger</param>
    public TokenizedCartService(ITokenizedCartClient tokenizedCartClient, ILogger<TokenizedCartService> logger)
    {
        _tokenizedCartClient = tokenizedCartClient;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<TokenizedCart>> GetUserCartsAsync(string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting carts for user {UserId}", userId);
        return await _tokenizedCartClient.GetUserCartsAsync(userId, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<TokenizedCart> GetCartByIdAsync(string userId, string cartId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting cart {CartId} for user {UserId}", cartId, userId);
        return await _tokenizedCartClient.GetCartByIdAsync(userId, cartId, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<CartResponse> CreateCartAsync(string userId, CartCreationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating cart for user {UserId}", userId);
        return await _tokenizedCartClient.CreateCartAsync(userId, request, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<CartResponse> UpdateCartAsync(string userId, string cartId, CartUpdateRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating cart {CartId} for user {UserId}", cartId, userId);
        return await _tokenizedCartClient.UpdateCartAsync(userId, cartId, request, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<CartResponse> AddItemToCartAsync(string userId, string cartId, CartItemRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding item to cart {CartId} for user {UserId}", cartId, userId);
        return await _tokenizedCartClient.AddItemToCartAsync(userId, cartId, request, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<CartResponse> RemoveItemFromCartAsync(string userId, string cartId, string itemId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing item {ItemId} from cart {CartId} for user {UserId}", itemId, cartId, userId);
        return await _tokenizedCartClient.RemoveItemFromCartAsync(userId, cartId, itemId, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<CartResponse> CheckoutCartAsync(string userId, string cartId, CartCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking out cart {CartId} for user {UserId}", cartId, userId);
        return await _tokenizedCartClient.CheckoutCartAsync(userId, cartId, request, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<CartResponse> AbandonCartAsync(string userId, string cartId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Abandoning cart {CartId} for user {UserId}", cartId, userId);
        return await _tokenizedCartClient.AbandonCartAsync(userId, cartId, cancellationToken);
    }
}
