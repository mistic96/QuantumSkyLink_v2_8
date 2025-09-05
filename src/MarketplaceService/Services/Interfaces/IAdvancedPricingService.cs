using MarketplaceService.Data.Entities;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Advanced pricing service interface for sophisticated pricing strategies
/// </summary>
public interface IAdvancedPricingService
{
    /// <summary>
    /// Calculate price using advanced pricing strategies with comprehensive business logic
    /// </summary>
    Task<AdvancedPriceCalculationResult> CalculateAdvancedPriceAsync(
        Guid listingId, 
        decimal quantity, 
        Guid? buyerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate bulk pricing requirements - all-or-nothing purchase logic
    /// </summary>
    Task<BulkPricingValidationResult> ValidateBulkPurchaseAsync(
        Guid listingId, 
        decimal requestedQuantity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tiered pricing breakdown with detailed tier information
    /// </summary>
    Task<TieredPricingBreakdownResult> GetTieredPricingBreakdownAsync(
        Guid listingId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update margin-based pricing with enhanced market data integration
    /// </summary>
    Task<MarginPricingUpdateResult> UpdateMarginBasedPricingAsync(
        Guid listingId,
        bool forceUpdate = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update dynamic pricing based on comprehensive market conditions
    /// </summary>
    Task<DynamicPricingUpdateResult> UpdateDynamicPricingAsync(
        Guid listingId,
        MarketConditions? marketConditions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comprehensive pricing analytics for a listing
    /// </summary>
    Task<PricingAnalyticsResult> GetPricingAnalyticsAsync(
        Guid listingId,
        TimeSpan? period = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Market data service interface for external price feeds and market conditions
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// Get current market price for an asset
    /// </summary>
    Task<MarketPriceResult> GetMarketPriceAsync(
        AssetType assetType, 
        string? assetSymbol = null, 
        Guid? tokenId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comprehensive market conditions for dynamic pricing
    /// </summary>
    Task<MarketConditions> GetMarketConditionsAsync(
        AssetType assetType, 
        string? assetSymbol = null, 
        Guid? tokenId = null, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Pricing strategy handler interface for strategy pattern implementation
/// </summary>
public interface IPricingStrategyHandler
{
    /// <summary>
    /// Calculate price using specific pricing strategy
    /// </summary>
    Task<AdvancedPriceCalculationResult> CalculatePriceAsync(
        MarketListing listing,
        decimal quantity,
        Dictionary<string, object> configuration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Advanced price calculation result with comprehensive details
/// </summary>
public class AdvancedPriceCalculationResult
{
    public decimal PricePerToken { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Currency { get; set; } = "USD";
    public PricingStrategy Strategy { get; set; }
    public string? CalculationDetails { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CalculatedAt { get; set; }
    public List<PricingTierDetail> AppliedTiers { get; set; } = new();
    public List<AppliedDiscount> AppliedDiscounts { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Bulk pricing validation result
/// </summary>
public class BulkPricingValidationResult
{
    public bool IsValid { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? ErrorMessage { get; set; }
    public DateTime ValidatedAt { get; set; }
}

/// <summary>
/// Tiered pricing breakdown result
/// </summary>
public class TieredPricingBreakdownResult
{
    public Guid ListingId { get; set; }
    public List<PricingTierDetail> Tiers { get; set; } = new();
    public string Currency { get; set; } = "USD";
    public decimal RemainingQuantity { get; set; }
    public DateTime RetrievedAt { get; set; }
}

/// <summary>
/// Margin pricing update result
/// </summary>
public class MarginPricingUpdateResult
{
    public Guid ListingId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal MarketPrice { get; set; }
    public PricingStrategy MarginType { get; set; }
    public decimal MarginValue { get; set; }
    public string Currency { get; set; } = "USD";
    public bool WasUpdated { get; set; }
    public string? UpdateReason { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? MarketDataSource { get; set; }
}

/// <summary>
/// Dynamic pricing update result
/// </summary>
public class DynamicPricingUpdateResult
{
    public Guid ListingId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PriceMultiplier { get; set; }
    public string Currency { get; set; } = "USD";
    public bool WasUpdated { get; set; }
    public MarketConditions MarketConditions { get; set; } = new();
    public List<string> AppliedFactors { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Pricing analytics result
/// </summary>
public class PricingAnalyticsResult
{
    public Guid ListingId { get; set; }
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public PricingStrategy PricingStrategy { get; set; }
    public TimeSpan AnalysisPeriod { get; set; }
    public List<PriceHistoryPoint> PriceHistory { get; set; } = new();
    public PricingStatistics Statistics { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Detailed pricing tier information
/// </summary>
public class PricingTierDetail
{
    public decimal MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public decimal PricePerToken { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? DiscountPercentage { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Applied discount information
/// </summary>
public class AppliedDiscount
{
    public string Type { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Price history point for analytics
/// </summary>
public class PriceHistoryPoint
{
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ChangeReason { get; set; }
    public bool IsAutomatic { get; set; }
}

/// <summary>
/// Pricing statistics for analytics
/// </summary>
public class PricingStatistics
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public int PriceChanges { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderSize { get; set; }
}

/// <summary>
/// Pricing configuration models
/// </summary>
public class BulkPricingConfiguration
{
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public bool AllOrNothing { get; set; } = true;
}

public class MarginPricingConfiguration
{
    public decimal? FixedMargin { get; set; }
    public decimal? PercentageMargin { get; set; }
    public string Currency { get; set; } = "USD";
    public string? MarketDataSource { get; set; }
    public int UpdateIntervalMinutes { get; set; } = 15;
}

public class TieredPricingConfiguration
{
    public List<PricingTier> Tiers { get; set; } = new();
    public string Currency { get; set; } = "USD";
}

public class DynamicPricingConfiguration
{
    public decimal BasePrice { get; set; }
    public decimal MaxPriceMultiplier { get; set; } = 2.0m;
    public decimal MinPriceMultiplier { get; set; } = 0.5m;
    public string Currency { get; set; } = "USD";
    public List<string> Factors { get; set; } = new();
    public int UpdateIntervalMinutes { get; set; } = 5;
}
