using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Markets;

namespace MobileAPIGateway.Services;

/// <summary>
/// Markets service
/// </summary>
public class MarketsService : IMarketsService
{
    private readonly IMarketsClient _marketsClient;
    private readonly ILogger<MarketsService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketsService"/> class
    /// </summary>
    /// <param name="marketsClient">Markets client</param>
    /// <param name="logger">Logger</param>
    public MarketsService(
        IMarketsClient marketsClient,
        ILogger<MarketsService> logger)
    {
        _marketsClient = marketsClient;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<Market>> GetMarketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all markets");
            
            var markets = await _marketsClient.GetMarketsAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} markets", markets.Count());
            
            return markets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting markets");
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Market> GetMarketAsync(string marketId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting market {MarketId}", marketId);
            
            var market = await _marketsClient.GetMarketAsync(marketId, cancellationToken);
            
            _logger.LogInformation("Retrieved market {MarketId}", marketId);
            
            return market;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market {MarketId}", marketId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<TradingPair>> GetTradingPairsAsync(string marketId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting trading pairs for market {MarketId}", marketId);
            
            var tradingPairs = await _marketsClient.GetTradingPairsAsync(marketId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} trading pairs for market {MarketId}", tradingPairs.Count(), marketId);
            
            return tradingPairs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trading pairs for market {MarketId}", marketId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<TradingPair> GetTradingPairAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting trading pair {Symbol}", symbol);
            
            var tradingPair = await _marketsClient.GetTradingPairAsync(symbol, cancellationToken);
            
            _logger.LogInformation("Retrieved trading pair {Symbol}", symbol);
            
            return tradingPair;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trading pair {Symbol}", symbol);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<Trade>> GetTradesAsync(string symbol, int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting trades for trading pair {Symbol} with limit {Limit}", symbol, limit);
            
            var trades = await _marketsClient.GetTradesAsync(symbol, limit, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} trades for trading pair {Symbol}", trades.Count(), symbol);
            
            return trades;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trades for trading pair {Symbol}", symbol);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<PriceTier>> GetPriceTiersAsync(string marketId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting price tiers for market {MarketId}", marketId);
            
            var priceTiers = await _marketsClient.GetPriceTiersAsync(marketId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} price tiers for market {MarketId}", priceTiers.Count(), marketId);
            
            return priceTiers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price tiers for market {MarketId}", marketId);
            throw;
        }
    }
}
