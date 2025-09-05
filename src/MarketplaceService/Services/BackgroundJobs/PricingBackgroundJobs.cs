using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services.BackgroundJobs;

/// <summary>
/// Background jobs for automated pricing updates using Hangfire
/// Follows PaymentGatewayService pattern with direct response returns and exception-based error handling
/// </summary>
public class PricingBackgroundJobs
{
    private readonly MarketplaceDbContext _context;
    private readonly IAdvancedPricingService _advancedPricingService;
    private readonly ILogger<PricingBackgroundJobs> _logger;

    public PricingBackgroundJobs(
        MarketplaceDbContext context,
        IAdvancedPricingService advancedPricingService,
        ILogger<PricingBackgroundJobs> logger)
    {
        _context = context;
        _advancedPricingService = advancedPricingService;
        _logger = logger;
    }

    /// <summary>
    /// Update margin-based pricing for all active listings
    /// Scheduled to run every 15 minutes
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task UpdateMarginBasedPricingAsync()
    {
        _logger.LogInformation("Starting margin-based pricing update job");

        try
        {
            var marginBasedListings = await _context.MarketListings
                .Where(l => l.Status == ListingStatus.Active && 
                           (l.PricingStrategy == PricingStrategy.MarginFixed || 
                            l.PricingStrategy == PricingStrategy.MarginPercentage))
                .ToListAsync();

            _logger.LogInformation("Found {Count} margin-based listings to update", marginBasedListings.Count);

            var updateTasks = marginBasedListings.Select(async listing =>
            {
                try
                {
                    var result = await _advancedPricingService.UpdateMarginBasedPricingAsync(
                        listing.Id, forceUpdate: false, CancellationToken.None);

                    if (result.WasUpdated)
                    {
                        _logger.LogInformation("Updated margin-based pricing for listing {ListingId}: {OldPrice} -> {NewPrice}", 
                            listing.Id, result.OldPrice, result.NewPrice);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update margin-based pricing for listing {ListingId}", listing.Id);
                }
            });

            await Task.WhenAll(updateTasks);

            _logger.LogInformation("Completed margin-based pricing update job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in margin-based pricing update job");
            throw;
        }
    }

    /// <summary>
    /// Update dynamic pricing for all active listings
    /// Scheduled to run every 5 minutes
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task UpdateDynamicPricingAsync()
    {
        _logger.LogInformation("Starting dynamic pricing update job");

        try
        {
            var dynamicPricingListings = await _context.MarketListings
                .Where(l => l.Status == ListingStatus.Active && 
                           l.PricingStrategy == PricingStrategy.Dynamic)
                .ToListAsync();

            _logger.LogInformation("Found {Count} dynamic pricing listings to update", dynamicPricingListings.Count);

            var updateTasks = dynamicPricingListings.Select(async listing =>
            {
                try
                {
                    var result = await _advancedPricingService.UpdateDynamicPricingAsync(
                        listing.Id, marketConditions: null, CancellationToken.None);

                    if (result.WasUpdated)
                    {
                        _logger.LogInformation("Updated dynamic pricing for listing {ListingId}: {OldPrice} -> {NewPrice} (multiplier: {Multiplier})", 
                            listing.Id, result.OldPrice, result.NewPrice, result.PriceMultiplier);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update dynamic pricing for listing {ListingId}", listing.Id);
                }
            });

            await Task.WhenAll(updateTasks);

            _logger.LogInformation("Completed dynamic pricing update job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in dynamic pricing update job");
            throw;
        }
    }

    /// <summary>
    /// Clean up expired price history records
    /// Scheduled to run daily at 2 AM
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task CleanupPriceHistoryAsync()
    {
        _logger.LogInformation("Starting price history cleanup job");

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-90); // Keep 90 days of history

            var expiredRecords = await _context.PriceHistory
                .Where(ph => ph.CreatedAt < cutoffDate)
                .CountAsync();

            if (expiredRecords > 0)
            {
                await _context.PriceHistory
                    .Where(ph => ph.CreatedAt < cutoffDate)
                    .ExecuteDeleteAsync();

                _logger.LogInformation("Cleaned up {Count} expired price history records", expiredRecords);
            }
            else
            {
                _logger.LogInformation("No expired price history records to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in price history cleanup job");
            throw;
        }
    }

    /// <summary>
    /// Generate pricing analytics reports
    /// Scheduled to run daily at 6 AM
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task GeneratePricingAnalyticsAsync()
    {
        _logger.LogInformation("Starting pricing analytics generation job");

        try
        {
            var activeListings = await _context.MarketListings
                .Where(l => l.Status == ListingStatus.Active)
                .Select(l => l.Id)
                .ToListAsync();

            _logger.LogInformation("Generating analytics for {Count} active listings", activeListings.Count);

            var analyticsResults = new List<PricingAnalyticsResult>();

            foreach (var listingId in activeListings)
            {
                try
                {
                    var analytics = await _advancedPricingService.GetPricingAnalyticsAsync(
                        listingId, TimeSpan.FromDays(7), CancellationToken.None);

                    analyticsResults.Add(analytics);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate analytics for listing {ListingId}", listingId);
                }
            }

            // Store analytics results (could be saved to database, sent to analytics service, etc.)
            _logger.LogInformation("Generated pricing analytics for {Count} listings", analyticsResults.Count);

            // Log summary statistics
            if (analyticsResults.Any())
            {
                var avgPriceChanges = analyticsResults.Average(a => a.Statistics.PriceChanges);
                var totalVolume = analyticsResults.Sum(a => a.Statistics.TotalVolume);
                var totalRevenue = analyticsResults.Sum(a => a.Statistics.TotalRevenue);

                _logger.LogInformation("Pricing analytics summary - Avg price changes: {AvgChanges:F1}, Total volume: {Volume:F2}, Total revenue: {Revenue:C}", 
                    avgPriceChanges, totalVolume, totalRevenue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in pricing analytics generation job");
            throw;
        }
    }

    /// <summary>
    /// Monitor and alert on pricing anomalies
    /// Scheduled to run every 30 minutes
    /// </summary>
    [AutomaticRetry(Attempts = 2)]
    public async Task MonitorPricingAnomaliesAsync()
    {
        _logger.LogInformation("Starting pricing anomaly monitoring job");

        try
        {
            var recentPriceChanges = await _context.PriceHistory
                .Where(ph => ph.CreatedAt >= DateTime.UtcNow.AddHours(-1))
                .Include(ph => ph.MarketListing)
                .ToListAsync();

            var anomalies = new List<string>();

            foreach (var priceChange in recentPriceChanges)
            {
                // Check for large price changes (>20%)
                var previousPrice = await _context.PriceHistory
                    .Where(ph => ph.ListingId == priceChange.ListingId && 
                               ph.CreatedAt < priceChange.CreatedAt)
                    .OrderByDescending(ph => ph.CreatedAt)
                    .Select(ph => ph.PricePerToken)
                    .FirstOrDefaultAsync();

                if (previousPrice > 0)
                {
                    var changePercentage = Math.Abs(priceChange.PricePerToken - previousPrice) / previousPrice;
                    
                    if (changePercentage > 0.2m) // 20% change threshold
                    {
                        anomalies.Add($"Large price change detected for listing {priceChange.ListingId}: " +
                                    $"{previousPrice:C} -> {priceChange.PricePerToken:C} ({changePercentage:P1} change)");
                    }
                }
            }

            if (anomalies.Any())
            {
                _logger.LogWarning("Pricing anomalies detected: {Anomalies}", string.Join("; ", anomalies));
                
                // In production, this could trigger alerts, notifications, or automatic safeguards
                // For now, we just log the anomalies
            }
            else
            {
                _logger.LogInformation("No pricing anomalies detected");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in pricing anomaly monitoring job");
            throw;
        }
    }
}

/// <summary>
/// Extension methods for registering pricing background jobs
/// </summary>
public static class PricingBackgroundJobsExtensions
{
    /// <summary>
    /// Schedule all pricing-related background jobs
    /// </summary>
    public static void SchedulePricingJobs(this IServiceProvider serviceProvider)
    {
        // Schedule margin-based pricing updates every 15 minutes
        RecurringJob.AddOrUpdate<PricingBackgroundJobs>(
            "update-margin-pricing",
            job => job.UpdateMarginBasedPricingAsync(),
            "*/15 * * * *"); // Every 15 minutes

        // Schedule dynamic pricing updates every 5 minutes
        RecurringJob.AddOrUpdate<PricingBackgroundJobs>(
            "update-dynamic-pricing",
            job => job.UpdateDynamicPricingAsync(),
            "*/5 * * * *"); // Every 5 minutes

        // Schedule price history cleanup daily at 2 AM
        RecurringJob.AddOrUpdate<PricingBackgroundJobs>(
            "cleanup-price-history",
            job => job.CleanupPriceHistoryAsync(),
            "0 2 * * *"); // Daily at 2 AM

        // Schedule pricing analytics generation daily at 6 AM
        RecurringJob.AddOrUpdate<PricingBackgroundJobs>(
            "generate-pricing-analytics",
            job => job.GeneratePricingAnalyticsAsync(),
            "0 6 * * *"); // Daily at 6 AM

        // Schedule pricing anomaly monitoring every 30 minutes
        RecurringJob.AddOrUpdate<PricingBackgroundJobs>(
            "monitor-pricing-anomalies",
            job => job.MonitorPricingAnomaliesAsync(),
            "*/30 * * * *"); // Every 30 minutes
    }
}
