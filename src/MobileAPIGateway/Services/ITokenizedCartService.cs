using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service interface for tokenized cart operations
/// </summary>
public interface ITokenizedCartService
{
    /// <summary>
    /// Gets all tokenized carts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tokenized carts</returns>
    Task<IEnumerable<TokenizedCart>> GetUserCartsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a tokenized cart by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tokenized cart</returns>
    Task<TokenizedCart> GetCartByIdAsync(string userId, string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Cart creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> CreateCartAsync(string userId, CartCreationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> UpdateCartAsync(string userId, string cartId, CartUpdateRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds an item to a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart item request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> AddItemToCartAsync(string userId, string cartId, CartItemRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes an item from a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> RemoveItemFromCartAsync(string userId, string cartId, string itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks out a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart checkout request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> CheckoutCartAsync(string userId, string cartId, CartCheckoutRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Abandons a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> AbandonCartAsync(string userId, string cartId, CancellationToken cancellationToken = default);
}
