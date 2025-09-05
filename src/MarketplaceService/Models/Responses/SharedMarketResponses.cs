using MarketplaceService.Data.Entities;
using MarketplaceService.Models.Requests;

namespace MarketplaceService.Models.Responses;

/// <summary>
/// Response model for marketplace search results
/// </summary>
public class MarketplaceSearchResponse
{
    public Guid Id { get; set; }
    public string AssetIdentifier { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public MarketType MarketType { get; set; }
    public PricingStrategy PricingStrategy { get; set; }
    public ListingStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int OrderCount { get; set; }
    public decimal? LastSalePrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsSoldOut { get; set; }
}

/// <summary>
/// Response model for marketplace analytics
/// </summary>
public class MarketplaceAnalyticsResponse
{
    public MarketType? MarketType { get; set; }
    public AssetType? AssetType { get; set; }
    public string? AssetIdentifier { get; set; }
    public string TimePeriod { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Overall Statistics
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalFees { get; set; }
    
    // Price Statistics
    public decimal? AveragePrice { get; set; }
    public decimal? MedianPrice { get; set; }
    public decimal? HighestPrice { get; set; }
    public decimal? LowestPrice { get; set; }
    public decimal? PriceChange { get; set; }
    public decimal? PriceChangePercentage { get; set; }
    
    // Volume Statistics
    public decimal VolumeChange { get; set; }
    public decimal VolumeChangePercentage { get; set; }
    public int OrderCountChange { get; set; }
    public decimal OrderCountChangePercentage { get; set; }
    
    // Historical Data
    public List<PricePoint> PriceHistory { get; set; } = new();
    public List<VolumePoint> VolumeHistory { get; set; } = new();
    public List<OrderBookEntry> OrderBook { get; set; } = new();
    
    // Top Performers
    public List<TopAssetResponse> TopAssetsByVolume { get; set; } = new();
    public List<TopAssetResponse> TopAssetsByPrice { get; set; } = new();
    public List<TopSellerResponse> TopSellers { get; set; } = new();
    
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Response model for top performing assets
/// </summary>
public class TopAssetResponse
{
    public string AssetIdentifier { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public decimal Price { get; set; }
    public decimal PriceChange { get; set; }
    public decimal PriceChangePercentage { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Response model for top sellers
/// </summary>
public class TopSellerResponse
{
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ListingCount { get; set; }
    public decimal TotalVolume { get; set; }
    public int CompletedOrders { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
}


/// <summary>
/// Fee breakdown for pricing calculations
/// </summary>
public class PricingFeeBreakdown
{
    public decimal PlatformFee { get; set; }
    public decimal PlatformFeePercentage { get; set; }
    public decimal TransactionFee { get; set; }
    public decimal TransactionFeePercentage { get; set; }
    public decimal EscrowFee { get; set; }
    public decimal EscrowFeePercentage { get; set; }
    public decimal TotalFees { get; set; }
    public decimal NetAmount { get; set; }
}

/// <summary>
/// Escrow details for pricing
/// </summary>
public class EscrowDetailsResponse
{
    public decimal EscrowAmount { get; set; }
    public decimal EscrowFee { get; set; }
    public int EscrowDurationDays { get; set; }
    public DateTime? EscrowExpiryDate { get; set; }
    public string EscrowTerms { get; set; } = string.Empty;
}

/// <summary>
/// Pricing tier for tiered pricing strategy
/// </summary>
public class PricingTier
{
    public decimal MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal DiscountPercentage { get; set; }
}

/// <summary>
/// Response model for user marketplace activity
/// </summary>
public class UserMarketplaceActivityResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public MarketplaceActivityType ActivityType { get; set; }
    public string ActivityDescription { get; set; } = string.Empty;
    public MarketType? MarketType { get; set; }
    public Guid? ListingId { get; set; }
    public string? ListingTitle { get; set; }
    public Guid? OrderId { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? AssetIdentifier { get; set; }
    public AssetType? AssetType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Response model for escrow management operations
/// </summary>
public class EscrowManagementResponse
{
    public Guid OrderId { get; set; }
    public Guid? EscrowAccountId { get; set; }
    public EscrowStatus EscrowStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal? EscrowAmount { get; set; }
    public string? Currency { get; set; }
    public DateTime? EscrowCreatedAt { get; set; }
    public DateTime? EscrowExpiryDate { get; set; }
    public DateTime ActionPerformedAt { get; set; }
    public string? TransactionHash { get; set; }
}


/// <summary>
/// Order status history entry
/// </summary>
public class OrderStatusHistory
{
    public OrderStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Notes { get; set; }
    public Guid? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
}

/// <summary>
/// Response model for reporting operations
/// </summary>
public class ReportResponse
{
    public Guid ReportId { get; set; }
    public Guid ListingId { get; set; }
    public Guid ReportedBy { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReportedAt { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response model for marketplace statistics summary
/// </summary>
public class MarketplaceStatsResponse
{
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal Volume24h { get; set; }
    public decimal VolumeChange24h { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers24h { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalFeesCollected { get; set; }
    public DateTime LastUpdated { get; set; }
}
