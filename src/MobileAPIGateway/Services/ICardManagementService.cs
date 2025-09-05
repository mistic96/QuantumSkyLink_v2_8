using System.Collections.Generic;
using System.Threading.Tasks;
using MobileAPIGateway.Models.CardManagement;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service interface for card management operations
/// </summary>
public interface ICardManagementService
{
    /// <summary>
    /// Gets all payment cards for the current user
    /// </summary>
    /// <returns>List of payment cards</returns>
    Task<List<PaymentCard>> GetCardsAsync();
    
    /// <summary>
    /// Gets a payment card by ID
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <returns>Payment card</returns>
    Task<PaymentCard> GetCardAsync(string cardId);
    
    /// <summary>
    /// Adds a new payment card for the current user
    /// </summary>
    /// <param name="request">Add card request</param>
    /// <returns>Card response</returns>
    Task<CardResponse> AddCardAsync(AddCardRequest request);
    
    /// <summary>
    /// Updates an existing payment card
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <param name="request">Update card request</param>
    /// <returns>Card response</returns>
    Task<CardResponse> UpdateCardAsync(string cardId, UpdateCardRequest request);
    
    /// <summary>
    /// Deletes a payment card
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <returns>Card response</returns>
    Task<CardResponse> DeleteCardAsync(string cardId);
    
    /// <summary>
    /// Sets a payment card as the default payment method
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <returns>Card response</returns>
    Task<CardResponse> SetDefaultCardAsync(string cardId);
    
    /// <summary>
    /// Verifies a payment card
    /// </summary>
    /// <param name="request">Card verification request</param>
    /// <returns>Card response</returns>
    Task<CardResponse> VerifyCardAsync(CardVerificationRequest request);
}
