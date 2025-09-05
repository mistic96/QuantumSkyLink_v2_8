using MarketplaceService.Data.Entities;

namespace MarketplaceService.Models.Requests;

/// <summary>
/// Request model for creating a primary market listing
/// </summary>
public class CreatePrimaryMarketListingRequest
{
    public Guid TokenId { get; set; }
    public PricingStrategy PricingStrategy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal? MinimumPurchaseQuantity { get; set; }
    public decimal? MaximumPurchaseQuantity { get; set; }
    public decimal? BasePrice { get; set; }
    public string? Currency { get; set; }
    public string? PricingConfiguration { get; set; }
    public string? Metadata { get; set; }
    public decimal? CommissionPercentage { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Tags { get; set; }
    public string? ContactInfo { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? RoadmapUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
}

/// <summary>
/// Request model for updating a primary market listing
/// </summary>
public class UpdatePrimaryMarketListingRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public string? PricingConfiguration { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Tags { get; set; }
    public string? ContactInfo { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? RoadmapUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
}

/// <summary>
/// Request model for getting primary market listings with filters
/// </summary>
public class GetPrimaryMarketListingsRequest
{
    public ListingStatus? Status { get; set; }
    public PricingStrategy? PricingStrategy { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Tags { get; set; }
    public string? SearchTerm { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

/// <summary>
/// Request model for creating an order in the primary market
/// </summary>
public class CreatePrimaryMarketOrderRequest
{
    public decimal Quantity { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Metadata { get; set; }
}
