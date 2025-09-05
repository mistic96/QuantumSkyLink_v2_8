using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Compatibility.TokenizedCart;
using MobileAPIGateway.Services.Compatibility;

namespace MobileAPIGateway.Controllers.Compatibility;

/// <summary>
/// Controller for tokenized cart compatibility with the old MobileOrchestrator
/// </summary>
[ApiController]
[Route("TokenizedCart")]
public class TokenizedCartCompatibilityController : ControllerBase
{
    private readonly ITokenizedCartCompatibilityService _tokenizedCartCompatibilityService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenizedCartCompatibilityController"/> class
    /// </summary>
    /// <param name="tokenizedCartCompatibilityService">The tokenized cart compatibility service</param>
    public TokenizedCartCompatibilityController(ITokenizedCartCompatibilityService tokenizedCartCompatibilityService)
    {
        _tokenizedCartCompatibilityService = tokenizedCartCompatibilityService ?? throw new ArgumentNullException(nameof(tokenizedCartCompatibilityService));
    }
    
    /// <summary>
    /// Creates a cart
    /// </summary>
    /// <param name="request">The cart creation request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The cart creation response</returns>
    [HttpPost("CreateCartAsync")]
    [Authorize]
    public async Task<ActionResult<CartCreationCompatibilityResponse>> CreateCartAsync(
        [FromBody] CartCreationCompatibilityRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _tokenizedCartCompatibilityService.CreateCartAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Updates a cart
    /// </summary>
    /// <param name="request">The cart update request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The cart update response</returns>
    [HttpPost("UpdateCartAsync")]
    [Authorize]
    public async Task<ActionResult<CartUpdateCompatibilityResponse>> UpdateCartAsync(
        [FromBody] CartUpdateCompatibilityRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _tokenizedCartCompatibilityService.UpdateCartAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Checks out a cart
    /// </summary>
    /// <param name="request">The cart checkout request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The cart checkout response</returns>
    [HttpPost("CheckoutCartAsync")]
    [Authorize]
    public async Task<ActionResult<CartCheckoutCompatibilityResponse>> CheckoutCartAsync(
        [FromBody] CartCheckoutCompatibilityRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _tokenizedCartCompatibilityService.CheckoutCartAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
}
