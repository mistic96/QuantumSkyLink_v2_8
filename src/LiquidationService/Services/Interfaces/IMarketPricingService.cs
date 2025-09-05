using LiquidationService.Models.Responses;

namespace LiquidationService.Services.Interfaces;

/// <summary>
/// Service interface for market pricing and price discovery
/// </summary>
public interface IMarketPricingService
{
    /// <summary>
    /// Get current market price for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market price snapshot</returns>
    Task<MarketPriceSnapshotResponse> GetCurrentPriceAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get price with slippage estimation for a specific amount
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price snapshot with slippage estimation</returns>
    Task<MarketPriceSnapshotResponse> GetPriceWithSlippageAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical prices for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical price snapshots</returns>
    Task<IEnumerable<MarketPriceSnapshotResponse>> GetHistoricalPricesAsync(
        string assetSymbol, 
        string outputSymbol, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if a price is suitable for liquidation
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="maxSlippagePercent">Maximum acceptable slippage percentage</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price validation result</returns>
    Task<bool> ValidatePriceForLiquidationAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal amount, 
        decimal maxSlippagePercent = 5.0m, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get best available price from multiple sources
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Best price snapshot</returns>
    Task<MarketPriceSnapshotResponse> GetBestPriceAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update price data from external sources
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated price snapshot</returns>
    Task<MarketPriceSnapshotResponse> RefreshPriceDataAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get price confidence level for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confidence level (0-100)</returns>
    Task<int> GetPriceConfidenceLevelAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate estimated output amount for liquidation
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="assetAmount">Amount of asset to liquidate</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="includeSlippage">Whether to include slippage in calculation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Estimated output amount</returns>
    Task<decimal> CalculateEstimatedOutputAsync(
        string assetSymbol, 
        decimal assetAmount, 
        string outputSymbol, 
        bool includeSlippage = true, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get supported trading pairs
    /// </summary>
    /// <param name="assetSymbol">Asset symbol (optional filter)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported trading pairs</returns>
    Task<IEnumerable<string>> GetSupportedTradingPairsAsync(
        string? assetSymbol = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market depth for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market depth information</returns>
    Task<object> GetMarketDepthAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get price alerts and notifications
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="priceThreshold">Price threshold for alerts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price alert configuration</returns>
    Task<object> SetupPriceAlertAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal priceThreshold, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pricing statistics and analytics
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="fromDate">Start date for statistics</param>
    /// <param name="toDate">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Pricing statistics</returns>
    Task<object> GetPricingStatisticsAsync(
        string assetSymbol, 
        string outputSymbol, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default);
}
