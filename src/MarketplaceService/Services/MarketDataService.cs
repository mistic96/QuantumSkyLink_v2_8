using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services;

/// <summary>
/// Market data service for external price feeds and market conditions
/// Follows PaymentGatewayService pattern with direct response returns and exception-based error handling
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly MarketplaceDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<MarketDataService> _logger;
    private readonly HttpClient _httpClient;

    public MarketDataService(
        MarketplaceDbContext context,
        IDistributedCache cache,
        ILogger<MarketDataService> logger,
        HttpClient httpClient)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Get current market price for an asset with caching and multiple data sources
    /// </summary>
    public async Task<MarketPriceResult> GetMarketPriceAsync(
        AssetType assetType, 
        string? assetSymbol = null, 
        Guid? tokenId = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting market price for asset type {AssetType}, symbol {AssetSymbol}, token {TokenId}", 
            assetType, assetSymbol, tokenId);

        try
        {
            // Check cache first
            var cacheKey = $"market_price_{assetType}_{assetSymbol}_{tokenId}";
            var cachedPrice = await GetCachedMarketPriceAsync(cacheKey, cancellationToken);
            if (cachedPrice != null)
            {
                _logger.LogInformation("Retrieved cached market price for {AssetType}", assetType);
                return cachedPrice;
            }

            MarketPriceResult result = assetType switch
            {
                AssetType.PlatformToken => await GetPlatformTokenPriceAsync(tokenId, cancellationToken),
                AssetType.Bitcoin => await GetExternalCryptoPriceAsync("BTC", cancellationToken),
                AssetType.Ethereum => await GetExternalCryptoPriceAsync("ETH", cancellationToken),
                AssetType.Solana => await GetExternalCryptoPriceAsync("SOL", cancellationToken),
                AssetType.OtherCrypto => await GetExternalCryptoPriceAsync(assetSymbol, cancellationToken),
                _ => new MarketPriceResult
                {
                    AssetType = assetType,
                    AssetSymbol = assetSymbol,
                    TokenId = tokenId,
                    IsValid = false,
                    ErrorMessage = $"Unsupported asset type: {assetType}",
                    Timestamp = DateTime.UtcNow
                }
            };

            // Cache successful results
            if (result.IsValid)
            {
                await CacheMarketPriceAsync(cacheKey, result, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market price for asset type {AssetType}", assetType);
            return new MarketPriceResult
            {
                AssetType = assetType,
                AssetSymbol = assetSymbol,
                TokenId = tokenId,
                IsValid = false,
                ErrorMessage = $"Error getting market price: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get comprehensive market conditions for dynamic pricing
    /// </summary>
    public async Task<MarketConditions> GetMarketConditionsAsync(
        AssetType assetType, 
        string? assetSymbol = null, 
        Guid? tokenId = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting market conditions for asset type {AssetType}, symbol {AssetSymbol}, token {TokenId}", 
            assetType, assetSymbol, tokenId);

        try
        {
            // Check cache first
            var cacheKey = $"market_conditions_{assetType}_{assetSymbol}_{tokenId}";
            var cachedConditions = await GetCachedMarketConditionsAsync(cacheKey, cancellationToken);
            if (cachedConditions != null)
            {
                _logger.LogInformation("Retrieved cached market conditions for {AssetType}", assetType);
                return cachedConditions;
            }

            var conditions = assetType switch
            {
                AssetType.PlatformToken => await GetPlatformTokenMarketConditionsAsync(tokenId, cancellationToken),
                AssetType.Bitcoin or AssetType.Ethereum or AssetType.Solana or AssetType.OtherCrypto => 
                    await GetExternalCryptoMarketConditionsAsync(assetSymbol ?? GetSymbolForAssetType(assetType), cancellationToken),
                _ => new MarketConditions
                {
                    Timestamp = DateTime.UtcNow,
                    TradingVolume24h = 0,
                    PriceChange24h = 0,
                    ActiveOrders = 0,
                    AverageOrderSize = 0,
                    MarketCap = 0,
                    Volatility = 0
                }
            };

            // Cache the results
            await CacheMarketConditionsAsync(cacheKey, conditions, cancellationToken);

            return conditions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market conditions for asset type {AssetType}", assetType);
            return new MarketConditions
            {
                Timestamp = DateTime.UtcNow,
                TradingVolume24h = 0,
                PriceChange24h = 0,
                ActiveOrders = 0,
                AverageOrderSize = 0,
                MarketCap = 0,
                Volatility = 0
            };
        }
    }

    // Private helper methods

    private async Task<MarketPriceResult> GetPlatformTokenPriceAsync(Guid? tokenId, CancellationToken cancellationToken)
    {
        if (!tokenId.HasValue)
        {
            return new MarketPriceResult
            {
                AssetType = AssetType.PlatformToken,
                TokenId = tokenId,
                IsValid = false,
                ErrorMessage = "Token ID is required for platform tokens",
                Timestamp = DateTime.UtcNow
            };
        }

        // Get average price from active listings for this token
        var avgPrice = await _context.MarketListings
            .Where(l => l.TokenId == tokenId.Value && l.Status == ListingStatus.Active && l.BasePrice.HasValue)
            .AverageAsync(l => (decimal?)l.BasePrice, cancellationToken);

        if (!avgPrice.HasValue)
        {
            // If no active listings, try to get from recent completed orders
            var recentOrderPrice = await _context.MarketplaceOrders
                .Include(o => o.MarketListing)
                .Where(o => o.MarketListing.TokenId == tokenId.Value && 
                           o.Status == OrderStatus.Completed && 
                           o.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => o.PricePerToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (recentOrderPrice > 0)
            {
                avgPrice = recentOrderPrice;
            }
        }

        return new MarketPriceResult
        {
            AssetType = AssetType.PlatformToken,
            TokenId = tokenId,
            Price = avgPrice ?? 0,
            Currency = "USD",
            Source = avgPrice.HasValue ? "Platform Listings Average" : "No Price Data",
            Timestamp = DateTime.UtcNow,
            IsValid = avgPrice.HasValue && avgPrice > 0
        };
    }

    private async Task<MarketPriceResult> GetExternalCryptoPriceAsync(string? symbol, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            return new MarketPriceResult
            {
                AssetType = AssetType.OtherCrypto,
                AssetSymbol = symbol,
                IsValid = false,
                ErrorMessage = "Asset symbol is required for external crypto assets",
                Timestamp = DateTime.UtcNow
            };
        }

        try
        {
            // Mock implementation - in production, this would call external APIs like CoinGecko, CoinMarketCap, etc.
            var mockPrice = await GetMockCryptoPriceAsync(symbol.ToUpper(), cancellationToken);

            return new MarketPriceResult
            {
                AssetType = AssetType.OtherCrypto,
                AssetSymbol = symbol,
                Price = mockPrice,
                Currency = "USD",
                Source = "Mock External API",
                Timestamp = DateTime.UtcNow,
                IsValid = mockPrice > 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external crypto price for {Symbol}", symbol);
            return new MarketPriceResult
            {
                AssetType = AssetType.OtherCrypto,
                AssetSymbol = symbol,
                IsValid = false,
                ErrorMessage = $"Error fetching price: {ex.Message}",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private async Task<MarketConditions> GetPlatformTokenMarketConditionsAsync(Guid? tokenId, CancellationToken cancellationToken)
    {
        if (!tokenId.HasValue)
        {
            return new MarketConditions { Timestamp = DateTime.UtcNow };
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-1);

        // Get 24h trading volume from completed orders
        var volume24h = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.MarketListing.TokenId == tokenId.Value && 
                       o.Status == OrderStatus.Completed && 
                       o.CreatedAt >= cutoffDate)
            .SumAsync(o => o.Quantity * o.PricePerToken, cancellationToken);

        // Get active orders count
        var activeOrders = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .CountAsync(o => o.MarketListing.TokenId == tokenId.Value && 
                           (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing), 
                       cancellationToken);

        // Get average order size
        var avgOrderSize = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.MarketListing.TokenId == tokenId.Value && 
                       o.Status == OrderStatus.Completed && 
                       o.CreatedAt >= cutoffDate)
            .AverageAsync(o => (decimal?)o.Quantity, cancellationToken) ?? 0;

        // Calculate price change (simplified)
        var priceChange24h = await CalculatePriceChange24hAsync(tokenId.Value, cancellationToken);

        // Calculate volatility (simplified)
        var volatility = await CalculateVolatilityAsync(tokenId.Value, cancellationToken);

        return new MarketConditions
        {
            TradingVolume24h = volume24h,
            PriceChange24h = priceChange24h,
            ActiveOrders = activeOrders,
            AverageOrderSize = avgOrderSize,
            MarketCap = 0, // Would need token supply data
            Volatility = volatility,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<MarketConditions> GetExternalCryptoMarketConditionsAsync(string symbol, CancellationToken cancellationToken)
    {
        // Mock implementation - in production, this would call external APIs
        await Task.Delay(50, cancellationToken); // Simulate API call

        var random = new Random();
        return new MarketConditions
        {
            TradingVolume24h = random.Next(1000000, 100000000),
            PriceChange24h = (decimal)(random.NextDouble() * 0.2 - 0.1), // -10% to +10%
            ActiveOrders = random.Next(100, 10000),
            AverageOrderSize = random.Next(100, 10000),
            MarketCap = random.Next(1000000, 1000000000),
            Volatility = (decimal)(random.NextDouble() * 0.1), // 0% to 10%
            Timestamp = DateTime.UtcNow
        };
    }

    private static async Task<decimal> GetMockCryptoPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        // Mock implementation with realistic prices
        await Task.Delay(100, cancellationToken); // Simulate API call

        var basePrices = new Dictionary<string, decimal>
        {
            ["BTC"] = 45000m,
            ["ETH"] = 3000m,
            ["SOL"] = 100m,
            ["USDT"] = 1m,
            ["USDC"] = 1m,
            ["BNB"] = 300m,
            ["ADA"] = 0.5m,
            ["DOT"] = 7m,
            ["AVAX"] = 35m,
            ["MATIC"] = 0.8m
        };

        var basePrice = basePrices.GetValueOrDefault(symbol, 100m);
        
        // Add some random variation (±5%)
        var random = new Random();
        var variation = (decimal)(random.NextDouble() * 0.1 - 0.05); // -5% to +5%
        return basePrice * (1 + variation);
    }

    private async Task<decimal> CalculatePriceChange24hAsync(Guid tokenId, CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-1);

        var currentPrice = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.MarketListing.TokenId == tokenId && o.Status == OrderStatus.Completed)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.PricePerToken)
            .FirstOrDefaultAsync(cancellationToken);

        var price24hAgo = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.MarketListing.TokenId == tokenId && 
                       o.Status == OrderStatus.Completed && 
                       o.CreatedAt <= cutoffDate)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.PricePerToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentPrice > 0 && price24hAgo > 0)
        {
            return (currentPrice - price24hAgo) / price24hAgo;
        }

        return 0;
    }

    private async Task<decimal> CalculateVolatilityAsync(Guid tokenId, CancellationToken cancellationToken)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7);

        var prices = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.MarketListing.TokenId == tokenId && 
                       o.Status == OrderStatus.Completed && 
                       o.CreatedAt >= cutoffDate)
            .OrderBy(o => o.CreatedAt)
            .Select(o => o.PricePerToken)
            .ToListAsync(cancellationToken);

        if (prices.Count < 2)
            return 0;

        var avgPrice = prices.Average();
        var variance = prices.Sum(p => (p - avgPrice) * (p - avgPrice)) / prices.Count;
        return (decimal)Math.Sqrt((double)variance) / avgPrice;
    }

    private static string GetSymbolForAssetType(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.Bitcoin => "BTC",
            AssetType.Ethereum => "ETH",
            AssetType.Solana => "SOL",
            _ => "UNKNOWN"
        };
    }

    private async Task<MarketPriceResult?> GetCachedMarketPriceAsync(string cacheKey, CancellationToken cancellationToken)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<MarketPriceResult>(cachedValue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving cached market price for key {CacheKey}", cacheKey);
        }

        return null;
    }

    private async Task CacheMarketPriceAsync(string cacheKey, MarketPriceResult result, CancellationToken cancellationToken)
    {
        try
        {
            var cacheValue = JsonSerializer.Serialize(result);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Cache for 5 minutes
            };

            await _cache.SetStringAsync(cacheKey, cacheValue, cacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error caching market price for key {CacheKey}", cacheKey);
        }
    }

    private async Task<MarketConditions?> GetCachedMarketConditionsAsync(string cacheKey, CancellationToken cancellationToken)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializer.Deserialize<MarketConditions>(cachedValue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving cached market conditions for key {CacheKey}", cacheKey);
        }

        return null;
    }

    private async Task CacheMarketConditionsAsync(string cacheKey, MarketConditions conditions, CancellationToken cancellationToken)
    {
        try
        {
            var cacheValue = JsonSerializer.Serialize(conditions);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };

            await _cache.SetStringAsync(cacheKey, cacheValue, cacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error caching market conditions for key {CacheKey}", cacheKey);
        }
    }
}
