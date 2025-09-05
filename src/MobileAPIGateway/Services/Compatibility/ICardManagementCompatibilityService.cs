using MobileAPIGateway.Models.Compatibility.CardManagement;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for card management compatibility service
/// </summary>
public interface ICardManagementCompatibilityService
{
    /// <summary>
    /// Adds a card
    /// </summary>
    /// <param name="request">The add card request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The add card response</returns>
    Task<AddCardCompatibilityResponse> AddCardAsync(AddCardCompatibilityRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a card
    /// </summary>
    /// <param name="request">The update card request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The update card response</returns>
    Task<UpdateCardCompatibilityResponse> UpdateCardAsync(UpdateCardCompatibilityRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifies a card
    /// </summary>
    /// <param name="request">The card verification request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The card verification response</returns>
    Task<CardVerificationCompatibilityResponse> VerifyCardAsync(CardVerificationCompatibilityRequest request, CancellationToken cancellationToken = default);
}
