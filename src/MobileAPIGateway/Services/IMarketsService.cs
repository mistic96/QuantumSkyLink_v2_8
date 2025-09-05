using MobileAPIGateway.Models.Markets;

namespace MobileAPIGateway.Services;

/// <summary>
/// Markets service interface
/// </summary>
public interface IMarketsService
{
    /// <summary>
    /// Gets all markets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of markets</returns>
    Task<IEnumerable<Market>> GetMarketsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a market by ID
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market</returns>
    Task<Market> GetMarketAsync(string marketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all trading pairs for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trading pairs</returns>
    Task<IEnumerable<TradingPair>> GetTradingPairsAsync(string marketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a trading pair by symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trading pair</returns>
    Task<TradingPair> GetTradingPairAsync(string symbol, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets recent trades for a trading pair
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="limit">Number of trades to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trades</returns>
    Task<IEnumerable<Trade>> GetTradesAsync(string symbol, int limit = 50, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets price tiers for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of price tiers</returns>
    Task<IEnumerable<PriceTier>> GetPriceTiersAsync(string marketId, CancellationToken cancellationToken = default);
}
