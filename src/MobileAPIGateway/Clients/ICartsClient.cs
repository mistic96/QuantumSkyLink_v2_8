using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Client interface for communicating with the backend service for cart operations
/// </summary>
public interface ICartsClient
{
    /// <summary>
    /// Gets all carts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of shopping carts</returns>
    [Get("/api/carts/user/{userId}")]
    Task<List<ShoppingCart>> GetCartsAsync(string userId);
    
    /// <summary>
    /// Gets a cart by ID
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Shopping cart</returns>
    [Get("/api/carts/{cartId}")]
    Task<ShoppingCart> GetCartAsync(string cartId);
    
    /// <summary>
    /// Creates a new cart
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Create cart request</param>
    /// <returns>Cart response</returns>
    [Post("/api/carts/user/{userId}")]
    Task<CartResponse> CreateCartAsync(string userId, [Body] CreateCartRequest request);
    
    /// <summary>
    /// Updates an existing cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Update cart request</param>
    /// <returns>Cart response</returns>
    [Put("/api/carts/{cartId}")]
    Task<CartResponse> UpdateCartAsync(string cartId, [Body] UpdateCartRequest request);
    
    /// <summary>
    /// Deletes a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Cart response</returns>
    [Delete("/api/carts/{cartId}")]
    Task<CartResponse> DeleteCartAsync(string cartId);
    
    /// <summary>
    /// Adds an item to a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Create cart item request</param>
    /// <returns>Cart response</returns>
    [Post("/api/carts/{cartId}/items")]
    Task<CartResponse> AddCartItemAsync(string cartId, [Body] CreateCartItemRequest request);
    
    /// <summary>
    /// Updates an item in a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="request">Update cart item request</param>
    /// <returns>Cart response</returns>
    [Put("/api/carts/{cartId}/items/{itemId}")]
    Task<CartResponse> UpdateCartItemAsync(string cartId, string itemId, [Body] UpdateCartItemRequest request);
    
    /// <summary>
    /// Removes an item from a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="itemId">Item ID</param>
    /// <returns>Cart response</returns>
    [Delete("/api/carts/{cartId}/items/{itemId}")]
    Task<CartResponse> RemoveCartItemAsync(string cartId, string itemId);
    
    /// <summary>
    /// Checks out a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Checkout cart request</param>
    /// <returns>Cart response</returns>
    [Post("/api/carts/{cartId}/checkout")]
    Task<CartResponse> CheckoutCartAsync(string cartId, [Body] CheckoutCartRequest request);
}
