using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.SecondaryMarkets;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Secondary markets controller
/// </summary>
[ApiController]
[Route("api/secondary-markets")]
public class SecondaryMarketsController : BaseController
{
    private readonly ISecondaryMarketsService _secondaryMarketsService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SecondaryMarketsController"/> class
    /// </summary>
    /// <param name="secondaryMarketsService">Secondary markets service</param>
    public SecondaryMarketsController(ISecondaryMarketsService secondaryMarketsService)
    {
        _secondaryMarketsService = secondaryMarketsService;
    }
    
    /// <summary>
    /// Gets the market listings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market listings</returns>
    [HttpGet("listings")]
    public async Task<ActionResult<List<MarketListing>>> GetMarketListingsAsync(CancellationToken cancellationToken = default)
    {
        var marketListings = await _secondaryMarketsService.GetMarketListingsAsync(cancellationToken);
        return Ok(marketListings);
    }
    
    /// <summary>
    /// Gets the market listing by ID
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market listing</returns>
    [HttpGet("listings/{id}")]
    public async Task<ActionResult<MarketListing>> GetMarketListingByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var marketListing = await _secondaryMarketsService.GetMarketListingByIdAsync(id, cancellationToken);
        return Ok(marketListing);
    }
    
    /// <summary>
    /// Creates a market listing
    /// </summary>
    /// <param name="marketListing">Market listing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created market listing</returns>
    [HttpPost("listings")]
    public async Task<ActionResult<MarketListing>> CreateMarketListingAsync([FromBody] MarketListing marketListing, CancellationToken cancellationToken = default)
    {
        var createdMarketListing = await _secondaryMarketsService.CreateMarketListingAsync(marketListing, cancellationToken);
        return CreatedAtAction(nameof(GetMarketListingByIdAsync), new { id = createdMarketListing.Id }, createdMarketListing);
    }
    
    /// <summary>
    /// Updates a market listing
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="marketListing">Market listing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated market listing</returns>
    [HttpPut("listings/{id}")]
    public async Task<ActionResult<MarketListing>> UpdateMarketListingAsync(string id, [FromBody] MarketListing marketListing, CancellationToken cancellationToken = default)
    {
        var updatedMarketListing = await _secondaryMarketsService.UpdateMarketListingAsync(id, marketListing, cancellationToken);
        return Ok(updatedMarketListing);
    }
    
    /// <summary>
    /// Deletes a market listing
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("listings/{id}")]
    public async Task<ActionResult> DeleteMarketListingAsync(string id, CancellationToken cancellationToken = default)
    {
        await _secondaryMarketsService.DeleteMarketListingAsync(id, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Gets the listing sales
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Listing sales</returns>
    [HttpGet("sales")]
    public async Task<ActionResult<List<ListingSale>>> GetListingSalesAsync(CancellationToken cancellationToken = default)
    {
        var listingSales = await _secondaryMarketsService.GetListingSalesAsync(cancellationToken);
        return Ok(listingSales);
    }
    
    /// <summary>
    /// Gets the listing sale by ID
    /// </summary>
    /// <param name="id">Listing sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Listing sale</returns>
    [HttpGet("sales/{id}")]
    public async Task<ActionResult<ListingSale>> GetListingSaleByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var listingSale = await _secondaryMarketsService.GetListingSaleByIdAsync(id, cancellationToken);
        return Ok(listingSale);
    }
    
    /// <summary>
    /// Creates a listing sale
    /// </summary>
    /// <param name="listingSale">Listing sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created listing sale</returns>
    [HttpPost("sales")]
    public async Task<ActionResult<ListingSale>> CreateListingSaleAsync([FromBody] ListingSale listingSale, CancellationToken cancellationToken = default)
    {
        var createdListingSale = await _secondaryMarketsService.CreateListingSaleAsync(listingSale, cancellationToken);
        return CreatedAtAction(nameof(GetListingSaleByIdAsync), new { id = createdListingSale.Id }, createdListingSale);
    }
}
