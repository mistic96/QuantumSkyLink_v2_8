// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using MobileAPIGateway.Models.CardManagement;
// using MobileAPIGateway.Services;

// namespace MobileAPIGateway.Controllers;

// /// <summary>
// /// Controller for card management operations
// /// </summary>
// [ApiController]
// [Authorize]
// [Route("api/cards")]
// public class CardManagementController : BaseController
// {
//     private readonly ICardManagementService _cardManagementService;
//     private readonly ILogger<CardManagementController> _logger;
    
//     /// <summary>
//     /// Initializes a new instance of the <see cref="CardManagementController"/> class
//     /// </summary>
//     /// <param name="cardManagementService">Card management service</param>
//     /// <param name="logger">Logger</param>
//     public CardManagementController(
//         ICardManagementService cardManagementService,
//         ILogger<CardManagementController> logger)
//     {
//         _cardManagementService = cardManagementService ?? throw new ArgumentNullException(nameof(cardManagementService));
//         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//     }
    
//     /// <summary>
//     /// Gets all payment cards for the current user
//     /// </summary>
//     /// <returns>List of payment cards</returns>
//     [HttpGet]
//     [ProducesResponseType(typeof(List<PaymentCard>), 200)]
//     [ProducesResponseType(401)]
//     [ProducesResponseType(500)]
//     public async Task<ActionResult<List<PaymentCard>>> GetCardsAsync()
//     {
//         try
//         {
//             _logger.LogInformation("Getting payment cards for user {UserId}", UserId);
//             var cards = await _cardManagementService.GetCardsAsync();
//             return Ok(cards);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting payment cards for user {UserId}", UserId);
//             return StatusCode(500, "An error occurred while getting payment cards");
//         }
//     }
    
//     /// <summary>
//     /// Gets a payment card by ID
//     /// </summary>
//     /// <param name="cardId">Card ID</param>
//     /// <returns>Payment card</returns>
//     [HttpGet("{cardId}")]
//     [ProducesResponseType(typeof(PaymentCard), 200)]
//     [ProducesResponseType(401)]
//     [ProducesResponseType(404)]
//     [ProducesResponseType(500)]
//     public async Task<ActionResult<PaymentCard>> GetCardAsync(string cardId)
//     {
//         try
//         {
//             _logger.LogInformation("Getting payment card {CardId} for user {UserId}", cardId, UserId);
//             var card = await _cardManagementService.GetCardAsync(cardId);
            
//             if (card == null)
//             {
//                 return NotFound($"Payment card with ID {cardId} not found");
//             }
            
//             return Ok(card);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error getting payment card {CardId} for user {UserId}", cardId, UserId);
//             return StatusCode(500, "An error occurred while getting the payment card");
//         }
//     }
    
//     /// <summary>
//     /// Adds a new payment card for the current user
//     /// </summary>
//     /// <param name="request">Add card request</param>
//     /// <returns>Card response</returns>
//     [HttpPost]
//     [ProducesResponseType(typeof(CardResponse), 201)]
//     [ProducesResponseType(400)]
//     [ProducesResponseType(401)]
//     [ProducesResponseType(500)]
//     public async Task<ActionResult<CardResponse>> AddCardAsync([FromBody] AddCardRequest request)
//     {
//         try
//         {
//             _logger.LogInformation("Adding payment card for user {UserId}", UserId);
//             var response = await _cardManagementService.AddCardAsync(request);
//             return CreatedAtAction(nameof(GetCardAsync), new { cardId = response.CardId }, response);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error adding payment card for user {UserId}", UserId);
//             return StatusCode(500, "An error occurred while adding the payment card");
//         }
//     }
    
//     /// <summary>
//     /// Updates an existing payment card
//     /// </summary>
//     /// <param name="cardId">Card ID</param>
//     /// <param name="request">Update card request</param>
//     /// <returns>Card response</returns>
//     [HttpPut("{cardId}")]
//     [ProducesResponseType(typeof(CardResponse), 200)]
//     [ProducesResponseType(400)]
//     [ProducesResponseType(401)]
//     [ProducesResponseType(404)]
//     [ProducesResponseType(500)]
//     public async Task<ActionResult<CardResponse>> UpdateCardAsync(string cardId, [FromBody] UpdateCardRequest request)
//     {
//         try
//         {
//             _logger.LogInformation("Updating payment card {CardId} for user {UserId}", cardId, UserId);
//             var response = await _cardManagementService.UpdateCardAsync(cardId, request);
//             return Ok(response);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error updating payment card {CardId} for user {UserId}", cardId, UserId);
//             return StatusCode(500, "An error occurred while updating the payment card");
//         }
//     }
    
//     /// <summary>
//     /// Deletes a payment card
//     /// </summary>
//     /// <param name="cardId">Card ID</param>
//     /// <returns>Card response</returns>
//     [HttpDelete("{cardId}")]
//     [ProducesResponseType(typeof(CardResponse), 200)]
//     [ProducesResponseType(401)]
//     [ProducesResponseType(404)]
//     [ProducesResponseType(500)]
//     public async Task<ActionResult<CardResponse>> DeleteCardAsync(string cardId)
//     {
//         try
//         {
//             _logger.LogInformation("Deleting payment card {CardId} for user {UserId}", cardId, UserId);
//             var response = await _cardManagementService.DeleteCardAsync(cardId);
//             return Ok(response);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error deleting payment card {CardId} for user {UserId}", cardId, UserId);
//             return StatusCode(500, "An error occurred while deleting the payment card");
//         }
//     }
    
//     /// <summary>
//     /// Sets a payment card as the default payment method
//     /// </summary>
//     /// <param name="cardId">Card ID</param>
//     /// <returns>Card response</returns>
//     [HttpPost("{cardId}/default")]
//     [ProducesResponseType(typeof(CardResponse), 200)]
//     [ProducesResponseType(401)]
//     [ProducesResponseType(404)]
//     [ProducesResponseType(500)]
//     public async Task<ActionResult<CardResponse>> SetDefaultCardAsync(string cardId)
//     {
//         try
//         {
//             _logger.LogInformation("Setting payment card {CardId} as default for user {UserId}", cardId, UserId);
//             var response = await _cardManagementService.SetDefaultCardAsync(cardId);
//             return Ok(response);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error setting payment card {CardId} as default for user {UserId}", cardId, UserId);
//             return StatusCode(500, "An error occurred while setting the payment card as default");
//         }
//     }
    
//     /// <summary>
//     /// Verifies a payment card
//     /// </summary>
//     /// <param name="request">Card verification request</param>
//     /// <returns>Card response</returns>
//     [HttpPost("verify")]
//     [ProducesResponseType(typeof(CardResponse), 200)]
//     [ProducesResponseType(400)]
//     [ProducesResponseType(401)]
//     [ProducesResponseType(404)]
//     [ProducesResponseType(500)]
//     public async Task<ActionResult<CardResponse>> VerifyCardAsync([FromBody] CardVerificationRequest request)
//     {
//         try
//         {
//             _logger.LogInformation("Verifying payment card {CardId} for user {UserId}", request.CardId, UserId);
//             var response = await _cardManagementService.VerifyCardAsync(request);
//             return Ok(response);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error verifying payment card {CardId} for user {UserId}", request.CardId, UserId);
//             return StatusCode(500, "An error occurred while verifying the payment card");
//         }
//     }
// }
