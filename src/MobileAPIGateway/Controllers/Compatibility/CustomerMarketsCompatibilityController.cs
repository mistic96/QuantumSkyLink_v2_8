using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Compatibility.CustomerMarkets;
using MobileAPIGateway.Services.Compatibility;

namespace MobileAPIGateway.Controllers.Compatibility;

/// <summary>
/// Controller for customer markets compatibility with the old MobileOrchestrator
/// </summary>
[ApiController]
[Route("CustomerMarkets")]
public class CustomerMarketsCompatibilityController : ControllerBase
{
    private readonly ICustomerMarketsCompatibilityService _customerMarketsCompatibilityService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerMarketsCompatibilityController"/> class
    /// </summary>
    /// <param name="customerMarketsCompatibilityService">The customer markets compatibility service</param>
    public CustomerMarketsCompatibilityController(ICustomerMarketsCompatibilityService customerMarketsCompatibilityService)
    {
        _customerMarketsCompatibilityService = customerMarketsCompatibilityService ?? throw new ArgumentNullException(nameof(customerMarketsCompatibilityService));
    }
    
    /// <summary>
    /// Gets the customer market list
    /// </summary>
    /// <param name="request">The customer market list request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The customer market list response</returns>
    [HttpPost("GetCustomerMarketListAsync")]
    [Authorize]
    public async Task<ActionResult<CustomerMarketListResponse>> GetCustomerMarketListAsync(
        [FromBody] CustomerMarketListRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _customerMarketsCompatibilityService.GetCustomerMarketListAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Gets the customer trading pairs for a customer market
    /// </summary>
    /// <param name="request">The customer trading pair request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The customer trading pair response</returns>
    [HttpPost("GetCustomerTradingPairsAsync")]
    [Authorize]
    public async Task<ActionResult<CustomerTradingPairResponse>> GetCustomerTradingPairsAsync(
        [FromBody] CustomerTradingPairRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _customerMarketsCompatibilityService.GetCustomerTradingPairsAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
}
