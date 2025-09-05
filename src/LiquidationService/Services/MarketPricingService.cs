using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using LiquidationService.Data;
using LiquidationService.Data.Entities;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using Mapster;

namespace LiquidationService.Services;

/// <summary>
/// Service for market pricing and price discovery
/// Follows the PaymentGatewayService pattern - returns response models directly and throws exceptions for errors
/// </summary>
public class MarketPricingService : IMarketPricingService
{
    private readonly LiquidationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MarketPricingService> _logger;

    public MarketPricingService(
        LiquidationDbContext context,
        IDistributedCache cache,
        ILogger<MarketPricingService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get current market price for an asset pair
    /// </summary>
    public async Task<MarketPriceSnapshotResponse> GetCurrentPriceAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting current price for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);

        try
        {
            // Check cache first
            var cacheKey = $"price:{assetSymbol}:{outputSymbol}";
            var cachedPrice = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedPrice))
            {
                _logger.LogInformation("Price found in cache for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
                // In a real implementation, you'd deserialize the cached price
                // For now, we'll fall through to generate a new price
            }

            // Get latest price from database or generate new one
            var latestPrice = await _context.MarketPriceSnapshots
                .Where(mps => mps.AssetSymbol == assetSymbol && mps.OutputSymbol == outputSymbol)
                .OrderByDescending(mps => mps.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            // Check if price is still valid (within 5 minutes)
            if (latestPrice != null && latestPrice.ExpiresAt > DateTime.UtcNow)
            {
                _logger.LogInformation("Valid price found in database for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
                return latestPrice.Adapt<MarketPriceSnapshotResponse>();
            }

            // Generate new price snapshot
            var newPriceSnapshot = await GeneratePriceSnapshotAsync(assetSymbol, outputSymbol, cancellationToken);

            _logger.LogInformation("Generated new price for {AssetSymbol} -> {OutputSymbol}: {Price}",
                assetSymbol, outputSymbol, newPriceSnapshot.Price);

            return newPriceSnapshot.Adapt<MarketPriceSnapshotResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current price for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get price with slippage estimation for a specific amount
    /// </summary>
    public async Task<MarketPriceSnapshotResponse> GetPriceWithSlippageAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting price with slippage for {AssetSymbol} -> {OutputSymbol}, amount: {Amount}",
            assetSymbol, outputSymbol, amount);

        try
        {
            // Get base price
            var basePrice = await GetCurrentPriceAsync(assetSymbol, outputSymbol, cancellationToken);

            // Calculate slippage based on amount
            var slippagePercentage = CalculateSlippagePercentage(amount);
            var adjustedPrice = basePrice.Price * (1 - slippagePercentage / 100);

            // Create new snapshot with slippage
            var priceWithSlippage = new MarketPriceSnapshot
            {
                Id = Guid.NewGuid(),
                AssetSymbol = assetSymbol,
                OutputSymbol = outputSymbol,
                Price = adjustedPrice,
                BidPrice = adjustedPrice * 0.999m, // Slightly lower bid
                AskPrice = adjustedPrice * 1.001m, // Slightly higher ask
                Volume24h = basePrice.Volume24h,
                Change24hPercent = basePrice.Change24hPercent,
                PriceSource = "InternalPricingEngine",
                Exchange = "Aggregated",
                ConfidenceLevel = Math.Max((basePrice.ConfidenceLevel ?? 80) - 10, 70), // Lower confidence with slippage
                IsSuitableForLiquidation = slippagePercentage <= 5.0m, // Not suitable if slippage > 5%
                EstimatedSlippage = slippagePercentage,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(2) // Shorter expiry for slippage prices
            };

            _context.MarketPriceSnapshots.Add(priceWithSlippage);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated price with slippage for {AssetSymbol} -> {OutputSymbol}: {Price} (slippage: {Slippage}%)",
                assetSymbol, outputSymbol, adjustedPrice, slippagePercentage);

            return priceWithSlippage.Adapt<MarketPriceSnapshotResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price with slippage for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get historical prices for an asset pair
    /// </summary>
    public async Task<IEnumerable<MarketPriceSnapshotResponse>> GetHistoricalPricesAsync(
        string assetSymbol, 
        string outputSymbol, 
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting historical prices for {AssetSymbol} -> {OutputSymbol} from {FromDate} to {ToDate}",
            assetSymbol, outputSymbol, fromDate, toDate);

        try
        {
            var historicalPrices = await _context.MarketPriceSnapshots
                .Where(mps => mps.AssetSymbol == assetSymbol && 
                             mps.OutputSymbol == outputSymbol &&
                             mps.CreatedAt >= fromDate && 
                             mps.CreatedAt <= toDate)
                .OrderBy(mps => mps.CreatedAt)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} historical price records for {AssetSymbol} -> {OutputSymbol}",
                historicalPrices.Count, assetSymbol, outputSymbol);

            return historicalPrices.Adapt<List<MarketPriceSnapshotResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical prices for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Validate if a price is suitable for liquidation
    /// </summary>
    public async Task<bool> ValidatePriceForLiquidationAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal amount, 
        decimal maxSlippagePercent = 5.0m, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating price for liquidation - {AssetSymbol} -> {OutputSymbol}, amount: {Amount}, max slippage: {MaxSlippage}%",
            assetSymbol, outputSymbol, amount, maxSlippagePercent);

        try
        {
            var priceWithSlippage = await GetPriceWithSlippageAsync(assetSymbol, outputSymbol, amount, cancellationToken);

            var isValid = priceWithSlippage.IsSuitableForLiquidation && 
                         priceWithSlippage.EstimatedSlippage <= maxSlippagePercent &&
                         priceWithSlippage.ConfidenceLevel >= 70;

            _logger.LogInformation("Price validation for {AssetSymbol} -> {OutputSymbol}: {IsValid} (slippage: {Slippage}%, confidence: {Confidence}%)",
                assetSymbol, outputSymbol, isValid, priceWithSlippage.EstimatedSlippage, priceWithSlippage.ConfidenceLevel);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating price for liquidation {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get best available price from multiple sources
    /// </summary>
    public async Task<MarketPriceSnapshotResponse> GetBestPriceAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting best price for {AssetSymbol} -> {OutputSymbol}, amount: {Amount}",
            assetSymbol, outputSymbol, amount);

        try
        {
            // In a real implementation, this would query multiple exchanges/sources
            // For simulation, we'll generate prices from multiple "sources" and pick the best

            var sources = new[] { "Binance", "Coinbase", "Kraken", "Uniswap" };
            var prices = new List<MarketPriceSnapshot>();

            foreach (var source in sources)
            {
                var price = await GeneratePriceSnapshotAsync(assetSymbol, outputSymbol, cancellationToken, source);
                prices.Add(price);
            }

            // Select best price (highest for selling, considering confidence and slippage)
            var bestPrice = prices
                .Where(p => p.ConfidenceLevel >= 70)
                .OrderByDescending(p => p.Price * (p.ConfidenceLevel / 100m))
                .FirstOrDefault();

            if (bestPrice == null)
            {
                // Fallback to any available price
                bestPrice = prices.OrderByDescending(p => p.Price).First();
            }

            // Apply slippage for the amount
            var slippagePercentage = CalculateSlippagePercentage(amount);
            bestPrice.Price *= (1 - slippagePercentage / 100);
            bestPrice.EstimatedSlippage = slippagePercentage;

            _context.MarketPriceSnapshots.Add(bestPrice);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Best price found for {AssetSymbol} -> {OutputSymbol}: {Price} from {Exchange}",
                assetSymbol, outputSymbol, bestPrice.Price, bestPrice.Exchange);

            return bestPrice.Adapt<MarketPriceSnapshotResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best price for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Update price data from external sources
    /// </summary>
    public async Task<MarketPriceSnapshotResponse> RefreshPriceDataAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refreshing price data for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);

        try
        {
            // Force refresh by generating new price snapshot
            var refreshedPrice = await GeneratePriceSnapshotAsync(assetSymbol, outputSymbol, cancellationToken);

            // Update cache
            var cacheKey = $"price:{assetSymbol}:{outputSymbol}";
            await _cache.SetStringAsync(cacheKey, refreshedPrice.Id.ToString(), 
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                }, cancellationToken);

            _logger.LogInformation("Price data refreshed for {AssetSymbol} -> {OutputSymbol}: {Price}",
                assetSymbol, outputSymbol, refreshedPrice.Price);

            return refreshedPrice.Adapt<MarketPriceSnapshotResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing price data for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get price confidence level for an asset pair
    /// </summary>
    public async Task<int> GetPriceConfidenceLevelAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting price confidence level for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);

        try
        {
            var currentPrice = await GetCurrentPriceAsync(assetSymbol, outputSymbol, cancellationToken);
            
            _logger.LogInformation("Price confidence level for {AssetSymbol} -> {OutputSymbol}: {ConfidenceLevel}%",
                assetSymbol, outputSymbol, currentPrice.ConfidenceLevel);

            return currentPrice.ConfidenceLevel ?? 80; // Default confidence
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price confidence level for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Calculate estimated output amount for liquidation
    /// </summary>
    public async Task<decimal> CalculateEstimatedOutputAsync(
        string assetSymbol, 
        decimal assetAmount, 
        string outputSymbol, 
        bool includeSlippage = true, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating estimated output for {AssetAmount} {AssetSymbol} -> {OutputSymbol}, include slippage: {IncludeSlippage}",
            assetAmount, assetSymbol, outputSymbol, includeSlippage);

        try
        {
            MarketPriceSnapshotResponse price;

            if (includeSlippage)
            {
                price = await GetPriceWithSlippageAsync(assetSymbol, outputSymbol, assetAmount, cancellationToken);
            }
            else
            {
                price = await GetCurrentPriceAsync(assetSymbol, outputSymbol, cancellationToken);
            }

            var estimatedOutput = assetAmount * price.Price;

            _logger.LogInformation("Estimated output for {AssetAmount} {AssetSymbol}: {EstimatedOutput} {OutputSymbol}",
                assetAmount, assetSymbol, estimatedOutput, outputSymbol);

            return estimatedOutput;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating estimated output for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get supported trading pairs
    /// </summary>
    public async Task<IEnumerable<string>> GetSupportedTradingPairsAsync(
        string? assetSymbol = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting supported trading pairs for asset: {AssetSymbol}", assetSymbol ?? "ALL");

        try
        {
            var query = _context.MarketPriceSnapshots.AsQueryable();

            if (!string.IsNullOrEmpty(assetSymbol))
            {
                query = query.Where(mps => mps.AssetSymbol == assetSymbol);
            }

            var tradingPairs = await query
                .Select(mps => $"{mps.AssetSymbol}/{mps.OutputSymbol}")
                .Distinct()
                .ToListAsync(cancellationToken);

            // Add some default trading pairs if none found
            if (!tradingPairs.Any())
            {
                tradingPairs = new List<string>
                {
                    "BTC/USD", "BTC/USDT", "ETH/USD", "ETH/USDT", "ETH/BTC",
                    "ADA/USD", "DOT/USD", "SOL/USD", "MATIC/USD", "LINK/USD"
                };

                if (!string.IsNullOrEmpty(assetSymbol))
                {
                    tradingPairs = tradingPairs.Where(tp => tp.StartsWith(assetSymbol + "/")).ToList();
                }
            }

            _logger.LogInformation("Found {Count} supported trading pairs", tradingPairs.Count);

            return tradingPairs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported trading pairs");
            throw;
        }
    }

    /// <summary>
    /// Get market depth for an asset pair
    /// </summary>
    public async Task<object> GetMarketDepthAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting market depth for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);

        try
        {
            var currentPrice = await GetCurrentPriceAsync(assetSymbol, outputSymbol, cancellationToken);
            var random = new Random();

            // Simulate market depth data
            var bids = new List<object>();
            var asks = new List<object>();

            // Generate bid orders (below current price)
            for (int i = 0; i < 10; i++)
            {
                var priceLevel = currentPrice.Price * (1 - (i + 1) * 0.001m);
                var volume = random.Next(100, 10000) / 100m;
                bids.Add(new { Price = priceLevel, Volume = volume });
            }

            // Generate ask orders (above current price)
            for (int i = 0; i < 10; i++)
            {
                var priceLevel = currentPrice.Price * (1 + (i + 1) * 0.001m);
                var volume = random.Next(100, 10000) / 100m;
                asks.Add(new { Price = priceLevel, Volume = volume });
            }

            var marketDepth = new
            {
                AssetSymbol = assetSymbol,
                OutputSymbol = outputSymbol,
                CurrentPrice = currentPrice.Price,
                Bids = bids,
                Asks = asks,
                BidVolume = bids.Sum(b => ((dynamic)b).Volume),
                AskVolume = asks.Sum(a => ((dynamic)a).Volume),
                Spread = currentPrice.AskPrice - currentPrice.BidPrice,
                SpreadPercentage = ((currentPrice.AskPrice - currentPrice.BidPrice) / currentPrice.Price) * 100,
                Timestamp = DateTime.UtcNow
            };

            return marketDepth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market depth for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get price alerts and notifications
    /// </summary>
    public async Task<object> SetupPriceAlertAsync(
        string assetSymbol, 
        string outputSymbol, 
        decimal priceThreshold, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting up price alert for {AssetSymbol} -> {OutputSymbol} at threshold {PriceThreshold}",
            assetSymbol, outputSymbol, priceThreshold);

        try
        {
            var currentPrice = await GetCurrentPriceAsync(assetSymbol, outputSymbol, cancellationToken);

            var priceAlert = new
            {
                Id = Guid.NewGuid(),
                AssetSymbol = assetSymbol,
                OutputSymbol = outputSymbol,
                PriceThreshold = priceThreshold,
                CurrentPrice = currentPrice.Price,
                AlertType = currentPrice.Price > priceThreshold ? "PriceAbove" : "PriceBelow",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30) // 30-day expiry
            };

            _logger.LogInformation("Price alert created for {AssetSymbol} -> {OutputSymbol}: {AlertId}",
                assetSymbol, outputSymbol, priceAlert.Id);

            return priceAlert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up price alert for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get pricing statistics and analytics
    /// </summary>
    public async Task<object> GetPricingStatisticsAsync(
        string assetSymbol, 
        string outputSymbol, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting pricing statistics for {AssetSymbol} -> {OutputSymbol} from {FromDate} to {ToDate}",
            assetSymbol, outputSymbol, fromDate, toDate);

        try
        {
            var query = _context.MarketPriceSnapshots
                .Where(mps => mps.AssetSymbol == assetSymbol && mps.OutputSymbol == outputSymbol);

            if (fromDate.HasValue)
                query = query.Where(mps => mps.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(mps => mps.CreatedAt <= toDate.Value);

            var prices = await query.OrderBy(mps => mps.CreatedAt).ToListAsync(cancellationToken);

            if (!prices.Any())
            {
                return new
                {
                    AssetSymbol = assetSymbol,
                    OutputSymbol = outputSymbol,
                    Message = "No price data available for the specified period",
                    GeneratedAt = DateTime.UtcNow
                };
            }

            var priceValues = prices.Select(p => p.Price).ToList();
            var currentPrice = priceValues.Last();
            var openPrice = priceValues.First();
            var highPrice = priceValues.Max();
            var lowPrice = priceValues.Min();
            var avgPrice = priceValues.Average();
            var priceChange = currentPrice - openPrice;
            var priceChangePercent = openPrice != 0 ? (priceChange / openPrice) * 100 : 0;

            var volatility = CalculateVolatility(priceValues);
            var avgVolume = prices.Where(p => p.Volume24h.HasValue).Average(p => p.Volume24h.Value);
            var avgConfidence = prices.Where(p => p.ConfidenceLevel.HasValue).Average(p => p.ConfidenceLevel.Value);

            var statistics = new
            {
                AssetSymbol = assetSymbol,
                OutputSymbol = outputSymbol,
                Period = new
                {
                    FromDate = fromDate ?? prices.First().CreatedAt,
                    ToDate = toDate ?? prices.Last().CreatedAt,
                    DataPoints = prices.Count
                },
                PriceStatistics = new
                {
                    CurrentPrice = currentPrice,
                    OpenPrice = openPrice,
                    HighPrice = highPrice,
                    LowPrice = lowPrice,
                    AveragePrice = avgPrice,
                    PriceChange = priceChange,
                    PriceChangePercent = priceChangePercent,
                    Volatility = volatility
                },
                VolumeStatistics = new
                {
                    AverageVolume24h = avgVolume,
                    TotalDataPoints = prices.Count(p => p.Volume24h.HasValue)
                },
                QualityMetrics = new
                {
                    AverageConfidenceLevel = avgConfidence,
                    SuitableForLiquidation = prices.Count(p => p.IsSuitableForLiquidation),
                    TotalSnapshots = prices.Count
                },
                GeneratedAt = DateTime.UtcNow
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing statistics for {AssetSymbol} -> {OutputSymbol}", assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Generate a new price snapshot for an asset pair
    /// </summary>
    private async Task<MarketPriceSnapshot> GeneratePriceSnapshotAsync(
        string assetSymbol, 
        string outputSymbol, 
        CancellationToken cancellationToken,
        string? exchange = null)
    {
        // Simulate price generation based on asset pair
        var random = new Random();
        var basePrice = GetBasePriceForPair(assetSymbol, outputSymbol);
        
        // Add some randomness (±5%)
        var priceVariation = (random.NextDouble() - 0.5) * 0.1; // ±5%
        var currentPrice = basePrice * (1 + (decimal)priceVariation);

        var priceSnapshot = new MarketPriceSnapshot
        {
            Id = Guid.NewGuid(),
            AssetSymbol = assetSymbol,
            OutputSymbol = outputSymbol,
            Price = currentPrice,
            BidPrice = currentPrice * 0.999m,
            AskPrice = currentPrice * 1.001m,
            Volume24h = random.Next(1000000, 100000000) / 100m,
            Change24hPercent = (decimal)(random.NextDouble() - 0.5) * 20, // ±10%
            PriceSource = "SimulatedPriceProvider",
            Exchange = exchange ?? "Aggregated",
            ConfidenceLevel = random.Next(70, 101),
            IsSuitableForLiquidation = true,
            EstimatedSlippage = 0, // Base price has no slippage
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _context.MarketPriceSnapshots.Add(priceSnapshot);
        await _context.SaveChangesAsync(cancellationToken);

        return priceSnapshot;
    }

    /// <summary>
    /// Get base price for a trading pair (simulated)
    /// </summary>
    private static decimal GetBasePriceForPair(string assetSymbol, string outputSymbol)
    {
        // Simulated base prices for common pairs
        var basePrices = new Dictionary<string, decimal>
        {
            ["BTC/USD"] = 45000m,
            ["BTC/USDT"] = 45000m,
            ["ETH/USD"] = 3000m,
            ["ETH/USDT"] = 3000m,
            ["ETH/BTC"] = 0.067m,
            ["ADA/USD"] = 0.5m,
            ["DOT/USD"] = 7m,
            ["SOL/USD"] = 100m,
            ["MATIC/USD"] = 0.8m,
            ["LINK/USD"] = 15m
        };

        var pairKey = $"{assetSymbol}/{outputSymbol}";
        
        if (basePrices.TryGetValue(pairKey, out var price))
        {
            return price;
        }

        // Default price calculation for unknown pairs
        return outputSymbol.ToUpper() switch
        {
            "USD" or "USDT" or "USDC" => 100m, // Default $100 for stablecoins
            "BTC" => 0.002m, // Default 0.002 BTC
            "ETH" => 0.03m, // Default 0.03 ETH
            _ => 1m // Default 1:1 ratio
        };
    }

    /// <summary>
    /// Calculate slippage percentage based on transaction amount
    /// </summary>
    private static decimal CalculateSlippagePercentage(decimal amount)
    {
        // Slippage increases with transaction size
        return amount switch
        {
            <= 1000 => 0.1m,
            <= 10000 => 0.3m,
            <= 50000 => 0.8m,
            <= 100000 => 1.5m,
            <= 500000 => 3.0m,
            <= 1000000 => 5.0m,
            _ => 8.0m // Large transactions have higher slippage
        };
    }

    /// <summary>
    /// Calculate price volatility
    /// </summary>
    private static decimal CalculateVolatility(List<decimal> prices)
    {
        if (prices.Count < 2) return 0;

        var mean = prices.Average();
        var variance = prices.Sum(p => (p - mean) * (p - mean)) / prices.Count;
        var standardDeviation = (decimal)Math.Sqrt((double)variance);
        
        return mean != 0 ? (standardDeviation / mean) * 100 : 0; // Return as percentage
    }
}
