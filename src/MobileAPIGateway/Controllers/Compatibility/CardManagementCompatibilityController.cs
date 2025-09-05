//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using MobileAPIGateway.Models.Compatibility.CardManagement;
//using MobileAPIGateway.Services.Compatibility;

//namespace MobileAPIGateway.Controllers.Compatibility;

///// <summary>
///// Controller for card management compatibility with the old MobileOrchestrator
///// </summary>
//[ApiController]
//[Route("CardManagement")]
//public class CardManagementCompatibilityController : ControllerBase
//{
//    private readonly ICardManagementCompatibilityService _cardManagementCompatibilityService;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="CardManagementCompatibilityController"/> class
//    /// </summary>
//    /// <param name="cardManagementCompatibilityService">The card management compatibility service</param>
//    public CardManagementCompatibilityController(ICardManagementCompatibilityService cardManagementCompatibilityService)
//    {
//        _cardManagementCompatibilityService = cardManagementCompatibilityService ?? throw new ArgumentNullException(nameof(cardManagementCompatibilityService));
//    }
    
//    /// <summary>
//    /// Adds a card
//    /// </summary>
//    /// <param name="request">The add card request</param>
//    /// <param name="cancellationToken">The cancellation token</param>
//    /// <returns>The add card response</returns>
//    [HttpPost("AddCardAsync")]
//    [Authorize]
//    public async Task<ActionResult<AddCardCompatibilityResponse>> AddCardAsync(
//        [FromBody] AddCardCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        var response = await _cardManagementCompatibilityService.AddCardAsync(request, cancellationToken);
        
//        if (!response.IsSuccessful)
//        {
//            return BadRequest(response);
//        }
        
//        return Ok(response);
//    }
    
//    /// <summary>
//    /// Updates a card
//    /// </summary>
//    /// <param name="request">The update card request</param>
//    /// <param name="cancellationToken">The cancellation token</param>
//    /// <returns>The update card response</returns>
//    [HttpPost("UpdateCardAsync")]
//    [Authorize]
//    public async Task<ActionResult<UpdateCardCompatibilityResponse>> UpdateCardAsync(
//        [FromBody] UpdateCardCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        var response = await _cardManagementCompatibilityService.UpdateCardAsync(request, cancellationToken);
        
//        if (!response.IsSuccessful)
//        {
//            return BadRequest(response);
//        }
        
//        return Ok(response);
//    }
    
//    /// <summary>
//    /// Verifies a card
//    /// </summary>
//    /// <param name="request">The card verification request</param>
//    /// <param name="cancellationToken">The cancellation token</param>
//    /// <returns>The card verification response</returns>
//    [HttpPost("VerifyCardAsync")]
//    [Authorize]
//    public async Task<ActionResult<CardVerificationCompatibilityResponse>> VerifyCardAsync(
//        [FromBody] CardVerificationCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        var response = await _cardManagementCompatibilityService.VerifyCardAsync(request, cancellationToken);
        
//        if (!response.IsSuccessful)
//        {
//            return BadRequest(response);
//        }
        
//        return Ok(response);
//    }
//}
