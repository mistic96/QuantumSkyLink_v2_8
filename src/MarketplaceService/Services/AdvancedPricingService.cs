using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;
using MarketplaceService.Services.PricingStrategies;
using Clients = MarketplaceService.Services.Clients;

namespace MarketplaceService.Services;

/// <summary>
/// Advanced pricing service implementing sophisticated pricing strategies for marketplace operations
/// Follows PaymentGatewayService pattern with direct response returns and exception-based error handling
/// </summary>
public class AdvancedPricingService : IAdvancedPricingService
{
    private readonly MarketplaceDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AdvancedPricingService> _logger;
    private readonly Clients.ITokenServiceClient _tokenServiceClient;
    private readonly IMarketDataService _marketDataService;
    private readonly Dictionary<PricingStrategy, IPricingStrategyHandler> _strategyHandlers;

    public AdvancedPricingService(
        MarketplaceDbContext context,
        IDistributedCache cache,
        ILogger<AdvancedPricingService> logger,
        Clients.ITokenServiceClient tokenServiceClient,
        IMarketDataService marketDataService)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _tokenServiceClient = tokenServiceClient;
        _marketDataService = marketDataService;

        // Initialize strategy handlers using strategy pattern
        _strategyHandlers = new Dictionary<PricingStrategy, IPricingStrategyHandler>
        {
            { PricingStrategy.Fixed, new FixedPricingHandler(_logger) },
            { PricingStrategy.Bulk, new BulkPricingHandler(_logger) },
            { PricingStrategy.MarginFixed, new MarginFixedPricingHandler(_logger, _marketDataService) },
            { PricingStrategy.MarginPercentage, new MarginPercentagePricingHandler(_logger, _marketDataService) },
            { PricingStrategy.Tiered, new TieredPricingHandler(_logger) },
            { PricingStrategy.Dynamic, new DynamicPricingHandler(_logger, _marketDataService) },
            { PricingStrategy.Unit, new UnitPricingHandler(_logger) }
        };
    }

    /// <summary>
    /// Calculate price using advanced pricing strategies with comprehensive business logic
    /// </summary>
    public async Task<AdvancedPriceCalculationResult> CalculateAdvancedPriceAsync(
        Guid listingId, 
        decimal quantity, 
        Guid? buyerId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating advanced price for listing {ListingId} with quantity {Quantity}", 
            listingId, quantity);

        try
        {
            var listing = await GetListingWithValidationAsync(listingId, cancellationToken);
            
            // Validate quantity constraints
            ValidateQuantityConstraints(listing, quantity);

            // Get the appropriate strategy handler
            if (!_strategyHandlers.TryGetValue(listing.PricingStrategy, out var handler))
            {
                throw new InvalidOperationException($"Unsupported pricing strategy: {listing.PricingStrategy}");
            }

            // Parse pricing configuration
            var config = ParsePricingConfiguration(listing.PricingConfiguration);

            // Calculate price using strategy handler
            var strategyResult = await handler.CalculatePriceAsync(listing, quantity, config, cancellationToken);

            // Apply any additional business rules
            var finalResult = await ApplyBusinessRulesAsync(listing, strategyResult, buyerId, cancellationToken);

            // Cache the result for performance
            await CachePriceCalculationAsync(listingId, quantity, finalResult, cancellationToken);

            _logger.LogInformation("Successfully calculated advanced price for listing {ListingId}: {TotalPrice} {Currency}", 
                listingId, finalResult.TotalPrice, finalResult.Currency);

            return finalResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating advanced price for listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Validate bulk pricing requirements - all-or-nothing purchase logic
    /// </summary>
    public async Task<BulkPricingValidationResult> ValidateBulkPurchaseAsync(
        Guid listingId, 
        decimal requestedQuantity,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating bulk purchase for listing {ListingId} with quantity {Quantity}", 
            listingId, requestedQuantity);

        var listing = await GetListingWithValidationAsync(listingId, cancellationToken);

        if (listing.PricingStrategy != PricingStrategy.Bulk)
        {
            throw new InvalidOperationException("Listing does not use bulk pricing strategy");
        }

        var config = ParsePricingConfiguration(listing.PricingConfiguration);
        var bulkConfig = JsonSerializer.Deserialize<BulkPricingConfiguration>(JsonSerializer.Serialize(config));

        // Bulk pricing requires all-or-nothing purchase
        var isValidBulkPurchase = Math.Abs(requestedQuantity - listing.RemainingQuantity) < 0.00000001m;

        return new BulkPricingValidationResult
        {
            IsValid = isValidBulkPurchase,
            RequiredQuantity = listing.RemainingQuantity,
            RequestedQuantity = requestedQuantity,
            TotalPrice = bulkConfig?.TotalPrice ?? (listing.BasePrice ?? 0) * listing.RemainingQuantity,
            Currency = listing.Currency,
            ErrorMessage = isValidBulkPurchase ? null : 
                $"Bulk pricing requires purchasing entire quantity of {listing.RemainingQuantity:F8} tokens. Cannot purchase {requestedQuantity:F8} tokens.",
            ValidatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get tiered pricing breakdown with detailed tier information
    /// </summary>
    public async Task<TieredPricingBreakdownResult> GetTieredPricingBreakdownAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tiered pricing breakdown for listing {ListingId}", listingId);

        var listing = await GetListingWithValidationAsync(listingId, cancellationToken);

        if (listing.PricingStrategy != PricingStrategy.Tiered)
        {
            throw new InvalidOperationException("Listing does not use tiered pricing strategy");
        }

        var config = ParsePricingConfiguration(listing.PricingConfiguration);
        var tieredConfig = JsonSerializer.Deserialize<TieredPricingConfiguration>(JsonSerializer.Serialize(config));

        var tiers = tieredConfig?.Tiers?.Select(t => new PricingTierDetail
        {
            MinQuantity = t.MinQuantity,
            MaxQuantity = t.MaxQuantity,
            PricePerToken = t.PricePerToken,
            Currency = listing.Currency,
            DiscountPercentage = CalculateDiscountPercentage(t.PricePerToken, listing.BasePrice ?? 0),
            Description = t.Description ?? $"Tier {t.MinQuantity}-{t.MaxQuantity ?? decimal.MaxValue}",
            IsAvailable = t.MinQuantity <= listing.RemainingQuantity
        }).OrderBy(t => t.MinQuantity).ToList() ?? new List<PricingTierDetail>();

        return new TieredPricingBreakdownResult
        {
            ListingId = listingId,
            Tiers = tiers,
            Currency = listing.Currency,
            RemainingQuantity = listing.RemainingQuantity,
            RetrievedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Update margin-based pricing with enhanced market data integration
    /// </summary>
    public async Task<MarginPricingUpdateResult> UpdateMarginBasedPricingAsync(
        Guid listingId,
        bool forceUpdate = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating margin-based pricing for listing {ListingId}", listingId);

        var listing = await GetListingWithValidationAsync(listingId, cancellationToken);

        if (listing.PricingStrategy != PricingStrategy.MarginFixed && 
            listing.PricingStrategy != PricingStrategy.MarginPercentage)
        {
            throw new InvalidOperationException("Listing does not use margin-based pricing strategy");
        }

        var config = ParsePricingConfiguration(listing.PricingConfiguration);
        var marginConfig = JsonSerializer.Deserialize<MarginPricingConfiguration>(JsonSerializer.Serialize(config));

        // Get current market price
        var marketPrice = await _marketDataService.GetMarketPriceAsync(
            listing.AssetType, listing.AssetSymbol, listing.TokenId, cancellationToken);

        if (!marketPrice.IsValid)
        {
            throw new InvalidOperationException($"Unable to get market price: {marketPrice.ErrorMessage}");
        }

        var oldPrice = listing.BasePrice ?? 0;
        var newPrice = CalculateMarginBasedPrice(marketPrice.Price, marginConfig, listing.PricingStrategy);

        // Only update if price changed significantly or forced
        var priceChangeThreshold = 0.01m; // 1%
        var shouldUpdate = forceUpdate || Math.Abs(newPrice - oldPrice) / oldPrice > priceChangeThreshold;

        if (shouldUpdate)
        {
            listing.BasePrice = newPrice;
            listing.UpdatedAt = DateTime.UtcNow;

            await CreatePriceHistoryRecordAsync(listingId, newPrice, 
                $"Margin-based price update: {listing.PricingStrategy}", true, null, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated margin-based price for listing {ListingId} from {OldPrice} to {NewPrice}", 
                listingId, oldPrice, newPrice);
        }

        return new MarginPricingUpdateResult
        {
            ListingId = listingId,
            OldPrice = oldPrice,
            NewPrice = newPrice,
            MarketPrice = marketPrice.Price,
            MarginType = listing.PricingStrategy,
            MarginValue = GetMarginValue(marginConfig, listing.PricingStrategy),
            Currency = listing.Currency,
            WasUpdated = shouldUpdate,
            UpdateReason = shouldUpdate ? "Significant price change detected" : "Price change below threshold",
            UpdatedAt = DateTime.UtcNow,
            MarketDataSource = marketPrice.Source
        };
    }

    /// <summary>
    /// Update dynamic pricing based on comprehensive market conditions
    /// </summary>
    public async Task<DynamicPricingUpdateResult> UpdateDynamicPricingAsync(
        Guid listingId,
        MarketConditions? marketConditions = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating dynamic pricing for listing {ListingId}", listingId);

        var listing = await GetListingWithValidationAsync(listingId, cancellationToken);

        if (listing.PricingStrategy != PricingStrategy.Dynamic)
        {
            throw new InvalidOperationException("Listing does not use dynamic pricing strategy");
        }

        var config = ParsePricingConfiguration(listing.PricingConfiguration);
        var dynamicConfig = JsonSerializer.Deserialize<DynamicPricingConfiguration>(JsonSerializer.Serialize(config));

        // Get market conditions if not provided
        marketConditions ??= await _marketDataService.GetMarketConditionsAsync(
            listing.AssetType, listing.AssetSymbol, listing.TokenId, cancellationToken);

        var oldPrice = listing.BasePrice ?? 0;
        var basePrice = dynamicConfig?.BasePrice ?? oldPrice;

        // Calculate dynamic price using market conditions
        var priceMultiplier = CalculateDynamicPriceMultiplier(marketConditions, dynamicConfig);
        var newPrice = basePrice * priceMultiplier;

        // Apply min/max constraints
        var minPrice = basePrice * (dynamicConfig?.MinPriceMultiplier ?? 0.5m);
        var maxPrice = basePrice * (dynamicConfig?.MaxPriceMultiplier ?? 2.0m);
        newPrice = Math.Max(minPrice, Math.Min(maxPrice, newPrice));

        // Update if price changed significantly
        var priceChangeThreshold = 0.005m; // 0.5%
        var shouldUpdate = Math.Abs(newPrice - oldPrice) / oldPrice > priceChangeThreshold;

        if (shouldUpdate)
        {
            listing.BasePrice = newPrice;
            listing.UpdatedAt = DateTime.UtcNow;

            await CreatePriceHistoryRecordAsync(listingId, newPrice, 
                "Dynamic price update based on market conditions", true, null, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated dynamic price for listing {ListingId} from {OldPrice} to {NewPrice}", 
                listingId, oldPrice, newPrice);
        }

        return new DynamicPricingUpdateResult
        {
            ListingId = listingId,
            OldPrice = oldPrice,
            NewPrice = newPrice,
            BasePrice = basePrice,
            PriceMultiplier = priceMultiplier,
            Currency = listing.Currency,
            WasUpdated = shouldUpdate,
            MarketConditions = marketConditions,
            AppliedFactors = GetAppliedPricingFactors(marketConditions, dynamicConfig),
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get comprehensive pricing analytics for a listing
    /// </summary>
    public async Task<PricingAnalyticsResult> GetPricingAnalyticsAsync(
        Guid listingId,
        TimeSpan? period = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting pricing analytics for listing {ListingId}", listingId);

        var listing = await GetListingWithValidationAsync(listingId, cancellationToken);
        period ??= TimeSpan.FromDays(30);

        var cutoffDate = DateTime.UtcNow.Subtract(period.Value);

        var priceHistory = await _context.PriceHistory
            .Where(ph => ph.ListingId == listingId && ph.CreatedAt >= cutoffDate)
            .OrderBy(ph => ph.CreatedAt)
            .ToListAsync(cancellationToken);

        var orders = await _context.MarketplaceOrders
            .Where(o => o.ListingId == listingId && o.CreatedAt >= cutoffDate && o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        return new PricingAnalyticsResult
        {
            ListingId = listingId,
            CurrentPrice = listing.BasePrice ?? 0,
            Currency = listing.Currency,
            PricingStrategy = listing.PricingStrategy,
            AnalysisPeriod = period.Value,
            PriceHistory = priceHistory.Select(ph => new PriceHistoryPoint
            {
                Price = ph.PricePerToken,
                Timestamp = ph.CreatedAt,
                ChangeReason = ph.ChangeReason,
                IsAutomatic = ph.IsAutomaticUpdate
            }).ToList(),
            Statistics = CalculatePriceStatistics(priceHistory, orders),
            GeneratedAt = DateTime.UtcNow
        };
    }

    // Private helper methods

    private async Task<MarketListing> GetListingWithValidationAsync(Guid listingId, CancellationToken cancellationToken)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {listingId} not found");
        }

        if (listing.Status != ListingStatus.Active)
        {
            throw new InvalidOperationException($"Listing {listingId} is not active (status: {listing.Status})");
        }

        return listing;
    }

    private static void ValidateQuantityConstraints(MarketListing listing, decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (quantity < listing.MinimumPurchaseQuantity)
        {
            throw new ArgumentException($"Quantity {quantity} is below minimum purchase quantity {listing.MinimumPurchaseQuantity}");
        }

        if (listing.MaximumPurchaseQuantity.HasValue && quantity > listing.MaximumPurchaseQuantity.Value)
        {
            throw new ArgumentException($"Quantity {quantity} exceeds maximum purchase quantity {listing.MaximumPurchaseQuantity}");
        }

        if (quantity > listing.RemainingQuantity)
        {
            throw new ArgumentException($"Quantity {quantity} exceeds available quantity {listing.RemainingQuantity}");
        }
    }

    private static Dictionary<string, object> ParsePricingConfiguration(string? configuration)
    {
        if (string.IsNullOrEmpty(configuration))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(configuration) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private async Task<AdvancedPriceCalculationResult> ApplyBusinessRulesAsync(
        MarketListing listing,
        AdvancedPriceCalculationResult strategyResult,
        Guid? buyerId,
        CancellationToken cancellationToken)
    {
        // Apply volume discounts for large purchases
        if (strategyResult.Quantity >= 1000 && listing.PricingStrategy != PricingStrategy.Bulk)
        {
            var volumeDiscount = 0.02m; // 2% discount for large volumes
            strategyResult.TotalPrice *= (1 - volumeDiscount);
            strategyResult.AppliedDiscounts.Add(new AppliedDiscount
            {
                Type = "Volume",
                Percentage = volumeDiscount * 100,
                Amount = strategyResult.TotalPrice * volumeDiscount,
                Description = "Large volume discount (1000+ tokens)"
            });
        }

        // Apply loyalty discounts for repeat customers
        if (buyerId.HasValue)
        {
            var previousOrders = await _context.MarketplaceOrders
                .CountAsync(o => o.BuyerId == buyerId.Value && o.Status == OrderStatus.Completed, cancellationToken);

            if (previousOrders >= 5)
            {
                var loyaltyDiscount = 0.01m; // 1% loyalty discount
                strategyResult.TotalPrice *= (1 - loyaltyDiscount);
                strategyResult.AppliedDiscounts.Add(new AppliedDiscount
                {
                    Type = "Loyalty",
                    Percentage = loyaltyDiscount * 100,
                    Amount = strategyResult.TotalPrice * loyaltyDiscount,
                    Description = "Loyal customer discount (5+ previous orders)"
                });
            }
        }

        return strategyResult;
    }

    private async Task CachePriceCalculationAsync(
        Guid listingId,
        decimal quantity,
        AdvancedPriceCalculationResult result,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"price_calc_{listingId}_{quantity}";
        var cacheValue = JsonSerializer.Serialize(result);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(cacheKey, cacheValue, cacheOptions, cancellationToken);
    }

    private static decimal CalculateMarginBasedPrice(decimal marketPrice, MarginPricingConfiguration? config, PricingStrategy strategy)
    {
        return strategy switch
        {
            PricingStrategy.MarginFixed => marketPrice + (config?.FixedMargin ?? 0),
            PricingStrategy.MarginPercentage => marketPrice * (1 + (config?.PercentageMargin ?? 0.2m)),
            _ => marketPrice
        };
    }

    private static decimal GetMarginValue(MarginPricingConfiguration? config, PricingStrategy strategy)
    {
        return strategy switch
        {
            PricingStrategy.MarginFixed => config?.FixedMargin ?? 0,
            PricingStrategy.MarginPercentage => config?.PercentageMargin ?? 0.2m,
            _ => 0
        };
    }

    private static decimal CalculateDynamicPriceMultiplier(MarketConditions marketConditions, DynamicPricingConfiguration? config)
    {
        var baseMultiplier = 1.0m;

        // Apply volatility factor
        var volatilityFactor = 1 + (marketConditions.Volatility * 0.1m);
        
        // Apply volume factor
        var volumeFactor = 1 + (marketConditions.TradingVolume24h / 1000000m * 0.05m);
        
        // Apply price change factor
        var priceChangeFactor = 1 + (marketConditions.PriceChange24h * 0.5m);

        return baseMultiplier * volatilityFactor * volumeFactor * priceChangeFactor;
    }

    private static List<string> GetAppliedPricingFactors(MarketConditions marketConditions, DynamicPricingConfiguration? config)
    {
        var factors = new List<string>();

        if (Math.Abs(marketConditions.Volatility) > 0.01m)
            factors.Add($"Volatility: {marketConditions.Volatility:P2}");

        if (marketConditions.TradingVolume24h > 0)
            factors.Add($"24h Volume: {marketConditions.TradingVolume24h:N0}");

        if (Math.Abs(marketConditions.PriceChange24h) > 0.01m)
            factors.Add($"24h Price Change: {marketConditions.PriceChange24h:P2}");

        return factors;
    }

    private static decimal CalculateDiscountPercentage(decimal tierPrice, decimal basePrice)
    {
        if (basePrice <= 0) return 0;
        return Math.Max(0, (basePrice - tierPrice) / basePrice * 100);
    }

    private async Task CreatePriceHistoryRecordAsync(
        Guid listingId,
        decimal pricePerToken,
        string? reason,
        bool isAutomatic,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var priceHistory = new PriceHistory
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            PricePerToken = pricePerToken,
            Currency = "USD", // Should be retrieved from listing
            PricingStrategy = PricingStrategy.Fixed, // Should be retrieved from listing
            ChangeReason = reason,
            IsAutomaticUpdate = isAutomatic,
            UpdatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PriceHistory.Add(priceHistory);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static PricingStatistics CalculatePriceStatistics(List<PriceHistory> priceHistory, List<MarketplaceOrder> orders)
    {
        var prices = priceHistory.Select(ph => ph.PricePerToken).ToList();
        
        return new PricingStatistics
        {
            MinPrice = prices.Any() ? prices.Min() : 0,
            MaxPrice = prices.Any() ? prices.Max() : 0,
            AveragePrice = prices.Any() ? prices.Average() : 0,
            PriceChanges = priceHistory.Count - 1,
            TotalVolume = orders.Sum(o => o.Quantity),
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            AverageOrderSize = orders.Any() ? orders.Average(o => o.Quantity) : 0
        };
    }
}
