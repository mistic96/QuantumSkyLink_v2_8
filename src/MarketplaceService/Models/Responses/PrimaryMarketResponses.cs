using MarketplaceService.Data.Entities;

namespace MarketplaceService.Models.Responses;

/// <summary>
/// Response model for primary market listing details
/// </summary>
public class PrimaryMarketListingResponse
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid? TokenId { get; set; }
    public AssetType AssetType { get; set; }
    public MarketType MarketType { get; set; }
    public ListingStatus Status { get; set; }
    public PricingStrategy PricingStrategy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal MinimumPurchaseQuantity { get; set; }
    public decimal? MaximumPurchaseQuantity { get; set; }
    public decimal? BasePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PricingConfiguration { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
    public decimal CommissionPercentage { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public long ViewCount { get; set; }
    public int OrderCount { get; set; }
    public decimal VolumeSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public string? Tags { get; set; }
    public string? ContactInfo { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? RoadmapUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Indicates if the listing belongs to the current authenticated user (populated by controller)
    public bool IsOwnListing { get; set; }
}

/// <summary>
/// Response model for marketplace order details
/// </summary>
public class MarketplaceOrderResponse
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public OrderStatus Status { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerToken { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? PaymentTransactionId { get; set; }
    public string Metadata { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response model for pricing calculations
/// </summary>
public class PricingResponse
{
    public decimal PricePerToken { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal Fees { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PricingStrategy { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
}
