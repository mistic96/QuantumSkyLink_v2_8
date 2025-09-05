using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceService.Data.Entities;

/// <summary>
/// Represents price history for marketplace listings
/// </summary>
[Table("PriceHistory")]
[Index(nameof(ListingId))]
[Index(nameof(CreatedAt))]
[Index(nameof(PricingStrategy))]
public class PriceHistory : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the price history record
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the marketplace listing this price history is for
    /// </summary>
    [Required]
    [ForeignKey("MarketListing")]
    public Guid ListingId { get; set; }

    /// <summary>
    /// Gets or sets the pricing strategy used at this time
    /// </summary>
    [Required]
    public PricingStrategy PricingStrategy { get; set; }

    /// <summary>
    /// Gets or sets the price per token at this point in time
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal PricePerToken { get; set; }

    /// <summary>
    /// Gets or sets the currency for the price (ISO 4217 code)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the market price used for margin calculations (if applicable)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MarketPrice { get; set; }

    /// <summary>
    /// Gets or sets the margin amount applied (if applicable)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MarginAmount { get; set; }

    /// <summary>
    /// Gets or sets the margin percentage applied (if applicable)
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? MarginPercentage { get; set; }

    /// <summary>
    /// Gets or sets the pricing configuration used (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string PricingConfiguration { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the reason for the price change
    /// </summary>
    [MaxLength(500)]
    public string? ChangeReason { get; set; }

    /// <summary>
    /// Gets or sets whether this was an automatic price update
    /// </summary>
    public bool IsAutomaticUpdate { get; set; } = false;

    /// <summary>
    /// Gets or sets the user ID who triggered the price change (if manual)
    /// </summary>
    [ForeignKey("UpdatedByUser")]
    public Guid? UpdatedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the trading volume at the time of this price
    /// </summary>
    [Column(TypeName = "decimal(28,8)")]
    public decimal? TradingVolume { get; set; }

    /// <summary>
    /// Gets or sets the number of active orders at this price
    /// </summary>
    public int? ActiveOrders { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the price record (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

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
    /// Gets or sets the marketplace listing for this price history
    /// </summary>
    public MarketListing MarketListing { get; set; } = null!;
}
