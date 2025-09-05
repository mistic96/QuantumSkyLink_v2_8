using MobileAPIGateway.Models.Compatibility.Markets;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for markets compatibility service
/// </summary>
public interface IMarketsCompatibilityService
{
    /// <summary>
    /// Gets the market list
    /// </summary>
    /// <param name="request">The market list request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The market list response</returns>
    Task<MarketListResponse> GetMarketListAsync(MarketListRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the trading pairs for a market
    /// </summary>
    /// <param name="request">The trading pair request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The trading pair response</returns>
    Task<TradingPairResponse> GetTradingPairsAsync(TradingPairRequest request, CancellationToken cancellationToken = default);
}
