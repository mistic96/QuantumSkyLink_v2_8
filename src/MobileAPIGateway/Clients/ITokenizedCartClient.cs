using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using MobileAPIGateway.Models.TokenizedCart;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Client interface for tokenized cart operations
/// </summary>
public interface ITokenizedCartClient
{
    /// <summary>
    /// Gets all tokenized carts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tokenized carts</returns>
    [Get("/api/tokenized-carts/user/{userId}")]
    Task<IEnumerable<TokenizedCart>> GetUserCartsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a tokenized cart by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tokenized cart</returns>
    [Get("/api/tokenized-carts/user/{userId}/cart/{cartId}")]
    Task<TokenizedCart> GetCartByIdAsync(string userId, string cartId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Cart creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [Post("/api/tokenized-carts/user/{userId}")]
    Task<CartResponse> CreateCartAsync(string userId, [Body] CartCreationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [Put("/api/tokenized-carts/user/{userId}/cart/{cartId}")]
    Task<CartResponse> UpdateCartAsync(string userId, string cartId, [Body] CartUpdateRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds an item to a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart item request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [Post("/api/tokenized-carts/user/{userId}/cart/{cartId}/items")]
    Task<CartResponse> AddItemToCartAsync(string userId, string cartId, [Body] CartItemRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes an item from a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [Delete("/api/tokenized-carts/user/{userId}/cart/{cartId}/items/{itemId}")]
    Task<CartResponse> RemoveItemFromCartAsync(string userId, string cartId, string itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks out a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Cart checkout request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [Post("/api/tokenized-carts/user/{userId}/cart/{cartId}/checkout")]
    Task<CartResponse> CheckoutCartAsync(string userId, string cartId, [Body] CartCheckoutRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Abandons a tokenized cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cart response</returns>
    [Delete("/api/tokenized-carts/user/{userId}/cart/{cartId}")]
    Task<CartResponse> AbandonCartAsync(string userId, string cartId, CancellationToken cancellationToken = default);
}
