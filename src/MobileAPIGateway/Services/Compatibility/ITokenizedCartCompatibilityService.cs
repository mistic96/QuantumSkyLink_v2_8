using MobileAPIGateway.Models.Compatibility.TokenizedCart;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for tokenized cart compatibility service
/// </summary>
public interface ITokenizedCartCompatibilityService
{
    /// <summary>
    /// Creates a cart
    /// </summary>
    /// <param name="request">The cart creation request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The cart creation response</returns>
    Task<CartCreationCompatibilityResponse> CreateCartAsync(CartCreationCompatibilityRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a cart
    /// </summary>
    /// <param name="request">The cart update request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The cart update response</returns>
    Task<CartUpdateCompatibilityResponse> UpdateCartAsync(CartUpdateCompatibilityRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks out a cart
    /// </summary>
    /// <param name="request">The cart checkout request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The cart checkout response</returns>
    Task<CartCheckoutCompatibilityResponse> CheckoutCartAsync(CartCheckoutCompatibilityRequest request, CancellationToken cancellationToken = default);
}
