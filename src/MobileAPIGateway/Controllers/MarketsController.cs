using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Markets;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Markets controller
/// </summary>
[ApiController]
[Route("api/markets")]
[Authorize]
public class MarketsController : BaseController
{
    private readonly IMarketsService _marketsService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketsController"/> class
    /// </summary>
    /// <param name="marketsService">Markets service</param>
    public MarketsController(IMarketsService marketsService)
    {
        _marketsService = marketsService;
    }
    
    /// <summary>
    /// Gets all markets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of markets</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Market>>> GetMarketsAsync(CancellationToken cancellationToken = default)
    {
        var markets = await _marketsService.GetMarketsAsync(cancellationToken);
        return Ok(markets);
    }
    
    /// <summary>
    /// Gets a market by ID
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market</returns>
    [HttpGet("{marketId}")]
    public async Task<ActionResult<Market>> GetMarketAsync(string marketId, CancellationToken cancellationToken = default)
    {
        var market = await _marketsService.GetMarketAsync(marketId, cancellationToken);
        return Ok(market);
    }
    
    /// <summary>
    /// Gets all trading pairs for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trading pairs</returns>
    [HttpGet("{marketId}/trading-pairs")]
    public async Task<ActionResult<IEnumerable<TradingPair>>> GetTradingPairsAsync(string marketId, CancellationToken cancellationToken = default)
    {
        var tradingPairs = await _marketsService.GetTradingPairsAsync(marketId, cancellationToken);
        return Ok(tradingPairs);
    }
    
    /// <summary>
    /// Gets a trading pair by symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trading pair</returns>
    [HttpGet("trading-pairs/{symbol}")]
    public async Task<ActionResult<TradingPair>> GetTradingPairAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var tradingPair = await _marketsService.GetTradingPairAsync(symbol, cancellationToken);
        return Ok(tradingPair);
    }
    
    /// <summary>
    /// Gets recent trades for a trading pair
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="limit">Number of trades to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trades</returns>
    [HttpGet("trading-pairs/{symbol}/trades")]
    public async Task<ActionResult<IEnumerable<Trade>>> GetTradesAsync(string symbol, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var trades = await _marketsService.GetTradesAsync(symbol, limit, cancellationToken);
        return Ok(trades);
    }
    
    /// <summary>
    /// Gets price tiers for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of price tiers</returns>
    [HttpGet("{marketId}/price-tiers")]
    public async Task<ActionResult<IEnumerable<PriceTier>>> GetPriceTiersAsync(string marketId, CancellationToken cancellationToken = default)
    {
        var priceTiers = await _marketsService.GetPriceTiersAsync(marketId, cancellationToken);
        return Ok(priceTiers);
    }
}
