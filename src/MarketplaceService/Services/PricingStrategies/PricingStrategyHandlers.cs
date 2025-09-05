using Microsoft.Extensions.Logging;
using System.Text.Json;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services.PricingStrategies;

/// <summary>
/// Fixed pricing strategy handler - simple direct pricing
/// </summary>
public class FixedPricingHandler : IPricingStrategyHandler
{
    private readonly ILogger _logger;

    public FixedPricingHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating fixed price for listing {ListingId}", listing.Id);

        var pricePerToken = listing.BasePrice ?? 0;
        var totalPrice = pricePerToken * quantity;

        return new AdvancedPriceCalculationResult
        {
            PricePerToken = pricePerToken,
            TotalPrice = totalPrice,
            Quantity = quantity,
            Currency = listing.Currency,
            Strategy = PricingStrategy.Fixed,
            CalculationDetails = $"Fixed price: {pricePerToken:C} per token",
            IsValid = true,
            CalculatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Bulk pricing strategy handler - all-or-nothing purchase requirement
/// </summary>
public class BulkPricingHandler : IPricingStrategyHandler
{
    private readonly ILogger _logger;

    public BulkPricingHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating bulk price for listing {ListingId}", listing.Id);

        var config = JsonSerializer.Deserialize<BulkPricingConfiguration>(JsonSerializer.Serialize(configuration));
        
        // Bulk pricing requires purchasing entire remaining quantity
        var isValidBulkPurchase = Math.Abs(quantity - listing.RemainingQuantity) < 0.00000001m;

        if (!isValidBulkPurchase)
        {
            return new AdvancedPriceCalculationResult
            {
                IsValid = false,
                ErrorMessage = $"Bulk pricing requires purchasing entire quantity of {listing.RemainingQuantity:F8} tokens",
                CalculatedAt = DateTime.UtcNow
            };
        }

        var totalPrice = config?.TotalPrice ?? (listing.BasePrice ?? 0) * listing.RemainingQuantity;
        var pricePerToken = totalPrice / quantity;

        return new AdvancedPriceCalculationResult
        {
            PricePerToken = pricePerToken,
            TotalPrice = totalPrice,
            Quantity = quantity,
            Currency = listing.Currency,
            Strategy = PricingStrategy.Bulk,
            CalculationDetails = $"Bulk pricing: {totalPrice:C} total for {quantity:F8} tokens (all-or-nothing)",
            IsValid = true,
            CalculatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Margin-based pricing with fixed dollar margin
/// </summary>
public class MarginFixedPricingHandler : IPricingStrategyHandler
{
    private readonly ILogger _logger;
    private readonly IMarketDataService _marketDataService;

    public MarginFixedPricingHandler(ILogger logger, IMarketDataService marketDataService)
    {
        _logger = logger;
        _marketDataService = marketDataService;
    }

    public async Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating margin-fixed price for listing {ListingId}", listing.Id);

        var config = JsonSerializer.Deserialize<MarginPricingConfiguration>(JsonSerializer.Serialize(configuration));
        var fixedMargin = config?.FixedMargin ?? 0;

        // Get current market price
        var marketPrice = await _marketDataService.GetMarketPriceAsync(
            listing.AssetType, listing.AssetSymbol, listing.TokenId, cancellationToken);

        if (!marketPrice.IsValid)
        {
            return new AdvancedPriceCalculationResult
            {
                IsValid = false,
                ErrorMessage = $"Unable to get market price: {marketPrice.ErrorMessage}",
                CalculatedAt = DateTime.UtcNow
            };
        }

        var pricePerToken = marketPrice.Price + fixedMargin;
        var totalPrice = pricePerToken * quantity;

        return new AdvancedPriceCalculationResult
        {
            PricePerToken = pricePerToken,
            TotalPrice = totalPrice,
            Quantity = quantity,
            Currency = listing.Currency,
            Strategy = PricingStrategy.MarginFixed,
            CalculationDetails = $"Market price {marketPrice.Price:C} + fixed margin {fixedMargin:C} = {pricePerToken:C}",
            IsValid = true,
            CalculatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["MarketPrice"] = marketPrice.Price,
                ["FixedMargin"] = fixedMargin,
                ["MarketDataSource"] = marketPrice.Source ?? "Unknown"
            }
        };
    }
}

/// <summary>
/// Margin-based pricing with percentage margin
/// </summary>
public class MarginPercentagePricingHandler : IPricingStrategyHandler
{
    private readonly ILogger _logger;
    private readonly IMarketDataService _marketDataService;

    public MarginPercentagePricingHandler(ILogger logger, IMarketDataService marketDataService)
    {
        _logger = logger;
        _marketDataService = marketDataService;
    }

    public async Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating margin-percentage price for listing {ListingId}", listing.Id);

        var config = JsonSerializer.Deserialize<MarginPricingConfiguration>(JsonSerializer.Serialize(configuration));
        var percentageMargin = config?.PercentageMargin ?? 0.2m; // Default 20%

        // Get current market price
        var marketPrice = await _marketDataService.GetMarketPriceAsync(
            listing.AssetType, listing.AssetSymbol, listing.TokenId, cancellationToken);

        if (!marketPrice.IsValid)
        {
            return new AdvancedPriceCalculationResult
            {
                IsValid = false,
                ErrorMessage = $"Unable to get market price: {marketPrice.ErrorMessage}",
                CalculatedAt = DateTime.UtcNow
            };
        }

        var pricePerToken = marketPrice.Price * (1 + percentageMargin);
        var totalPrice = pricePerToken * quantity;

        return new AdvancedPriceCalculationResult
        {
            PricePerToken = pricePerToken,
            TotalPrice = totalPrice,
            Quantity = quantity,
            Currency = listing.Currency,
            Strategy = PricingStrategy.MarginPercentage,
            CalculationDetails = $"Market price {marketPrice.Price:C} + {percentageMargin:P} margin = {pricePerToken:C}",
            IsValid = true,
            CalculatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["MarketPrice"] = marketPrice.Price,
                ["PercentageMargin"] = percentageMargin,
                ["MarketDataSource"] = marketPrice.Source ?? "Unknown"
            }
        };
    }
}

