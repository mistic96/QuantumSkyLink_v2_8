using System.ComponentModel.DataAnnotations;
using MarketplaceService.Data.Entities;
using MarketplaceService.Models.Shared;

namespace MarketplaceService.Models.Requests;

/// <summary>
/// Request to create a secondary market listing for existing tokens
/// </summary>
public class CreateSecondaryMarketListingRequest
{
    /// <summary>
    /// Token ID for platform tokens, or asset symbol for external crypto
    /// </summary>
    [Required]
    public string AssetIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Type of asset being listed
    /// </summary>
    [Required]
    public AssetType AssetType { get; set; }

    /// <summary>
    /// Pricing strategy for the listing
    /// </summary>
    [Required]
    public PricingStrategy PricingStrategy { get; set; }

    /// <summary>
    /// Title for the listing
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the listing
    /// </summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Total quantity available for sale
    /// </summary>
    [Required]
    [Range(0.000001, double.MaxValue)]
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// Minimum purchase quantity per order
    /// </summary>
    [Range(0.000001, double.MaxValue)]
    public decimal? MinimumPurchaseQuantity { get; set; }

    /// <summary>
    /// Maximum purchase quantity per order
    /// </summary>
    [Range(0.000001, double.MaxValue)]
    public decimal? MaximumPurchaseQuantity { get; set; }

    /// <summary>
    /// Base price per unit
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Currency for pricing (USD, BTC, ETH, etc.)
    /// </summary>
    [StringLength(10)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// JSON configuration for pricing strategy
    /// </summary>
    public string? PricingConfiguration { get; set; }

    /// <summary>
    /// Trading pair for crypto assets (e.g., BTC/USD, ETH/BTC)
    /// </summary>
    [StringLength(20)]
    public string? TradingPair { get; set; }

    /// <summary>
    /// Tags for categorization and search
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Expiration date for the listing
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Seller contact information
    /// </summary>
    [StringLength(500)]
    public string? ContactInfo { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Request to update a secondary market listing
/// </summary>
public class UpdateSecondaryMarketListingRequest
{
    /// <summary>
    /// Updated title for the listing
    /// </summary>
    [StringLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Updated description
    /// </summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Updated base price per unit
    /// </summary>
    [Range(0.01, double.MaxValue)]
    public decimal? BasePrice { get; set; }

    /// <summary>
    /// Updated pricing configuration
    /// </summary>
    public string? PricingConfiguration { get; set; }

    /// <summary>
    /// Updated expiration date
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Updated tags
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Updated contact information
    /// </summary>
    [StringLength(500)]
    public string? ContactInfo { get; set; }

    /// <summary>
    /// Updated metadata
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Request to get secondary market listings with filtering
/// </summary>
public class GetSecondaryMarketListingsRequest
{
    /// <summary>
    /// Asset type filter
    /// </summary>
    public AssetType? AssetType { get; set; }

    /// <summary>
    /// Asset identifier filter (token ID or symbol)
    /// </summary>
    public string? AssetIdentifier { get; set; }

    /// <summary>
    /// Trading pair filter
    /// </summary>
    public string? TradingPair { get; set; }

    /// <summary>
    /// Pricing strategy filter
    /// </summary>
    public PricingStrategy? PricingStrategy { get; set; }

    /// <summary>
    /// Minimum price filter
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Currency filter
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Tags filter (comma-separated)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Search query for title/description
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Sort field
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "desc";

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? Page { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    [Range(1, 100)]
    public int? PageSize { get; set; } = 20;
}

/// <summary>
/// Request to create an order for a secondary market listing
/// </summary>
public class CreateSecondaryMarketOrderRequest
{
    /// <summary>
    /// Quantity to purchase
    /// </summary>
    [Required]
    [Range(0.000001, double.MaxValue)]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Optional buyer notes or metadata
    /// </summary>
    [StringLength(1000)]
    public string? Metadata { get; set; }

    /// <summary>
    /// Preferred payment method
    /// </summary>
    [StringLength(50)]
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// Request to create a trading pair
/// </summary>
public class CreateTradingPairRequest
{
    /// <summary>
    /// Base asset (what you're selling)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string BaseAsset { get; set; } = string.Empty;

    /// <summary>
    /// Quote asset (what you're buying with)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string QuoteAsset { get; set; } = string.Empty;

    /// <summary>
    /// Minimum trade amount
    /// </summary>
    [Range(0.000001, double.MaxValue)]
    public decimal? MinTradeAmount { get; set; }

    /// <summary>
    /// Maximum trade amount
    /// </summary>
    [Range(0.000001, double.MaxValue)]
    public decimal? MaxTradeAmount { get; set; }

    /// <summary>
    /// Trading fees percentage
    /// </summary>
    [Range(0, 100)]
    public decimal? TradingFeePercentage { get; set; }

    /// <summary>
    /// Whether the trading pair is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
