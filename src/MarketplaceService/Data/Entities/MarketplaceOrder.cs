using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceService.Data.Entities;

/// <summary>
/// Represents an order in the marketplace
/// </summary>
[Table("MarketplaceOrders")]
[Index(nameof(BuyerId))]
[Index(nameof(SellerId))]
[Index(nameof(ListingId))]
[Index(nameof(Status))]
[Index(nameof(TransactionType))]
[Index(nameof(CreatedAt))]
[Index(nameof(CompletedAt))]
[Index(nameof(BuyerId), nameof(ListingId), nameof(IdempotencyKey), IsUnique = true)]
public class MarketplaceOrder : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the order
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the marketplace listing this order is for
    /// </summary>
    [Required]
    [ForeignKey("MarketListing")]
    public Guid ListingId { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the buyer
    /// </summary>
    [Required]
    [ForeignKey("Buyer")]
    public Guid BuyerId { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the seller
    /// </summary>
    [Required]
    [ForeignKey("Seller")]
    public Guid SellerId { get; set; }

    /// <summary>
    /// Gets or sets the current status of the order
    /// </summary>
    [Required]
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the type of transaction
    /// </summary>
    [Required]
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Gets or sets the quantity of tokens being purchased
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(28,8)")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price per token at the time of order
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal PricePerToken { get; set; }

    /// <summary>
    /// Gets or sets the total amount for the order (before fees)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the currency for the order (ISO 4217 code)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the platform fee amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal PlatformFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the transaction fee amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal TransactionFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total fees for the order
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalFees { get; set; } = 0;

    /// <summary>
    /// Gets or sets the seller proceeds amount (for secondary market)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? SellerProceeds { get; set; }

    /// <summary>
    /// Gets or sets the final amount including all fees
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal FinalAmount { get; set; }

    /// <summary>
    /// Optional idempotency key to dedupe checkout requests
    /// </summary>
    [MaxLength(64)]
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Gets or sets the escrow account ID for this order
    /// </summary>
    [ForeignKey("EscrowAccount")]
    public Guid? EscrowAccountId { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway transaction ID
    /// </summary>
    [MaxLength(255)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the blockchain transaction hash (for crypto payments)
    /// </summary>
    [MaxLength(255)]
    public string? BlockchainTransactionHash { get; set; }

    /// <summary>
    /// Gets or sets additional order metadata (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the pricing details used for this order (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string PricingDetails { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the buyer's notes or comments
    /// </summary>
    [MaxLength(1000)]
    public string? BuyerNotes { get; set; }

    /// <summary>
    /// Gets or sets the seller's notes or comments
    /// </summary>
    [MaxLength(1000)]
    public string? SellerNotes { get; set; }

    /// <summary>
    /// Gets or sets the cancellation reason (if cancelled)
    /// </summary>
    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Gets or sets the failure reason (if failed)
    /// </summary>
    [MaxLength(500)]
    public string? FailureReason { get; set; }

    /// <summary>
    /// Gets or sets the client IP address for security tracking
    /// </summary>
    [MaxLength(45)]
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent for security tracking
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets when the order was confirmed
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Gets or sets when the payment was received
    /// </summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Gets or sets when the order was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets when the order was cancelled
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets when the order expires (for pending orders)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the estimated completion time
    /// </summary>
    public DateTime? EstimatedCompletionAt { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts for failed orders
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts allowed
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether this order requires manual review
    /// </summary>
    public bool RequiresManualReview { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this order has been reviewed by an admin
    /// </summary>
    public bool IsReviewed { get; set; } = false;

    /// <summary>
    /// Gets or sets the admin user ID who reviewed this order
    /// </summary>
    [ForeignKey("ReviewedByAdmin")]
    public Guid? ReviewedByAdminId { get; set; }

    /// <summary>
    /// Gets or sets when the order was reviewed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Gets or sets the review notes from admin
    /// </summary>
    [MaxLength(1000)]
    public string? ReviewNotes { get; set; }

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
    /// Gets or sets the marketplace listing for this order
    /// </summary>
    public MarketListing MarketListing { get; set; } = null!;

    /// <summary>
    /// Gets or sets the escrow account for this order
    /// </summary>
    public EscrowAccount? EscrowAccount { get; set; }

    /// <summary>
    /// Gets or sets the collection of order history records
    /// </summary>
    public ICollection<OrderHistory> OrderHistory { get; set; } = new List<OrderHistory>();
}
