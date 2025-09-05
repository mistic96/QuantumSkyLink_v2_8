using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services;

public class PricingService : IPricingService
{
    private readonly MarketplaceDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PricingService> _logger;

    public PricingService(
        MarketplaceDbContext context,
        IDistributedCache cache,
        ILogger<PricingService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PriceCalculationResult> CalculatePriceAsync(Guid listingId, decimal quantity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating price for listing {ListingId} with quantity {Quantity}", listingId, quantity);

        try
        {
            var listing = await _context.MarketListings
                .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

            if (listing == null)
            {
                return new PriceCalculationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Listing {listingId} not found",
                    CalculatedAt = DateTime.UtcNow
                };
            }

            if (listing.Status != ListingStatus.Active)
            {
                return new PriceCalculationResult
                {
                    IsValid = false,
                    ErrorMessage = "Listing is not active",
                    CalculatedAt = DateTime.UtcNow
                };
            }

            if (quantity <= 0)
            {
                return new PriceCalculationResult
                {
                    IsValid = false,
                    ErrorMessage = "Quantity must be greater than zero",
                    CalculatedAt = DateTime.UtcNow
                };
            }

            if (quantity > listing.RemainingQuantity)
            {
                return new PriceCalculationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Requested quantity {quantity} exceeds available quantity {listing.RemainingQuantity}",
                    CalculatedAt = DateTime.UtcNow
                };
            }

            var pricePerToken = await CalculatePricePerTokenAsync(listing, quantity, cancellationToken);
            var totalPrice = pricePerToken * quantity;

            var result = new PriceCalculationResult
            {
                PricePerToken = pricePerToken,
                TotalPrice = totalPrice,
                Quantity = quantity,
                Currency = listing.Currency,
                Strategy = listing.PricingStrategy,
                IsValid = true,
                CalculatedAt = DateTime.UtcNow,
                CalculationDetails = $"Calculated using {listing.PricingStrategy} strategy"
            };

            // Add pricing tiers if applicable
            if (listing.PricingStrategy == PricingStrategy.Tiered)
            {
                result.AppliedTiers = await GetPricingTiersAsync(listingId, cancellationToken);
            }

            _logger.LogInformation("Calculated price for listing {ListingId}: {PricePerToken} per token, total {TotalPrice}", 
                listingId, pricePerToken, totalPrice);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for listing {ListingId}", listingId);
            return new PriceCalculationResult
            {
                IsValid = false,
                ErrorMessage = $"Error calculating price: {ex.Message}",
                CalculatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<PriceUpdateResult> UpdateMarginBasedPricingAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating margin-based pricing for listing {ListingId}", listingId);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            return new PriceUpdateResult
            {
                ListingId = listingId,
                IsSuccessful = false,
                ErrorMessage = "Listing not found",
                UpdatedAt = DateTime.UtcNow
            };
        }

        if (listing.PricingStrategy != PricingStrategy.MarginFixed && listing.PricingStrategy != PricingStrategy.MarginPercentage)
        {
            return new PriceUpdateResult
            {
                ListingId = listingId,
                IsSuccessful = false,
                ErrorMessage = "Listing does not use margin-based pricing",
                UpdatedAt = DateTime.UtcNow
            };
        }

        try
        {
            var marketPrice = await GetMarketPriceAsync(listing.AssetType, listing.AssetSymbol, listing.TokenId, cancellationToken);
            
            if (!marketPrice.IsValid)
            {
                return new PriceUpdateResult
                {
                    ListingId = listingId,
                    IsSuccessful = false,
                    ErrorMessage = $"Could not get market price: {marketPrice.ErrorMessage}",
                    UpdatedAt = DateTime.UtcNow
                };
            }

            var config = ParsePricingConfiguration(listing.PricingConfiguration);
            var margin = GetMarginFromConfig(config);
            var oldPrice = listing.BasePrice ?? 0;
            var newPrice = marketPrice.Price * (1 + margin);

            // Only update if price changed significantly (more than 1%)
            if (Math.Abs(newPrice - oldPrice) / oldPrice > 0.01m)
            {
                listing.BasePrice = newPrice;
                listing.UpdatedAt = DateTime.UtcNow;

                await CreatePriceHistoryAsync(listingId, newPrice, "Margin-based price update", true, null, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated margin-based price for listing {ListingId} from {OldPrice} to {NewPrice}", 
                    listingId, oldPrice, newPrice);
            }

            return new PriceUpdateResult
            {
                ListingId = listingId,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                Currency = listing.Currency,
                UpdateReason = "Margin-based price update",
                IsSuccessful = true,
                UpdatedAt = DateTime.UtcNow,
                MarketPrice = marketPrice
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating margin-based pricing for listing {ListingId}", listingId);
            return new PriceUpdateResult
            {
                ListingId = listingId,
                IsSuccessful = false,
                ErrorMessage = $"Error updating price: {ex.Message}",
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<MarketPriceResult> GetMarketPriceAsync(AssetType assetType, string? assetSymbol = null, Guid? tokenId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting market price for asset type {AssetType}, symbol {AssetSymbol}, token {TokenId}", 
            assetType, assetSymbol, tokenId);

        try
        {
            // For platform tokens, get average price from active listings
            if (assetType == AssetType.PlatformToken && tokenId.HasValue)
            {
                var avgPrice = await _context.MarketListings
                    .Where(l => l.TokenId == tokenId.Value && l.Status == ListingStatus.Active)
                    .AverageAsync(l => (decimal?)l.BasePrice, cancellationToken);

                if (avgPrice.HasValue)
                {
                    return new MarketPriceResult
                    {
                        AssetType = assetType,
                        TokenId = tokenId,
                        Price = avgPrice.Value,
                        Currency = "USD",
                        Source = "Platform Listings",
                        Timestamp = DateTime.UtcNow,
                        IsValid = true
                    };
                }
            }

            // For external assets, get price from external sources (mock implementation)
            if (assetType == AssetType.OtherCrypto && !string.IsNullOrEmpty(assetSymbol))
            {
                var mockPrice = await GetExternalAssetPriceAsync(assetSymbol, cancellationToken);
                return new MarketPriceResult
                {
                    AssetType = assetType,
                    AssetSymbol = assetSymbol,
                    Price = mockPrice,
                    Currency = "USD",
                    Source = "External API",
                    Timestamp = DateTime.UtcNow,
                    IsValid = true
                };
            }

            return new MarketPriceResult
            {
                AssetType = assetType,
                AssetSymbol = assetSymbol,
                TokenId = tokenId,
                IsValid = false,
                ErrorMessage = "No market price available for this asset",
                Timestamp = DateTime.UtcNow
            };
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

    public async Task<PricingValidationResult> ValidatePricingConfigurationAsync(PricingStrategy strategy, string configuration, decimal? basePrice = null)
    {
        _logger.LogInformation("Validating pricing configuration for strategy {Strategy}", strategy);

        var result = new PricingValidationResult();
        var errors = new List<string>();
        var warnings = new List<string>();

        try
        {
            var config = ParsePricingConfiguration(configuration);

            switch (strategy)
            {
                case PricingStrategy.Fixed:
                    ValidateFixedPricingConfig(config, basePrice, errors, warnings);
                    break;
                case PricingStrategy.Bulk:
                    ValidateBulkPricingConfig(config, errors, warnings);
                    break;
                case PricingStrategy.MarginFixed:
                case PricingStrategy.MarginPercentage:
                    ValidateMarginBasedConfig(config, errors, warnings);
                    break;
                case PricingStrategy.Tiered:
                    ValidateTieredPricingConfig(config, errors, warnings);
                    break;
                case PricingStrategy.Dynamic:
                    ValidateDynamicPricingConfig(config, errors, warnings);
                    break;
                default:
                    errors.Add($"Unsupported pricing strategy: {strategy}");
                    break;
            }

            result.IsValid = !errors.Any();
            result.Errors = errors;
            result.Warnings = warnings;
            result.ParsedConfiguration = JsonSerializer.Serialize(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating pricing configuration");
            result.IsValid = false;
            result.Errors = new[] { $"Invalid configuration format: {ex.Message}" };
        }

        return result;
    }

    public async Task<IEnumerable<PricingTier>> GetPricingTiersAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            return Enumerable.Empty<PricingTier>();
        }

        var config = ParsePricingConfiguration(listing.PricingConfiguration);

        return listing.PricingStrategy switch
        {
            PricingStrategy.Tiered => GetTieredPricingTiers(config, listing.Currency),
            PricingStrategy.Bulk => GetBulkPricingTiers(listing.BasePrice ?? 0, config, listing.Currency),
            _ => new[]
            {
                new PricingTier
                {
                    MinQuantity = 1,
                    MaxQuantity = null,
                    PricePerToken = listing.BasePrice ?? 0,
                    Currency = listing.Currency,
                    Description = "Standard pricing"
                }
            }
        };
    }

    public async Task<PriceUpdateResult> UpdateDynamicPricingAsync(Guid listingId, MarketConditions marketConditions, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating dynamic pricing for listing {ListingId}", listingId);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            return new PriceUpdateResult
            {
                ListingId = listingId,
                IsSuccessful = false,
                ErrorMessage = "Listing not found",
                UpdatedAt = DateTime.UtcNow
            };
        }

        if (listing.PricingStrategy != PricingStrategy.Dynamic)
        {
            return new PriceUpdateResult
            {
                ListingId = listingId,
                IsSuccessful = false,
                ErrorMessage = "Listing does not use dynamic pricing",
                UpdatedAt = DateTime.UtcNow
            };
        }

        try
        {
            var config = ParsePricingConfiguration(listing.PricingConfiguration);
            var oldPrice = listing.BasePrice ?? 0;
            var newPrice = CalculateDynamicPrice(listing.BasePrice ?? 0, marketConditions, config);

            // Only update if price changed significantly
            if (Math.Abs(newPrice - oldPrice) / oldPrice > 0.005m) // 0.5% threshold
            {
                listing.BasePrice = newPrice;
                listing.UpdatedAt = DateTime.UtcNow;

                await CreatePriceHistoryAsync(listingId, newPrice, "Dynamic price update", true, null, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated dynamic price for listing {ListingId} from {OldPrice} to {NewPrice}", 
                    listingId, oldPrice, newPrice);
            }

            return new PriceUpdateResult
            {
                ListingId = listingId,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                Currency = listing.Currency,
                UpdateReason = "Dynamic price update based on market conditions",
                IsSuccessful = true,
                UpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dynamic pricing for listing {ListingId}", listingId);
            return new PriceUpdateResult
            {
                ListingId = listingId,
                IsSuccessful = false,
                ErrorMessage = $"Error updating price: {ex.Message}",
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<IEnumerable<Guid>> GetListingsNeedingPriceUpdateAsync(PricingStrategy? strategy = null, CancellationToken cancellationToken = default)
    {
        var query = _context.MarketListings
            .Where(l => l.Status == ListingStatus.Active && (l.PricingStrategy == PricingStrategy.Dynamic || l.PricingStrategy == PricingStrategy.MarginFixed || l.PricingStrategy == PricingStrategy.MarginPercentage));

        if (strategy.HasValue)
        {
            query = query.Where(l => l.PricingStrategy == strategy.Value);
        }

        // Get listings that haven't been updated in the last hour
        var cutoffTime = DateTime.UtcNow.AddHours(-1);
        query = query.Where(l => l.UpdatedAt < cutoffTime);

        return await query.Select(l => l.Id).ToListAsync(cancellationToken);
    }

    public async Task<BulkPriceUpdateResult> BulkUpdatePricesAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting bulk price update for {Count} listings", listingIds.Count());

        var startTime = DateTime.UtcNow;
        var results = new List<PriceUpdateResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var listingId in listingIds)
        {
            try
            {
                var listing = await _context.MarketListings
                    .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

                if (listing == null)
                {
                    results.Add(new PriceUpdateResult
                    {
                        ListingId = listingId,
                        IsSuccessful = false,
                        ErrorMessage = "Listing not found",
                        UpdatedAt = DateTime.UtcNow
                    });
                    failureCount++;
                    continue;
                }

                PriceUpdateResult result = listing.PricingStrategy switch
                {
                    PricingStrategy.MarginFixed or PricingStrategy.MarginPercentage => await UpdateMarginBasedPricingAsync(listingId, cancellationToken),
                    PricingStrategy.Dynamic => await UpdateDynamicPricingAsync(listingId, new MarketConditions { Timestamp = DateTime.UtcNow }, cancellationToken),
                    _ => new PriceUpdateResult
                    {
                        ListingId = listingId,
                        IsSuccessful = false,
                        ErrorMessage = "Pricing strategy does not support automatic updates",
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                results.Add(result);
                if (result.IsSuccessful)
                    successCount++;
                else
                    failureCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating price for listing {ListingId}", listingId);
                results.Add(new PriceUpdateResult
                {
                    ListingId = listingId,
                    IsSuccessful = false,
                    ErrorMessage = $"Error: {ex.Message}",
                    UpdatedAt = DateTime.UtcNow
                });
                failureCount++;
            }
        }

        var endTime = DateTime.UtcNow;
        _logger.LogInformation("Completed bulk price update: {Success} successful, {Failed} failed", successCount, failureCount);

        return new BulkPriceUpdateResult
        {
            TotalListings = listingIds.Count(),
            SuccessfulUpdates = successCount,
            FailedUpdates = failureCount,
            Results = results,
            ProcessedAt = endTime,
            ProcessingTime = endTime - startTime
        };
    }

    public async Task<PriceHistoryDto> CreatePriceHistoryAsync(Guid listingId, decimal pricePerToken, string? reason = null, bool isAutomatic = false, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var priceHistory = new PriceHistory
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            PricePerToken = pricePerToken,
            Currency = "USD", // Default, should be retrieved from listing
            PricingStrategy = PricingStrategy.Fixed, // Default, should be retrieved from listing
            ChangeReason = reason,
            IsAutomaticUpdate = isAutomatic,
            UpdatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PriceHistory.Add(priceHistory);
        await _context.SaveChangesAsync(cancellationToken);

        return new PriceHistoryDto
        {
            Id = priceHistory.Id,
            PricePerToken = priceHistory.PricePerToken,
            Currency = priceHistory.Currency,
            PricingStrategy = priceHistory.PricingStrategy,
            ChangeReason = priceHistory.ChangeReason,
            IsAutomaticUpdate = priceHistory.IsAutomaticUpdate,
            CreatedAt = priceHistory.CreatedAt
        };
    }

    // Private helper methods
    private async Task<decimal> CalculatePricePerTokenAsync(MarketListing listing, decimal quantity, CancellationToken cancellationToken)
    {
        var config = ParsePricingConfiguration(listing.PricingConfiguration);

        return listing.PricingStrategy switch
        {
            PricingStrategy.Fixed => listing.BasePrice ?? 0,
            PricingStrategy.Bulk => CalculateBulkPrice(listing.BasePrice ?? 0, quantity, config),
            PricingStrategy.MarginFixed or PricingStrategy.MarginPercentage => await CalculateMarginBasedPriceAsync(listing, config, cancellationToken),
            PricingStrategy.Tiered => CalculateTieredPrice(quantity, config),
            PricingStrategy.Dynamic => listing.BasePrice ?? 0, // Current price, updated separately
            _ => listing.BasePrice ?? 0
        };
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

    private static decimal GetMarginFromConfig(Dictionary<string, object> config)
    {
        if (config.TryGetValue("margin", out var marginObj) && decimal.TryParse(marginObj.ToString(), out var margin))
            return margin;
        return 0.2m; // Default 20% margin
    }

    private static decimal CalculateBulkPrice(decimal basePrice, decimal quantity, Dictionary<string, object> config)
    {
        if (config.TryGetValue("bulkDiscount", out var discountObj) && decimal.TryParse(discountObj.ToString(), out var discount))
        {
            var minQuantity = 10m; // Default minimum for bulk discount
            if (config.TryGetValue("minBulkQuantity", out var minObj) && decimal.TryParse(minObj.ToString(), out var min))
                minQuantity = min;

            if (quantity >= minQuantity)
                return basePrice * (1 - discount);
        }
        return basePrice;
    }

    private async Task<decimal> CalculateMarginBasedPriceAsync(MarketListing listing, Dictionary<string, object> config, CancellationToken cancellationToken)
    {
        var marketPrice = await GetMarketPriceAsync(listing.AssetType, listing.AssetSymbol, listing.TokenId, cancellationToken);
        if (!marketPrice.IsValid)
            return listing.BasePrice ?? 0;

        var margin = GetMarginFromConfig(config);
        return marketPrice.Price * (1 + margin);
    }

    private static decimal CalculateTieredPrice(decimal quantity, Dictionary<string, object> config)
    {
        var tiers = GetTieredPricingTiers(config, "USD");
        
        foreach (var tier in tiers.OrderBy(t => t.MinQuantity))
        {
            if (quantity >= tier.MinQuantity && (tier.MaxQuantity == null || quantity <= tier.MaxQuantity))
                return tier.PricePerToken;
        }

        return tiers.LastOrDefault()?.PricePerToken ?? 0;
    }

    private static decimal CalculateDynamicPrice(decimal basePrice, MarketConditions marketConditions, Dictionary<string, object> config)
    {
        var volatilityFactor = 1 + (marketConditions.Volatility * 0.1m);
        var volumeFactor = 1 + (marketConditions.TradingVolume24h / 1000000m * 0.05m);
        var priceFactor = 1 + (marketConditions.PriceChange24h * 0.5m);

        var adjustedPrice = basePrice * volatilityFactor * volumeFactor * priceFactor;

        // Apply min/max constraints from config
        if (config.TryGetValue("maxMultiplier", out var maxObj) && decimal.TryParse(maxObj.ToString(), out var maxMultiplier))
            adjustedPrice = Math.Min(adjustedPrice, basePrice * maxMultiplier);

        if (config.TryGetValue("minMultiplier", out var minObj) && decimal.TryParse(minObj.ToString(), out var minMultiplier))
            adjustedPrice = Math.Max(adjustedPrice, basePrice * minMultiplier);

        return adjustedPrice;
    }

    private static async Task<decimal> GetExternalAssetPriceAsync(string assetSymbol, CancellationToken cancellationToken)
    {
        // Mock implementation - in real scenario, this would call external APIs
        await Task.Delay(100, cancellationToken);
        
        return assetSymbol.ToUpper() switch
        {
            "BTC" => 45000m,
            "ETH" => 3000m,
            "USDT" => 1m,
            "USDC" => 1m,
            _ => 100m // Default mock price
        };
    }

    private static IEnumerable<PricingTier> GetTieredPricingTiers(Dictionary<string, object> config, string currency)
    {
        if (!config.TryGetValue("tiers", out var tiersObj))
            return Enumerable.Empty<PricingTier>();

        try
        {
            var tiersJson = JsonSerializer.Serialize(tiersObj);
            var tiers = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(tiersJson);
            
            return tiers?.Select(t => new PricingTier
            {
                MinQuantity = decimal.TryParse(t.GetValueOrDefault("minQuantity")?.ToString(), out var min) ? min : 1,
                MaxQuantity = decimal.TryParse(t.GetValueOrDefault("maxQuantity")?.ToString(), out var max) ? max : null,
                PricePerToken = decimal.TryParse(t.GetValueOrDefault("price")?.ToString(), out var price) ? price : 0,
                Currency = currency,
                Description = t.GetValueOrDefault("description")?.ToString()
            }) ?? Enumerable.Empty<PricingTier>();
        }
        catch
        {
            return Enumerable.Empty<PricingTier>();
        }
    }

    private static IEnumerable<PricingTier> GetBulkPricingTiers(decimal basePrice, Dictionary<string, object> config, string currency)
    {
        var tiers = new List<PricingTier>
        {
            new PricingTier
            {
                MinQuantity = 1,
                MaxQuantity = 9,
                PricePerToken = basePrice,
                Currency = currency,
                Description = "Standard pricing"
            }
        };

        if (config.TryGetValue("bulkDiscount", out var discountObj) && decimal.TryParse(discountObj.ToString(), out var discount))
        {
            var minBulkQuantity = 10m;
            if (config.TryGetValue("minBulkQuantity", out var minObj) && decimal.TryParse(minObj.ToString(), out var min))
                minBulkQuantity = min;

            tiers.Add(new PricingTier
            {
                MinQuantity = minBulkQuantity,
                MaxQuantity = null,
                PricePerToken = basePrice * (1 - discount),
                Currency = currency,
                DiscountPercentage = discount * 100,
                Description = $"Bulk pricing ({discount:P0} discount)"
            });
        }

        return tiers;
    }

    private static void ValidateFixedPricingConfig(Dictionary<string, object> config, decimal? basePrice, List<string> errors, List<string> warnings)
    {
        if (!basePrice.HasValue || basePrice <= 0)
            errors.Add("Base price must be greater than zero for fixed pricing");
    }

    private static void ValidateBulkPricingConfig(Dictionary<string, object> config, List<string> errors, List<string> warnings)
    {
        if (config.TryGetValue("bulkDiscount", out var discountObj) && decimal.TryParse(discountObj.ToString(), out var discount))
        {
            if (discount < 0 || discount > 1)
                errors.Add("Bulk discount must be between 0 and 1");
        }
    }

    private static void ValidateMarginBasedConfig(Dictionary<string, object> config, List<string> errors, List<string> warnings)
    {
        if (!config.ContainsKey("margin"))
            errors.Add("Margin configuration must include 'margin' field");
        else if (config.TryGetValue("margin", out var marginObj) && decimal.TryParse(marginObj.ToString(), out var margin))
        {
            if (margin < 0)
                errors.Add("Margin cannot be negative");
            if (margin > 5)
                warnings.Add("Margin is very high (>500%)");
        }
    }

    private static void ValidateTieredPricingConfig(Dictionary<string, object> config, List<string> errors, List<string> warnings)
    {
        if (!config.ContainsKey("tiers"))
            errors.Add("Tiered pricing configuration must include 'tiers' field");
    }

    private static void ValidateDynamicPricingConfig(Dictionary<string, object> config, List<string> errors, List<string> warnings)
    {
        if (config.TryGetValue("maxMultiplier", out var maxObj) && decimal.TryParse(maxObj.ToString(), out var maxMultiplier))
        {
            if (maxMultiplier <= 1)
                warnings.Add("Maximum price multiplier should be greater than 1");
        }

        if (config.TryGetValue("minMultiplier", out var minObj) && decimal.TryParse(minObj.ToString(), out var minMultiplier))
        {
            if (minMultiplier >= 1)
                warnings.Add("Minimum price multiplier should be less than 1");
        }
    }
}
