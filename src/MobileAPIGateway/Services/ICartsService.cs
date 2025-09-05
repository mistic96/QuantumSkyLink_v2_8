using System.Collections.Generic;
using System.Threading.Tasks;
using MobileAPIGateway.Models.Carts;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service interface for cart operations
/// </summary>
public interface ICartsService
{
    /// <summary>
    /// Gets all carts for the current user
    /// </summary>
    /// <returns>List of shopping carts</returns>
    Task<List<ShoppingCart>> GetCartsAsync();
    
    /// <summary>
    /// Gets a cart by ID
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Shopping cart</returns>
    Task<ShoppingCart> GetCartAsync(string cartId);
    
    /// <summary>
    /// Creates a new cart for the current user
    /// </summary>
    /// <param name="request">Create cart request</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> CreateCartAsync(CreateCartRequest request);
    
    /// <summary>
    /// Updates an existing cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Update cart request</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> UpdateCartAsync(string cartId, UpdateCartRequest request);
    
    /// <summary>
    /// Deletes a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> DeleteCartAsync(string cartId);
    
    /// <summary>
    /// Adds an item to a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Create cart item request</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> AddCartItemAsync(string cartId, CreateCartItemRequest request);
    
    /// <summary>
    /// Updates an item in a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="request">Update cart item request</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> UpdateCartItemAsync(string cartId, string itemId, UpdateCartItemRequest request);
    
    /// <summary>
    /// Removes an item from a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="itemId">Item ID</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> RemoveCartItemAsync(string cartId, string itemId);
    
    /// <summary>
    /// Checks out a cart
    /// </summary>
    /// <param name="cartId">Cart ID</param>
    /// <param name="request">Checkout cart request</param>
    /// <returns>Cart response</returns>
    Task<CartResponse> CheckoutCartAsync(string cartId, CheckoutCartRequest request);
}
