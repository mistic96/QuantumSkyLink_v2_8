using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services;

public class MarketAnalyticsService : IMarketAnalyticsService
{
    private readonly MarketplaceDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MarketAnalyticsService> _logger;

    public MarketAnalyticsService(
        MarketplaceDbContext context,
        IDistributedCache cache,
        ILogger<MarketAnalyticsService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TradingVolumeStats> GetTradingVolumeStatsAsync(TimePeriod period, AssetType? assetType = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting trading volume stats for period {Period} and asset type {AssetType}", period, assetType);

        var (startDate, endDate) = GetDateRangeForPeriod(period);
        
        var query = _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == OrderStatus.Completed);

        if (assetType.HasValue)
        {
            query = query.Where(o => o.MarketListing!.AssetType == assetType.Value);
        }

        var orders = await query.ToListAsync(cancellationToken);
        
        var totalVolume = orders.Sum(o => o.TotalAmount);
        var totalTransactions = orders.Count;
        var averageTransactionSize = totalTransactions > 0 ? totalVolume / totalTransactions : 0;

        // Calculate previous period for comparison
        var previousPeriodStart = GetPreviousPeriodStart(startDate, period);
        var previousOrders = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.CreatedAt >= previousPeriodStart && o.CreatedAt < startDate && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        var previousVolume = previousOrders.Sum(o => o.TotalAmount);
        var volumeChange = totalVolume - previousVolume;
        var volumeChangePercentage = previousVolume > 0 ? (volumeChange / previousVolume) * 100 : 0;

        // Generate volume history data points
        var volumeHistory = await GenerateVolumeHistoryAsync(startDate, endDate, assetType, cancellationToken);

        return new TradingVolumeStats
        {
            Period = period,
            TotalVolume = totalVolume,
            Currency = "USD",
            TotalTransactions = totalTransactions,
            AverageTransactionSize = averageTransactionSize,
            VolumeChange = volumeChange,
            VolumeChangePercentage = volumeChangePercentage,
            VolumeHistory = volumeHistory,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<MarketplacePerformanceMetrics> GetPerformanceMetricsAsync(TimePeriod period, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting performance metrics for period {Period}", period);

        var (startDate, endDate) = GetDateRangeForPeriod(period);

        var totalListings = await _context.MarketListings
            .CountAsync(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate, cancellationToken);

        var activeListings = await _context.MarketListings
            .CountAsync(l => l.Status == ListingStatus.Active, cancellationToken);

        var orders = await _context.MarketplaceOrders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var averageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0;
        var successRate = orders.Any() ? (decimal)completedOrders.Count / orders.Count * 100 : 0;

        // Calculate platform fees (assuming 2.5% fee)
        var platformFees = totalRevenue * 0.025m;

        // Calculate average processing time
        var processingTimes = completedOrders
            .Select(o => o.UpdatedAt - o.CreatedAt)
            .ToList();

        var averageProcessingTime = processingTimes.Any() 
            ? TimeSpan.FromTicks((long)processingTimes.Average(t => t.Ticks))
            : TimeSpan.Zero;

        return new MarketplacePerformanceMetrics
        {
            Period = period,
            TotalListings = totalListings,
            ActiveListings = activeListings,
            CompletedOrders = completedOrders.Count,
            SuccessRate = successRate,
            AverageOrderValue = averageOrderValue,
            TotalRevenue = totalRevenue,
            PlatformFees = platformFees,
            AverageOrderProcessingTime = averageProcessingTime,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<UserActivityStats> GetUserActivityStatsAsync(TimePeriod period, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user activity stats for period {Period}", period);

        var (startDate, endDate) = GetDateRangeForPeriod(period);

        // Get unique users from orders
        var orderUsers = await _context.MarketplaceOrders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .Select(o => new { o.BuyerId, o.SellerId })
            .ToListAsync(cancellationToken);

        var allUserIds = orderUsers.SelectMany(u => new[] { u.BuyerId, u.SellerId }).Distinct().ToList();
        var totalUsers = allUserIds.Count;

        // Get users from listings
        var listingUsers = await _context.MarketListings
            .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate)
            .Select(l => l.SellerId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeUsers = allUserIds.Union(listingUsers).Distinct().Count();

        // Calculate new vs returning users (simplified - would need user registration data)
        var newUsers = totalUsers; // Simplified assumption
        var returningUsers = 0;

        var totalSellers = listingUsers.Count;
        var totalBuyers = orderUsers.Select(u => u.BuyerId).Distinct().Count();

        return new UserActivityStats
        {
            Period = period,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            NewUsers = newUsers,
            ReturningUsers = returningUsers,
            UserRetentionRate = totalUsers > 0 ? (decimal)returningUsers / totalUsers * 100 : 0,
            AverageSessionDuration = 0, // Would need session tracking
            TotalSellers = totalSellers,
            TotalBuyers = totalBuyers,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<ListingPerformanceDto>> GetTopPerformingListingsAsync(TimePeriod period, PerformanceMetric metric, int limit = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting top performing listings for period {Period} by metric {Metric}", period, metric);

        var (startDate, endDate) = GetDateRangeForPeriod(period);

        var query = _context.MarketListings
            .Include(l => l.Orders)
            .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate);

        var listings = await query.ToListAsync(cancellationToken);

        var performanceData = listings.Select(l => new ListingPerformanceDto
        {
            ListingId = l.Id,
            Title = l.Title,
            AssetType = l.AssetType,
            AssetSymbol = l.AssetSymbol,
            Volume = l.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
            Revenue = l.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
            OrderCount = l.Orders.Count(o => o.Status == OrderStatus.Completed),
            ViewCount = 0, // Would need view tracking
            ConversionRate = l.Orders.Any() ? (decimal)l.Orders.Count(o => o.Status == OrderStatus.Completed) / l.Orders.Count * 100 : 0,
            RankedBy = metric,
            Rank = 0 // Will be set after ordering
        }).ToList();

        // Order by the specified metric
        var orderedData = metric switch
        {
            PerformanceMetric.Volume => performanceData.OrderByDescending(p => p.Volume),
            PerformanceMetric.Revenue => performanceData.OrderByDescending(p => p.Revenue),
            PerformanceMetric.Orders => performanceData.OrderByDescending(p => p.OrderCount),
            PerformanceMetric.Views => performanceData.OrderByDescending(p => p.ViewCount),
            PerformanceMetric.ConversionRate => performanceData.OrderByDescending(p => p.ConversionRate),
            _ => performanceData.OrderByDescending(p => p.Volume)
        };

        // Set ranks and return top performers
        var result = orderedData.Take(limit).Select((p, index) => 
        {
            p.Rank = index + 1;
            return p;
        }).ToList();

        return result;
    }

    public async Task<PriceTrendAnalysis> GetPriceTrendAnalysisAsync(AssetType assetType, string? assetSymbol = null, Guid? tokenId = null, TimePeriod period = TimePeriod.Month, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting price trend analysis for asset type {AssetType}", assetType);

        var (startDate, endDate) = GetDateRangeForPeriod(period);

        var query = _context.MarketListings
            .Where(l => l.AssetType == assetType);

        if (!string.IsNullOrEmpty(assetSymbol))
        {
            query = query.Where(l => l.AssetSymbol == assetSymbol);
        }

        if (tokenId.HasValue)
        {
            query = query.Where(l => l.TokenId == tokenId.Value);
        }

        var listings = await query
            .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        if (!listings.Any())
        {
            return new PriceTrendAnalysis
            {
                AssetType = assetType,
                AssetSymbol = assetSymbol,
                TokenId = tokenId,
                Period = period,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var prices = listings.Where(l => l.BasePrice.HasValue).Select(l => l.BasePrice!.Value).ToList();
        
        if (!prices.Any())
        {
            return new PriceTrendAnalysis
            {
                AssetType = assetType,
                AssetSymbol = assetSymbol,
                TokenId = tokenId,
                Period = period,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var currentPrice = prices.Last();
        var startPrice = prices.First();
        var highPrice = prices.Max();
        var lowPrice = prices.Min();
        var priceChange = currentPrice - startPrice;
        var priceChangePercentage = startPrice > 0 ? (priceChange / startPrice) * 100 : 0;

        var volatility = CalculateVolatility(prices);
        var trend = priceChangePercentage > 5 ? "Bullish" : priceChangePercentage < -5 ? "Bearish" : "Stable";

        var priceHistory = listings.Where(l => l.BasePrice.HasValue).Select(l => new PriceDataPoint
        {
            Timestamp = l.CreatedAt,
            Price = l.BasePrice!.Value,
            Volume = 0 // Would need order volume data
        }).ToList();

        return new PriceTrendAnalysis
        {
            AssetType = assetType,
            AssetSymbol = assetSymbol,
            TokenId = tokenId,
            Period = period,
            CurrentPrice = currentPrice,
            StartPrice = startPrice,
            HighPrice = highPrice,
            LowPrice = lowPrice,
            PriceChange = priceChange,
            PriceChangePercentage = priceChangePercentage,
            Volatility = volatility,
            Trend = trend,
            PriceHistory = priceHistory,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<MarketDepthAnalysis> GetMarketDepthAnalysisAsync(AssetType assetType, string? assetSymbol = null, Guid? tokenId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting market depth analysis for asset type {AssetType}", assetType);

        var query = _context.MarketListings
            .Where(l => l.AssetType == assetType && l.Status == ListingStatus.Active);

        if (!string.IsNullOrEmpty(assetSymbol))
        {
            query = query.Where(l => l.AssetSymbol == assetSymbol);
        }

        if (tokenId.HasValue)
        {
            query = query.Where(l => l.TokenId == tokenId.Value);
        }

        var listings = await query.ToListAsync(cancellationToken);

        // Group by price levels for order book
        var sellOrders = listings
            .Where(l => l.BasePrice.HasValue)
            .GroupBy(l => l.BasePrice!.Value)
            .Select(g => new OrderBookEntry
            {
                Price = g.Key,
                Quantity = g.Sum(l => l.RemainingQuantity),
                TotalValue = g.Key * g.Sum(l => l.RemainingQuantity),
                OrderCount = g.Count()
            })
            .OrderBy(o => o.Price)
            .ToList();

        // For buy orders, we'd need pending orders data - using empty list for now
        var buyOrders = new List<OrderBookEntry>();

        var bestAskPrice = sellOrders.Any() ? sellOrders.Min(o => o.Price) : 0;
        var bestBidPrice = 0m; // Would need buy order data
        var spread = bestAskPrice - bestBidPrice;
        var spreadPercentage = bestAskPrice > 0 ? (spread / bestAskPrice) * 100 : 0;

        return new MarketDepthAnalysis
        {
            AssetType = assetType,
            AssetSymbol = assetSymbol,
            TokenId = tokenId,
            BuyOrders = buyOrders,
            SellOrders = sellOrders,
            BestBidPrice = bestBidPrice,
            BestAskPrice = bestAskPrice,
            Spread = spread,
            SpreadPercentage = spreadPercentage,
            TotalBuyVolume = buyOrders.Sum(o => o.Quantity),
            TotalSellVolume = sellOrders.Sum(o => o.Quantity),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<RevenueAnalytics> GetRevenueAnalyticsAsync(TimePeriod period, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting revenue analytics for period {Period}", period);

        var (startDate, endDate) = GetDateRangeForPeriod(period);

        var completedOrders = await _context.MarketplaceOrders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        
        // Calculate different fee types (assuming standard rates)
        var platformFees = totalRevenue * 0.025m; // 2.5% platform fee
        var listingFees = completedOrders.Count * 10m; // $10 per listing fee
        var transactionFees = totalRevenue * 0.01m; // 1% transaction fee
        var escrowFees = totalRevenue * 0.005m; // 0.5% escrow fee

        // Calculate previous period for growth
        var previousPeriodStart = GetPreviousPeriodStart(startDate, period);
        var previousOrders = await _context.MarketplaceOrders
            .Where(o => o.CreatedAt >= previousPeriodStart && o.CreatedAt < startDate && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        var previousRevenue = previousOrders.Sum(o => o.TotalAmount);
        var revenueGrowth = totalRevenue - previousRevenue;
        var revenueGrowthPercentage = previousRevenue > 0 ? (revenueGrowth / previousRevenue) * 100 : 0;

        // Generate revenue history
        var revenueHistory = await GenerateRevenueHistoryAsync(startDate, endDate, cancellationToken);

        return new RevenueAnalytics
        {
            Period = period,
            TotalRevenue = totalRevenue,
            PlatformFees = platformFees,
            ListingFees = listingFees,
            TransactionFees = transactionFees,
            EscrowFees = escrowFees,
            RevenueGrowth = revenueGrowth,
            RevenueGrowthPercentage = revenueGrowthPercentage,
            RevenueHistory = revenueHistory,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<ConversionFunnelMetrics> GetConversionFunnelAsync(TimePeriod period, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting conversion funnel metrics for period {Period}", period);

        var (startDate, endDate) = GetDateRangeForPeriod(period);

        // Would need view tracking for accurate metrics - using simplified calculations
        var listings = await _context.MarketListings
            .Include(l => l.Orders)
            .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var orders = await _context.MarketplaceOrders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var listingViews = listings.Count * 10; // Estimated views per listing
        var ordersStarted = orders.Count;
        var ordersCompleted = orders.Count(o => o.Status == OrderStatus.Completed);

        var viewToOrderRate = listingViews > 0 ? (decimal)ordersStarted / listingViews * 100 : 0;
        var orderCompletionRate = ordersStarted > 0 ? (decimal)ordersCompleted / ordersStarted * 100 : 0;
        var overallConversionRate = listingViews > 0 ? (decimal)ordersCompleted / listingViews * 100 : 0;

        // Calculate average times
        var orderTimes = orders
            .Select(o => o.UpdatedAt - o.CreatedAt)
            .ToList();

        var averageOrderDuration = orderTimes.Any() 
            ? TimeSpan.FromTicks((long)orderTimes.Average(t => t.Ticks))
            : TimeSpan.Zero;

        return new ConversionFunnelMetrics
        {
            Period = period,
            ListingViews = listingViews,
            OrdersStarted = ordersStarted,
            OrdersCompleted = ordersCompleted,
            ViewToOrderRate = viewToOrderRate,
            OrderCompletionRate = orderCompletionRate,
            OverallConversionRate = overallConversionRate,
            AverageTimeToOrder = TimeSpan.FromHours(2), // Estimated
            AverageOrderDuration = averageOrderDuration,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<MarketplaceHealthReport> GenerateHealthReportAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating marketplace health report");

        var activeListings = await _context.MarketListings
            .CountAsync(l => l.Status == ListingStatus.Active, cancellationToken);

        var pendingOrders = await _context.MarketplaceOrders
            .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

        var activeEscrows = await _context.EscrowAccounts
            .CountAsync(e => e.Status == EscrowStatus.Funded, cancellationToken);

        var disputedEscrows = await _context.EscrowAccounts
            .CountAsync(e => e.Status == EscrowStatus.Disputed, cancellationToken);

        // Calculate health score based on various factors
        var healthScore = CalculateHealthScore(activeListings, pendingOrders, disputedEscrows);
        var isHealthy = healthScore >= 70;

        var issues = new List<HealthIssue>();
        var recommendations = new List<HealthRecommendation>();

        // Check for issues
        if (disputedEscrows > activeEscrows * 0.1m)
        {
            issues.Add(new HealthIssue
            {
                Category = "Escrow",
                Description = "High number of disputed escrows detected",
                Severity = "High",
                DetectedAt = DateTime.UtcNow
            });

            recommendations.Add(new HealthRecommendation
            {
                Category = "Escrow",
                Recommendation = "Review dispute resolution process and improve seller verification",
                Priority = "High",
                ExpectedImpact = "Reduce dispute rate by 50%"
            });
        }

        if (pendingOrders > activeListings * 0.2m)
        {
            issues.Add(new HealthIssue
            {
                Category = "Orders",
                Description = "High number of pending orders relative to active listings",
                Severity = "Medium",
                DetectedAt = DateTime.UtcNow
            });
        }

        return new MarketplaceHealthReport
        {
            IsHealthy = isHealthy,
            HealthScore = healthScore,
            ActiveListings = activeListings,
            PendingOrders = pendingOrders,
            ActiveEscrows = activeEscrows,
            DisputedEscrows = disputedEscrows,
            SystemUptime = 99.9m, // Would need monitoring data
            AverageResponseTime = 150m, // Would need performance monitoring
            ErrorRate = 1, // Would need error tracking
            Issues = issues,
            Recommendations = recommendations,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<RealTimeMarketStats> GetRealTimeStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting real-time market stats");

        var now = DateTime.UtcNow;
        var last24Hours = now.AddDays(-1);

        var activeListings = await _context.MarketListings
            .CountAsync(l => l.Status == ListingStatus.Active, cancellationToken);

        var ordersLast24h = await _context.MarketplaceOrders
            .CountAsync(o => o.CreatedAt >= last24Hours, cancellationToken);

        var volumeLast24h = await _context.MarketplaceOrders
            .Where(o => o.CreatedAt >= last24Hours && o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var pendingOrders = await _context.MarketplaceOrders
            .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

        var activeEscrows = await _context.EscrowAccounts
            .CountAsync(e => e.Status == EscrowStatus.Funded, cancellationToken);

        var averageOrderValue = ordersLast24h > 0 ? volumeLast24h / ordersLast24h : 0;

        // Get most traded asset
        var mostTradedAsset = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.CreatedAt >= last24Hours)
            .GroupBy(o => o.MarketListing!.AssetType)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key.ToString())
            .FirstOrDefaultAsync(cancellationToken) ?? "None";

        return new RealTimeMarketStats
        {
            ActiveListings = activeListings,
            OnlineUsers = 0, // Would need session tracking
            OrdersLast24h = ordersLast24h,
            VolumeLast24h = volumeLast24h,
            PendingOrders = pendingOrders,
            ActiveEscrows = activeEscrows,
            AverageOrderValue = averageOrderValue,
            MostTradedAsset = mostTradedAsset,
            SystemLoad = 0.3m, // Would need system monitoring
            LastUpdated = now
        };
    }

    // Helper Methods
    private static (DateTime startDate, DateTime endDate) GetDateRangeForPeriod(TimePeriod period)
    {
        var now = DateTime.UtcNow;
        return period switch
        {
            TimePeriod.Hour => (now.AddHours(-1), now),
            TimePeriod.Day => (now.AddDays(-1), now),
            TimePeriod.Week => (now.AddDays(-7), now),
            TimePeriod.Month => (now.AddDays(-30), now),
            TimePeriod.Quarter => (now.AddDays(-90), now),
            TimePeriod.Year => (now.AddDays(-365), now),
            TimePeriod.AllTime => (DateTime.MinValue, now),
            _ => (now.AddDays(-30), now)
        };
    }

    private static DateTime GetPreviousPeriodStart(DateTime startDate, TimePeriod period)
    {
        return period switch
        {
            TimePeriod.Hour => startDate.AddHours(-1),
            TimePeriod.Day => startDate.AddDays(-1),
            TimePeriod.Week => startDate.AddDays(-7),
            TimePeriod.Month => startDate.AddDays(-30),
            TimePeriod.Quarter => startDate.AddDays(-90),
            TimePeriod.Year => startDate.AddDays(-365),
            _ => startDate.AddDays(-30)
        };
    }

    private async Task<IEnumerable<VolumeDataPoint>> GenerateVolumeHistoryAsync(DateTime startDate, DateTime endDate, AssetType? assetType, CancellationToken cancellationToken)
    {
        var days = (endDate - startDate).Days;
        var dataPoints = new List<VolumeDataPoint>();

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var nextDate = date.AddDays(1);

            var query = _context.MarketplaceOrders
                .Include(o => o.MarketListing)
                .Where(o => o.CreatedAt >= date && o.CreatedAt < nextDate && o.Status == OrderStatus.Completed);

            if (assetType.HasValue)
            {
                query = query.Where(o => o.MarketListing!.AssetType == assetType.Value);
            }

            var dayOrders = await query.ToListAsync(cancellationToken);

            dataPoints.Add(new VolumeDataPoint
            {
                Timestamp = date,
                Volume = dayOrders.Sum(o => o.TotalAmount),
                TransactionCount = dayOrders.Count
            });
        }

        return dataPoints;
    }

    private async Task<IEnumerable<RevenueDataPoint>> GenerateRevenueHistoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var days = (endDate - startDate).Days;
        var dataPoints = new List<RevenueDataPoint>();

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var nextDate = date.AddDays(1);

            var dayOrders = await _context.MarketplaceOrders
                .Where(o => o.CreatedAt >= date && o.CreatedAt < nextDate && o.Status == OrderStatus.Completed)
                .ToListAsync(cancellationToken);

            var revenue = dayOrders.Sum(o => o.TotalAmount);
            var fees = revenue * 0.025m; // 2.5% platform fee

            dataPoints.Add(new RevenueDataPoint
            {
                Timestamp = date,
                Revenue = revenue,
                Fees = fees
            });
        }

        return dataPoints;
    }

    private static decimal CalculateVolatility(List<decimal> prices)
    {
        if (prices.Count <= 1) return 0;

        var average = prices.Average();
        var variance = prices.Sum(p => (p - average) * (p - average)) / prices.Count;
        var standardDeviation = (decimal)Math.Sqrt((double)variance);
        
        return average > 0 ? standardDeviation / average : 0;
    }

    private static decimal CalculateHealthScore(int activeListings, int pendingOrders, int disputedEscrows)
    {
        var baseScore = 100m;
        
        // Deduct points for issues
        if (activeListings == 0) baseScore -= 30;
        if (pendingOrders > activeListings * 0.2m) baseScore -= 20;
        if (disputedEscrows > 0) baseScore -= Math.Min(disputedEscrows * 5, 30);
        
        return Math.Max(0, baseScore);
    }
}
