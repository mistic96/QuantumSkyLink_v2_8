using MarketplaceService.Data.Entities;

namespace MarketplaceService.Models.Responses;

/// <summary>
/// Response model for secondary market listing details
/// </summary>
public class SecondaryMarketListingResponse
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
    public decimal SoldQuantity { get; set; }
    public decimal MinimumPurchaseQuantity { get; set; }
    public decimal? MaximumPurchaseQuantity { get; set; }
    public decimal BasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PricingConfiguration { get; set; }
    public string? TradingPair { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ContactInfo { get; set; }
    public string? Metadata { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int OrderCount { get; set; }
    public decimal? LastSalePrice { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsSoldOut { get; set; }
}

/// <summary>
/// Response model for trading pair information
/// </summary>
public class TradingPairResponse
{
    public Guid Id { get; set; }
    public string BaseAsset { get; set; } = string.Empty;
    public string QuoteAsset { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal? MinTradeAmount { get; set; }
    public decimal? MaxTradeAmount { get; set; }
    public decimal? TradingFeePercentage { get; set; }
    public bool IsActive { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? PriceChange24h { get; set; }
    public decimal? PriceChangePercentage24h { get; set; }
    public decimal? Volume24h { get; set; }
    public decimal? High24h { get; set; }
    public decimal? Low24h { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response model for secondary market order details
/// </summary>
public class SecondaryMarketOrderResponse
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalFees { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? EscrowAccountId { get; set; }
    public EscrowStatus? EscrowStatus { get; set; }
    public string? Notes { get; set; }
    public bool CanCancel { get; set; }
    public bool CanRefund { get; set; }
}

/// <summary>
/// Response model for secondary market analytics
/// </summary>
public class SecondaryMarketAnalyticsResponse
{
    public string AssetIdentifier { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string? TradingPair { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal? PriceChange24h { get; set; }
    public decimal? PriceChangePercentage24h { get; set; }
    public decimal Volume24h { get; set; }
    public decimal? High24h { get; set; }
    public decimal? Low24h { get; set; }
    public int ActiveListings { get; set; }
    public int TotalOrders24h { get; set; }
    public decimal TotalVolume24h { get; set; }
    public decimal AverageOrderSize { get; set; }
    public List<PricePoint> PriceHistory { get; set; } = new();
    public List<VolumePoint> VolumeHistory { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Price point for historical data
/// </summary>
public class PricePoint
{
    public DateTime Timestamp { get; set; }
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
}

/// <summary>
/// Volume point for historical data
/// </summary>
public class VolumePoint
{
    public DateTime Timestamp { get; set; }
    public decimal Volume { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Response model for market depth information
/// </summary>
public class MarketDepthResponse
{
    public string AssetIdentifier { get; set; } = string.Empty;
    public string? TradingPair { get; set; }
    public List<OrderBookEntry> Bids { get; set; } = new();
    public List<OrderBookEntry> Asks { get; set; } = new();
    public decimal? BestBid { get; set; }
    public decimal? BestAsk { get; set; }
    public decimal? Spread { get; set; }
    public decimal? SpreadPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Order book entry for market depth
/// </summary>
public class OrderBookEntry
{
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal Total { get; set; }
    public int OrderCount { get; set; }
}

/// <summary>
/// Response model for asset information in secondary market
/// </summary>
public class SecondaryMarketAssetResponse
{
    public string AssetIdentifier { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? CurrentPrice { get; set; }
    public string? Currency { get; set; }
    public decimal? MarketCap { get; set; }
    public decimal? CirculatingSupply { get; set; }
    public decimal? TotalSupply { get; set; }
    public int ActiveListings { get; set; }
    public decimal Volume24h { get; set; }
    public decimal? PriceChange24h { get; set; }
    public decimal? PriceChangePercentage24h { get; set; }
    public List<string> SupportedTradingPairs { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
}
