// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using MobileAPIGateway.Clients;
// using MobileAPIGateway.Models.CardManagement;

// namespace MobileAPIGateway.Services;

// /// <summary>
// /// Service implementation for card management operations
// /// </summary>
// public class CardManagementService : ICardManagementService
// {
//     private readonly ICardManagementClient _cardManagementClient;
//     private readonly IUserContext _userContext;
//     private readonly ILogger<CardManagementService> _logger;
    
//     /// <summary>
//     /// Initializes a new instance of the <see cref="CardManagementService"/> class
//     /// </summary>
//     /// <param name="cardManagementClient">Card management client</param>
//     /// <param name="userContext">User context</param>
//     /// <param name="logger">Logger</param>
//     public CardManagementService(
//         ICardManagementClient cardManagementClient,
//         IUserContext userContext,
//         ILogger<CardManagementService> logger)
//     {
//         _cardManagementClient = cardManagementClient ?? throw new ArgumentNullException(nameof(cardManagementClient));
//         _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//     }
    
//     /// <inheritdoc />
//     public async Task<List<PaymentCard>> GetCardsAsync()
//     {
//         try
//         {
//             _logger.LogInformation("Getting payment cards for user {UserId}", _userContext.UserId);
//             return await _cardManagementClient.GetCardsAsync(_userContext.UserId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting payment cards for user {UserId}", _userContext.UserId);
//             throw;
//         }
//     }
    
//     /// <inheritdoc />
//     public async Task<PaymentCard> GetCardAsync(string cardId)
//     {
//         try
//         {
//             _logger.LogInformation("Getting payment card {CardId} for user {UserId}", cardId, _userContext.UserId);
//             return await _cardManagementClient.GetCardAsync(cardId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting payment card {CardId} for user {UserId}", cardId, _userContext.UserId);
//             throw;
//         }
//     }
    
//     /// <inheritdoc />
//     public async Task<CardResponse> AddCardAsync(AddCardRequest request)
//     {
//         try
//         {
//             _logger.LogInformation("Adding payment card for user {UserId}", _userContext.UserId);
//             return await _cardManagementClient.AddCardAsync(_userContext.UserId, request);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error adding payment card for user {UserId}", _userContext.UserId);
//             throw;
//         }
//     }
    
//     /// <inheritdoc />
//     public async Task<CardResponse> UpdateCardAsync(string cardId, UpdateCardRequest request)
//     {
//         try
//         {
//             _logger.LogInformation("Updating payment card {CardId} for user {UserId}", cardId, _userContext.UserId);
//             return await _cardManagementClient.UpdateCardAsync(cardId, request);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error updating payment card {CardId} for user {UserId}", cardId, _userContext.UserId);
//             throw;
//         }
//     }
    
//     /// <inheritdoc />
//     public async Task<CardResponse> DeleteCardAsync(string cardId)
//     {
//         try
//         {
//             _logger.LogInformation("Deleting payment card {CardId} for user {UserId}", cardId, _userContext.UserId);
//             return await _cardManagementClient.DeleteCardAsync(cardId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error deleting payment card {CardId} for user {UserId}", cardId, _userContext.UserId);
//             throw;
//         }
//     }
    
//     /// <inheritdoc />
//     public async Task<CardResponse> SetDefaultCardAsync(string cardId)
//     {
//         try
//         {
//             _logger.LogInformation("Setting payment card {CardId} as default for user {UserId}", cardId, _userContext.UserId);
//             return await _cardManagementClient.SetDefaultCardAsync(cardId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error setting payment card {CardId} as default for user {UserId}", cardId, _userContext.UserId);
//             throw;
//         }
//     }
    
//     /// <inheritdoc />
//     public async Task<CardResponse> VerifyCardAsync(CardVerificationRequest request)
//     {
//         try
//         {
//             _logger.LogInformation("Verifying payment card {CardId} for user {UserId}", request.CardId, _userContext.UserId);
//             return await _cardManagementClient.VerifyCardAsync(request);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error verifying payment card {CardId} for user {UserId}", request.CardId, _userContext.UserId);
//             throw;
//         }
//     }
// }
