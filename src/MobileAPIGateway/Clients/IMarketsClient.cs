using Refit;
using MobileAPIGateway.Models.Markets;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Markets client interface
/// </summary>
public interface IMarketsClient
{
    /// <summary>
    /// Gets all markets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of markets</returns>
    [Get("/api/markets")]
    Task<IEnumerable<Market>> GetMarketsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a market by ID
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market</returns>
    [Get("/api/markets/{marketId}")]
    Task<Market> GetMarketAsync(string marketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all trading pairs for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trading pairs</returns>
    [Get("/api/markets/{marketId}/trading-pairs")]
    Task<IEnumerable<TradingPair>> GetTradingPairsAsync(string marketId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a trading pair by symbol
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trading pair</returns>
    [Get("/api/trading-pairs/{symbol}")]
    Task<TradingPair> GetTradingPairAsync(string symbol, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets recent trades for a trading pair
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="limit">Number of trades to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of trades</returns>
    [Get("/api/trading-pairs/{symbol}/trades")]
    Task<IEnumerable<Trade>> GetTradesAsync(string symbol, int limit = 50, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets price tiers for a market
    /// </summary>
    /// <param name="marketId">Market ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of price tiers</returns>
    [Get("/api/markets/{marketId}/price-tiers")]
    Task<IEnumerable<PriceTier>> GetPriceTiersAsync(string marketId, CancellationToken cancellationToken = default);
}