/// <summary>
/// Tiered pricing strategy handler - different prices for different quantity ranges
/// </summary>
public class TieredPricingHandler : IPricingStrategyHandler
{
    private readonly ILogger _logger;

    public TieredPricingHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating tiered price for listing {ListingId}", listing.Id);

        var config = JsonSerializer.Deserialize<TieredPricingConfiguration>(JsonSerializer.Serialize(configuration));
        var tiers = config?.Tiers?.OrderBy(t => t.MinQuantity).ToList() ?? new List<PricingTier>();

        if (!tiers.Any())
        {
            return new AdvancedPriceCalculationResult
            {
                IsValid = false,
                ErrorMessage = "No pricing tiers configured",
                CalculatedAt = DateTime.UtcNow
            };
        }

        // Find the appropriate tier for the requested quantity
        var applicableTier = tiers.LastOrDefault(t => quantity >= t.MinQuantity && 
            (t.MaxQuantity == null || quantity <= t.MaxQuantity));

        if (applicableTier == null)
        {
            return new AdvancedPriceCalculationResult
            {
                IsValid = false,
                ErrorMessage = $"No pricing tier available for quantity {quantity}",
                CalculatedAt = DateTime.UtcNow
            };
        }

        var pricePerToken = applicableTier.PricePerToken;
        var totalPrice = pricePerToken * quantity;

        var appliedTiers = new List<PricingTierDetail>
        {
            new PricingTierDetail
            {
                MinQuantity = applicableTier.MinQuantity,
                MaxQuantity = applicableTier.MaxQuantity,
                PricePerToken = applicableTier.PricePerToken,
                Currency = listing.Currency,
                Description = applicableTier.Description,
                IsAvailable = true
            }
        };

        return new AdvancedPriceCalculationResult
        {
            PricePerToken = pricePerToken,
            TotalPrice = totalPrice,
            Quantity = quantity,
            Currency = listing.Currency,
            Strategy = PricingStrategy.Tiered,
            CalculationDetails = $"Tier {applicableTier.MinQuantity}-{applicableTier.MaxQuantity?.ToString() ?? "∞"}: {pricePerToken:C} per token",
            IsValid = true,
            CalculatedAt = DateTime.UtcNow,
            AppliedTiers = appliedTiers
        };
    }
}

/// <summary>
/// Dynamic pricing strategy handler - price changes based on market conditions
/// </summary>
public class DynamicPricingHandler : IPricingStrategyHandler
{
    private readonly ILogger _logger;
    private readonly IMarketDataService _marketDataService;

