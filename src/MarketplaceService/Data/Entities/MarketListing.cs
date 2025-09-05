using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceService.Data.Entities;

/// <summary>
/// Represents a marketplace listing for buying and selling tokens
/// </summary>
[Table("MarketListings")]
[Index(nameof(SellerId))]
[Index(nameof(TokenId))]
[Index(nameof(Status))]
[Index(nameof(MarketType))]
[Index(nameof(AssetType))]
[Index(nameof(PricingStrategy))]
[Index(nameof(CreatedAt))]
[Index(nameof(ExpiresAt))]
public class MarketListing : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the listing
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the seller
    /// </summary>
    [Required]
    [ForeignKey("Seller")]
    public Guid SellerId { get; set; }

    /// <summary>
    /// Gets or sets the token ID being sold (for platform tokens)
    /// </summary>
    [ForeignKey("Token")]
    public Guid? TokenId { get; set; }

    /// <summary>
    /// Gets or sets the asset symbol (e.g., BTC, ETH, SOL for external assets)
    /// </summary>
    [MaxLength(10)]
    public string? AssetSymbol { get; set; }

    /// <summary>
    /// Gets or sets the type of asset being traded
    /// </summary>
    [Required]
    public AssetType AssetType { get; set; }

    /// <summary>
    /// Gets or sets the type of market (Primary or Secondary)
    /// </summary>
    [Required]
    public MarketType MarketType { get; set; }

    /// <summary>
    /// Gets or sets the current status of the listing
    /// </summary>
    [Required]
    public ListingStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the pricing strategy for this listing
    /// </summary>
    [Required]
    public PricingStrategy PricingStrategy { get; set; }

    /// <summary>
    /// Gets or sets the title of the listing
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the listing
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the total quantity of tokens available for sale
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(28,8)")]
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// Gets or sets the remaining quantity available for sale
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(28,8)")]
    public decimal RemainingQuantity { get; set; }

    /// <summary>
    /// Gets or sets the minimum purchase quantity
    /// </summary>
    [Column(TypeName = "decimal(28,8)")]
    public decimal MinimumPurchaseQuantity { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum purchase quantity (null for unlimited)
    /// </summary>
    [Column(TypeName = "decimal(28,8)")]
    public decimal? MaximumPurchaseQuantity { get; set; }

    /// <summary>
    /// Gets or sets the base price per token (used for fixed pricing)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? BasePrice { get; set; }

    /// <summary>
    /// Gets or sets the currency for pricing (ISO 4217 code)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the pricing configuration as JSON (for complex pricing strategies)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string PricingConfiguration { get; set; } = "{}";

    /// <summary>
    /// Gets or sets additional metadata for the listing (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the listing fee amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal ListingFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the commission percentage for sales
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal CommissionPercentage { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether this is a featured listing
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this listing is verified
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Gets or sets the listing expiration date (null for no expiration)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets when the listing was last activated
    /// </summary>
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the listing was completed/sold out
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the total number of views for this listing
    /// </summary>
    public long ViewCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total number of orders placed for this listing
    /// </summary>
    public int OrderCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total volume sold from this listing
    /// </summary>
    [Column(TypeName = "decimal(28,8)")]
    public decimal VolumeSold { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total revenue generated from this listing
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalRevenue { get; set; } = 0;

    /// <summary>
    /// Gets or sets additional tags for categorization and search
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the contact information for support (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? ContactInfo { get; set; }

    /// <summary>
    /// Gets or sets the product documentation URL
    /// </summary>
    [MaxLength(500)]
    public string? DocumentationUrl { get; set; }

    /// <summary>
    /// Gets or sets the product roadmap URL
    /// </summary>
    [MaxLength(500)]
    public string? RoadmapUrl { get; set; }

    /// <summary>
    /// Gets or sets the whitepaper URL
    /// </summary>
    [MaxLength(500)]
    public string? WhitepaperUrl { get; set; }

    /// <summary>
    /// Gets or sets the website URL
    /// </summary>
    [MaxLength(500)]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets social media links (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? SocialLinks { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Gets or sets the collection of orders for this listing
    /// </summary>
    public ICollection<MarketplaceOrder> Orders { get; set; } = new List<MarketplaceOrder>();

    /// <summary>
    /// Gets or sets the collection of price history records for this listing
    /// </summary>
    public ICollection<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
}
