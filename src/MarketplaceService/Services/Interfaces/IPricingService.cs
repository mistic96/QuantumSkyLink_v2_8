using MarketplaceService.Data.Entities;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Service interface for managing pricing strategies and calculations
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculate price for a given quantity using the listing's pricing strategy
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="quantity">Quantity to purchase</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price calculation result</returns>
    Task<PriceCalculationResult> CalculatePriceAsync(Guid listingId, decimal quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update pricing for margin-based listings based on current market prices
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated price information</returns>
    Task<PriceUpdateResult> UpdateMarginBasedPricingAsync(Guid listingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current market price for an asset
    /// </summary>
    /// <param name="assetType">Asset type</param>
    /// <param name="assetSymbol">Asset symbol (for external assets)</param>
    /// <param name="tokenId">Token ID (for platform tokens)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current market price</returns>
    Task<MarketPriceResult> GetMarketPriceAsync(AssetType assetType, string? assetSymbol = null, Guid? tokenId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate pricing configuration for a specific strategy
    /// </summary>
    /// <param name="strategy">Pricing strategy</param>
    /// <param name="configuration">Pricing configuration JSON</param>
    /// <param name="basePrice">Base price (if applicable)</param>
    /// <returns>Validation result</returns>
    Task<PricingValidationResult> ValidatePricingConfigurationAsync(PricingStrategy strategy, string configuration, decimal? basePrice = null);

    /// <summary>
    /// Get pricing tiers for tiered pricing strategy
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pricing tiers</returns>
    Task<IEnumerable<PricingTier>> GetPricingTiersAsync(Guid listingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update dynamic pricing based on market conditions
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="marketConditions">Current market conditions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated price information</returns>
    Task<PriceUpdateResult> UpdateDynamicPricingAsync(Guid listingId, MarketConditions marketConditions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all listings that need price updates
    /// </summary>
    /// <param name="strategy">Optional pricing strategy filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of listings needing price updates</returns>
    Task<IEnumerable<Guid>> GetListingsNeedingPriceUpdateAsync(PricingStrategy? strategy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update prices for multiple listings
    /// </summary>
    /// <param name="listingIds">Collection of listing IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk update results</returns>
    Task<BulkPriceUpdateResult> BulkUpdatePricesAsync(IEnumerable<Guid> listingIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create price history record
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="pricePerToken">Price per token</param>
    /// <param name="reason">Reason for price change</param>
    /// <param name="isAutomatic">Whether this was an automatic update</param>
    /// <param name="userId">User ID (if manual update)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created price history record</returns>
    Task<PriceHistoryDto> CreatePriceHistoryAsync(Guid listingId, decimal pricePerToken, string? reason = null, bool isAutomatic = false, Guid? userId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer objects for pricing service
/// </summary>
public class PriceCalculationResult
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
    public IEnumerable<PricingTier>? AppliedTiers { get; set; }
}

public class PriceUpdateResult
{
    public Guid ListingId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? UpdateReason { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UpdatedAt { get; set; }
    public MarketPriceResult? MarketPrice { get; set; }
}

public class MarketPriceResult
{
    public AssetType AssetType { get; set; }
    public string? AssetSymbol { get; set; }
    public Guid? TokenId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Source { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PricingValidationResult
{
    public bool IsValid { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
    public string? ParsedConfiguration { get; set; }
}

public class PricingTier
{
    public decimal MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public decimal PricePerToken { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? DiscountPercentage { get; set; }
    public string? Description { get; set; }
}

public class MarketConditions
{
    public decimal TradingVolume24h { get; set; }
    public decimal PriceChange24h { get; set; }
    public int ActiveOrders { get; set; }
    public decimal AverageOrderSize { get; set; }
    public decimal MarketCap { get; set; }
    public decimal Volatility { get; set; }
    public DateTime Timestamp { get; set; }
}

public class BulkPriceUpdateResult
{
    public int TotalListings { get; set; }
    public int SuccessfulUpdates { get; set; }
    public int FailedUpdates { get; set; }
    public IEnumerable<PriceUpdateResult> Results { get; set; } = new List<PriceUpdateResult>();
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Pricing strategy configurations
/// </summary>
public static class PricingConfigurations
{
    public class FixedPricingConfig
    {
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class BulkPricingConfig
    {
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public bool AllOrNothing { get; set; } = true;
    }

    public class MarginPricingConfig
    {
        public decimal? FixedMargin { get; set; }
        public decimal? PercentageMargin { get; set; }
        public string Currency { get; set; } = "USD";
        public string? MarketDataSource { get; set; }
        public int UpdateIntervalMinutes { get; set; } = 15;
    }

    public class TieredPricingConfig
    {
        public IEnumerable<PricingTier> Tiers { get; set; } = new List<PricingTier>();
        public string Currency { get; set; } = "USD";
    }

    public class DynamicPricingConfig
    {
        public decimal BasePrice { get; set; }
        public decimal MaxPriceMultiplier { get; set; } = 2.0m;
        public decimal MinPriceMultiplier { get; set; } = 0.5m;
        public string Currency { get; set; } = "USD";
        public IEnumerable<string> Factors { get; set; } = new List<string>();
        public int UpdateIntervalMinutes { get; set; } = 5;
    }
}
