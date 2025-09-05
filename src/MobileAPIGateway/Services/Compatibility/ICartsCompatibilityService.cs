using MobileAPIGateway.Models.Compatibility.Carts;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for the carts compatibility service
/// </summary>
public interface ICartsCompatibilityService
{
    /// <summary>
    /// Creates a cart asynchronously
    /// </summary>
    /// <param name="request">The create cart request</param>
    /// <returns>The create cart response</returns>
    Task<CreateCartCompatibilityResponse> CreateCartAsync(CreateCartCompatibilityRequest request);
    
    /// <summary>
    /// Updates a cart asynchronously
    /// </summary>
    /// <param name="request">The update cart request</param>
    /// <returns>The update cart response</returns>
    Task<UpdateCartCompatibilityResponse> UpdateCartAsync(UpdateCartCompatibilityRequest request);
    
    /// <summary>
    /// Checks out a cart asynchronously
    /// </summary>
    /// <param name="request">The checkout cart request</param>
    /// <returns>The checkout cart response</returns>
    Task<CheckoutCartCompatibilityResponse> CheckoutCartAsync(CheckoutCartCompatibilityRequest request);
    
    /// <summary>
    /// Gets a cart asynchronously
    /// </summary>
    /// <param name="cartId">The cart ID</param>
    /// <param name="customerId">The customer ID</param>
    /// <returns>The cart response</returns>
    Task<CreateCartCompatibilityResponse> GetCartAsync(string cartId, string customerId);
    
    /// <summary>
    /// Deletes a cart asynchronously
    /// </summary>
    /// <param name="cartId">The cart ID</param>
    /// <param name="customerId">The customer ID</param>
    /// <returns>True if the cart was deleted successfully, otherwise false</returns>
    Task<bool> DeleteCartAsync(string cartId, string customerId);
}
