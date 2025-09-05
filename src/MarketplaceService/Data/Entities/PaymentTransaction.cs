using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketplaceService.Data.Entities;

/// <summary>
/// Represents a payment transaction in the marketplace
/// </summary>
[Table("PaymentTransactions")]
public class PaymentTransaction
{
    /// <summary>
    /// Unique identifier for the payment transaction
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The marketplace listing ID this transaction is for
    /// </summary>
    [Required]
    public Guid ListingId { get; set; }

    /// <summary>
    /// The order ID this transaction is associated with
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// The buyer's user ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string BuyerId { get; set; } = string.Empty;

    /// <summary>
    /// The seller's user ID
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SellerId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment method used
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status
    /// </summary>
    [Required]
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// External transaction ID from payment provider
    /// </summary>
    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Transaction hash for blockchain payments
    /// </summary>
    [MaxLength(100)]
    public string? BlockchainTxHash { get; set; }

    /// <summary>
    /// Fee amount charged
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal FeeAmount { get; set; }

    /// <summary>
    /// Net amount after fees
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Error message if transaction failed
    /// </summary>
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata in JSON format
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// When the transaction was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the transaction was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the transaction was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Payment transaction status enumeration
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending
    /// </summary>
    Pending,

    /// <summary>
    /// Payment is being processed
    /// </summary>
    Processing,

    /// <summary>
    /// Payment completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed,

    /// <summary>
    /// Payment was cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Payment was refunded
    /// </summary>
    Refunded,

    /// <summary>
    /// Payment is in escrow
    /// </summary>
    InEscrow,

    /// <summary>
    /// Payment released from escrow
    /// </summary>
    Released
}
