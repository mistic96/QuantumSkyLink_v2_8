using MarketplaceService.Data.Entities;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Service interface for marketplace analytics and statistics
/// </summary>
public interface IMarketAnalyticsService
{
    /// <summary>
    /// Get trading volume statistics
    /// </summary>
    /// <param name="period">Time period for statistics</param>
    /// <param name="assetType">Optional asset type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trading volume statistics</returns>
    Task<TradingVolumeStats> GetTradingVolumeStatsAsync(TimePeriod period, AssetType? assetType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get marketplace performance metrics
    /// </summary>
    /// <param name="period">Time period for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance metrics</returns>
    Task<MarketplacePerformanceMetrics> GetPerformanceMetricsAsync(TimePeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user activity statistics
    /// </summary>
    /// <param name="period">Time period for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User activity statistics</returns>
    Task<UserActivityStats> GetUserActivityStatsAsync(TimePeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top performing listings
    /// </summary>
    /// <param name="period">Time period for analysis</param>
    /// <param name="metric">Metric to rank by</param>
    /// <param name="limit">Maximum number of listings to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top performing listings</returns>
    Task<IEnumerable<ListingPerformanceDto>> GetTopPerformingListingsAsync(TimePeriod period, PerformanceMetric metric, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get price trend analysis for an asset
    /// </summary>
    /// <param name="assetType">Asset type</param>
    /// <param name="assetSymbol">Asset symbol (for external assets)</param>
    /// <param name="tokenId">Token ID (for platform tokens)</param>
    /// <param name="period">Time period for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price trend analysis</returns>
    Task<PriceTrendAnalysis> GetPriceTrendAnalysisAsync(AssetType assetType, string? assetSymbol = null, Guid? tokenId = null, TimePeriod period = TimePeriod.Month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get market depth analysis for active listings
    /// </summary>
    /// <param name="assetType">Asset type</param>
    /// <param name="assetSymbol">Asset symbol (for external assets)</param>
    /// <param name="tokenId">Token ID (for platform tokens)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market depth analysis</returns>
    Task<MarketDepthAnalysis> GetMarketDepthAnalysisAsync(AssetType assetType, string? assetSymbol = null, Guid? tokenId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get revenue analytics for the marketplace
    /// </summary>
    /// <param name="period">Time period for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Revenue analytics</returns>
    Task<RevenueAnalytics> GetRevenueAnalyticsAsync(TimePeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversion funnel analysis
    /// </summary>
    /// <param name="period">Time period for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversion funnel metrics</returns>
    Task<ConversionFunnelMetrics> GetConversionFunnelAsync(TimePeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate marketplace health report
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Marketplace health report</returns>
    Task<MarketplaceHealthReport> GenerateHealthReportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get real-time marketplace statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Real-time statistics</returns>
    Task<RealTimeMarketStats> GetRealTimeStatsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Enums for analytics service
/// </summary>
public enum TimePeriod
{
    Hour = 1,
    Day = 2,
    Week = 3,
    Month = 4,
    Quarter = 5,
    Year = 6,
    AllTime = 7
}

public enum PerformanceMetric
{
    Volume = 1,
    Revenue = 2,
    Orders = 3,
    Views = 4,
    ConversionRate = 5
}

/// <summary>
/// Data transfer objects for analytics service
/// </summary>
public class TradingVolumeStats
{
    public TimePeriod Period { get; set; }
    public decimal TotalVolume { get; set; }
    public string Currency { get; set; } = "USD";
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionSize { get; set; }
    public decimal VolumeChange { get; set; }
    public decimal VolumeChangePercentage { get; set; }
    public IEnumerable<VolumeDataPoint> VolumeHistory { get; set; } = new List<VolumeDataPoint>();
    public DateTime GeneratedAt { get; set; }
}

public class VolumeDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Volume { get; set; }
    public int TransactionCount { get; set; }
}

public class MarketplacePerformanceMetrics
{
    public TimePeriod Period { get; set; }
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int CompletedOrders { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFees { get; set; }
    public TimeSpan AverageOrderProcessingTime { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class UserActivityStats
{
    public TimePeriod Period { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }
    public int ReturningUsers { get; set; }
    public decimal UserRetentionRate { get; set; }
    public decimal AverageSessionDuration { get; set; }
    public int TotalSellers { get; set; }
    public int TotalBuyers { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class ListingPerformanceDto
{
    public Guid ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string? AssetSymbol { get; set; }
    public decimal Volume { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public long ViewCount { get; set; }
    public decimal ConversionRate { get; set; }
    public PerformanceMetric RankedBy { get; set; }
    public int Rank { get; set; }
}

public class PriceTrendAnalysis
{
    public AssetType AssetType { get; set; }
    public string? AssetSymbol { get; set; }
    public Guid? TokenId { get; set; }
    public TimePeriod Period { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal StartPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal PriceChange { get; set; }
    public decimal PriceChangePercentage { get; set; }
    public decimal Volatility { get; set; }
    public string Trend { get; set; } = "Stable"; // Bullish, Bearish, Stable
    public IEnumerable<PriceDataPoint> PriceHistory { get; set; } = new List<PriceDataPoint>();
    public DateTime GeneratedAt { get; set; }
}

public class PriceDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
}

public class MarketDepthAnalysis
{
    public AssetType AssetType { get; set; }
    public string? AssetSymbol { get; set; }
    public Guid? TokenId { get; set; }
    public IEnumerable<OrderBookEntry> BuyOrders { get; set; } = new List<OrderBookEntry>();
    public IEnumerable<OrderBookEntry> SellOrders { get; set; } = new List<OrderBookEntry>();
    public decimal BestBidPrice { get; set; }
    public decimal BestAskPrice { get; set; }
    public decimal Spread { get; set; }
    public decimal SpreadPercentage { get; set; }
    public decimal TotalBuyVolume { get; set; }
    public decimal TotalSellVolume { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class OrderBookEntry
{
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalValue { get; set; }
    public int OrderCount { get; set; }
}

public class RevenueAnalytics
{
    public TimePeriod Period { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal ListingFees { get; set; }
    public decimal TransactionFees { get; set; }
    public decimal EscrowFees { get; set; }
    public decimal RevenueGrowth { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    public IEnumerable<RevenueDataPoint> RevenueHistory { get; set; } = new List<RevenueDataPoint>();
    public DateTime GeneratedAt { get; set; }
}

public class RevenueDataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Revenue { get; set; }
    public decimal Fees { get; set; }
}

public class ConversionFunnelMetrics
{
    public TimePeriod Period { get; set; }
    public int ListingViews { get; set; }
    public int OrdersStarted { get; set; }
    public int OrdersCompleted { get; set; }
    public decimal ViewToOrderRate { get; set; }
    public decimal OrderCompletionRate { get; set; }
    public decimal OverallConversionRate { get; set; }
    public TimeSpan AverageTimeToOrder { get; set; }
    public TimeSpan AverageOrderDuration { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class MarketplaceHealthReport
{
    public bool IsHealthy { get; set; }
    public decimal HealthScore { get; set; } // 0-100
    public int ActiveListings { get; set; }
    public int PendingOrders { get; set; }
    public int ActiveEscrows { get; set; }
    public int DisputedEscrows { get; set; }
    public decimal SystemUptime { get; set; }
    public decimal AverageResponseTime { get; set; }
    public int ErrorRate { get; set; }
    public IEnumerable<HealthIssue> Issues { get; set; } = new List<HealthIssue>();
    public IEnumerable<HealthRecommendation> Recommendations { get; set; } = new List<HealthRecommendation>();
    public DateTime GeneratedAt { get; set; }
}

public class HealthIssue
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical
    public DateTime DetectedAt { get; set; }
}

public class HealthRecommendation
{
    public string Category { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string Priority { get; set; } = "Low"; // Low, Medium, High
    public string ExpectedImpact { get; set; } = string.Empty;
}

public class RealTimeMarketStats
{
    public int ActiveListings { get; set; }
    public int OnlineUsers { get; set; }
    public int OrdersLast24h { get; set; }
    public decimal VolumeLast24h { get; set; }
    public int PendingOrders { get; set; }
    public int ActiveEscrows { get; set; }
    public decimal AverageOrderValue { get; set; }
    public string MostTradedAsset { get; set; } = string.Empty;
    public decimal SystemLoad { get; set; }
    public DateTime LastUpdated { get; set; }
}