    public DynamicPricingHandler(ILogger logger, IMarketDataService marketDataService)
    {
        _logger = logger;
        _marketDataService = marketDataService;
    }

    public async Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating dynamic price for listing {ListingId}", listing.Id);

        var config = JsonSerializer.Deserialize<DynamicPricingConfiguration>(JsonSerializer.Serialize(configuration));
        var basePrice = config?.BasePrice ?? listing.BasePrice ?? 0;

        // Get current market conditions
        var marketConditions = await _marketDataService.GetMarketConditionsAsync(
            listing.AssetType, listing.AssetSymbol, listing.TokenId, cancellationToken);

        // Calculate dynamic price multiplier based on market conditions
        var priceMultiplier = CalculateDynamicPriceMultiplier(marketConditions, config);
        
        // Apply min/max constraints
        var minMultiplier = config?.MinPriceMultiplier ?? 0.5m;
        var maxMultiplier = config?.MaxPriceMultiplier ?? 2.0m;
        priceMultiplier = Math.Max(minMultiplier, Math.Min(maxMultiplier, priceMultiplier));

        var pricePerToken = basePrice * priceMultiplier;
        var totalPrice = pricePerToken * quantity;

        var appliedFactors = GetAppliedPricingFactors(marketConditions);

        return new AdvancedPriceCalculationResult
        {
            PricePerToken = pricePerToken,
            TotalPrice = totalPrice,
            Quantity = quantity,
            Currency = listing.Currency,
            Strategy = PricingStrategy.Dynamic,
            CalculationDetails = $"Base price {basePrice:C} × {priceMultiplier:F3} multiplier = {pricePerToken:C}",
            IsValid = true,
            CalculatedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["BasePrice"] = basePrice,
                ["PriceMultiplier"] = priceMultiplier,
                ["AppliedFactors"] = appliedFactors,
                ["MarketConditions"] = marketConditions
            }
        };
    }

    private static decimal CalculateDynamicPriceMultiplier(MarketConditions marketConditions, DynamicPricingConfiguration? config)
    {
        var baseMultiplier = 1.0m;

        // Apply volatility factor (higher volatility = higher price)
        var volatilityFactor = 1 + (marketConditions.Volatility * 0.1m);
        
        // Apply volume factor (higher volume = slight price increase)
        var volumeFactor = 1 + (marketConditions.TradingVolume24h / 1000000m * 0.05m);
        
        // Apply price change factor (positive price change = higher price)
        var priceChangeFactor = 1 + (marketConditions.PriceChange24h * 0.5m);

        return baseMultiplier * volatilityFactor * volumeFactor * priceChangeFactor;
    }

    private static List<string> GetAppliedPricingFactors(MarketConditions marketConditions)
    {
        var factors = new List<string>();

        if (Math.Abs(marketConditions.Volatility) > 0.01m)
            factors.Add($"Volatility: {marketConditions.Volatility:P2}");

        if (marketConditions.TradingVolume24h > 0)
            factors.Add($"24h Volume: {marketConditions.TradingVolume24h:N0}");

        if (Math.Abs(marketConditions.PriceChange24h) > 0.01m)
            factors.Add($"24h Price Change: {marketConditions.PriceChange24h:P2}");

        if (marketConditions.ActiveOrders > 0)
            factors.Add($"Active Orders: {marketConditions.ActiveOrders}");

        return factors;
    }
}

/// <summary>
/// Unit pricing strategy handler - standard per-token pricing
/// </summary>
public class UnitPricingHandler : IPricingStrategyHandler
{
    private readonly ILogger _logger;

    public UnitPricingHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating unit price for listing {ListingId}", listing.Id);

        var pricePerToken = listing.BasePrice ?? 0;
        var totalPrice = pricePerToken * quantity;

        return new AdvancedPriceCalculationResult
        {
            PricePerToken = pricePerToken,
            TotalPrice = totalPrice,
            Quantity = quantity,
            Currency = listing.Currency,
            Strategy = PricingStrategy.Unit,
            CalculationDetails = $"Unit price: {pricePerToken:C} per token × {quantity:F8} tokens",
            IsValid = true,
            CalculatedAt = DateTime.UtcNow
        };
    }
}
