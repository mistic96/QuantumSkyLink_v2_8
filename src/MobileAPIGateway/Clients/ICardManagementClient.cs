using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using MobileAPIGateway.Models.CardManagement;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Client interface for communicating with the PaymentGatewayService for card management operations
/// </summary>
public interface ICardManagementClient
{
    /// <summary>
    /// Gets all payment cards for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of payment cards</returns>
    [Get("/api/cards/user/{userId}")]
    Task<List<PaymentCard>> GetCardsAsync(string userId);
    
    /// <summary>
    /// Gets a payment card by ID
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <returns>Payment card</returns>
    [Get("/api/cards/{cardId}")]
    Task<PaymentCard> GetCardAsync(string cardId);
    
    /// <summary>
    /// Adds a new payment card
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Add card request</param>
    /// <returns>Card response</returns>
    [Post("/api/cards/user/{userId}")]
    Task<CardResponse> AddCardAsync(string userId, [Body] AddCardRequest request);
    
    /// <summary>
    /// Updates an existing payment card
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <param name="request">Update card request</param>
    /// <returns>Card response</returns>
    [Put("/api/cards/{cardId}")]
    Task<CardResponse> UpdateCardAsync(string cardId, [Body] UpdateCardRequest request);
    
    /// <summary>
    /// Deletes a payment card
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <returns>Card response</returns>
    [Delete("/api/cards/{cardId}")]
    Task<CardResponse> DeleteCardAsync(string cardId);
    
    /// <summary>
    /// Sets a payment card as the default payment method
    /// </summary>
    /// <param name="cardId">Card ID</param>
    /// <returns>Card response</returns>
    [Post("/api/cards/{cardId}/default")]
    Task<CardResponse> SetDefaultCardAsync(string cardId);
    
    /// <summary>
    /// Verifies a payment card
    /// </summary>
    /// <param name="request">Card verification request</param>
    /// <returns>Card response</returns>
    [Post("/api/cards/verify")]
    Task<CardResponse> VerifyCardAsync([Body] CardVerificationRequest request);
}
