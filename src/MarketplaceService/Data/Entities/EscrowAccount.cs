using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceService.Data.Entities;

/// <summary>
/// Represents an escrow account for secure marketplace transactions
/// </summary>
[Table("EscrowAccounts")]
[Index(nameof(BuyerId))]
[Index(nameof(SellerId))]
[Index(nameof(OrderId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
[Index(nameof(ExpiresAt))]
public class EscrowAccount : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the escrow account
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the marketplace order this escrow is for
    /// </summary>
    [Required]
    [ForeignKey("MarketplaceOrder")]
    public Guid OrderId { get; set; }

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
    /// Gets or sets the current status of the escrow
    /// </summary>
    [Required]
    public EscrowStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the total amount held in escrow
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal EscrowAmount { get; set; }

    /// <summary>
    /// Gets or sets the currency for the escrow (ISO 4217 code)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the quantity of tokens being held in escrow
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(28,8)")]
    public decimal TokenQuantity { get; set; }

    /// <summary>
    /// Gets or sets the token ID being held (for platform tokens)
    /// </summary>
    [ForeignKey("Token")]
    public Guid? TokenId { get; set; }

    /// <summary>
    /// Gets or sets the asset symbol (for external crypto assets)
    /// </summary>
    [MaxLength(10)]
    public string? AssetSymbol { get; set; }

    /// <summary>
    /// Gets or sets the type of asset being held in escrow
    /// </summary>
    [Required]
    public AssetType AssetType { get; set; }

    /// <summary>
    /// Gets or sets the external wallet address for crypto assets
    /// </summary>
    [MaxLength(255)]
    public string? WalletAddress { get; set; }

    /// <summary>
    /// Gets or sets the blockchain transaction hash for asset locking
    /// </summary>
    [MaxLength(255)]
    public string? LockTransactionHash { get; set; }

    /// <summary>
    /// Gets or sets the blockchain transaction hash for asset release
    /// </summary>
    [MaxLength(255)]
    public string? ReleaseTransactionHash { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway transaction ID for funds
    /// </summary>
    [MaxLength(255)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the escrow conditions that must be met (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string EscrowConditions { get; set; } = "{}";

    /// <summary>
    /// Gets or sets additional escrow metadata (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the escrow fee amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal EscrowFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the dispute resolution fee (if applicable)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal DisputeFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the total fees for the escrow
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalFees { get; set; } = 0;

    /// <summary>
    /// Gets or sets the net amount to be released to seller
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal NetReleaseAmount { get; set; }

    /// <summary>
    /// Gets or sets when the assets were locked in escrow
    /// </summary>
    public DateTime? LockedAt { get; set; }

    /// <summary>
    /// Gets or sets when the payment was received and verified
    /// </summary>
    public DateTime? FundedAt { get; set; }

    /// <summary>
    /// Gets or sets when the escrow was released
    /// </summary>
    public DateTime? ReleasedAt { get; set; }

    /// <summary>
    /// Gets or sets when the escrow was cancelled
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Gets or sets when the escrow expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the reason for cancellation (if cancelled)
    /// </summary>
    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Gets or sets whether this escrow is in dispute
    /// </summary>
    public bool IsDisputed { get; set; } = false;

    /// <summary>
    /// Gets or sets the dispute reason
    /// </summary>
    [MaxLength(1000)]
    public string? DisputeReason { get; set; }

    /// <summary>
    /// Gets or sets who initiated the dispute
    /// </summary>
    [ForeignKey("DisputeInitiator")]
    public Guid? DisputeInitiatedBy { get; set; }

    /// <summary>
    /// Gets or sets when the dispute was initiated
    /// </summary>
    public DateTime? DisputeInitiatedAt { get; set; }

    /// <summary>
    /// Gets or sets the dispute resolution outcome
    /// </summary>
    [MaxLength(1000)]
    public string? DisputeResolution { get; set; }

    /// <summary>
    /// Gets or sets who resolved the dispute
    /// </summary>
    [ForeignKey("DisputeResolver")]
    public Guid? DisputeResolvedBy { get; set; }

    /// <summary>
    /// Gets or sets when the dispute was resolved
    /// </summary>
    public DateTime? DisputeResolvedAt { get; set; }

    /// <summary>
    /// Gets or sets whether automatic release is enabled
    /// </summary>
    public bool AutoReleaseEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the automatic release delay in hours
    /// </summary>
    public int AutoReleaseDelayHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets when automatic release will occur
    /// </summary>
    public DateTime? AutoReleaseAt { get; set; }

    /// <summary>
    /// Gets or sets whether manual approval is required for release
    /// </summary>
    public bool RequiresManualApproval { get; set; } = false;

    /// <summary>
    /// Gets or sets who approved the manual release
    /// </summary>
    [ForeignKey("ApprovedBy")]
    public Guid? ApprovedByUserId { get; set; }

    /// <summary>
    /// Gets or sets when the manual approval was given
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Gets or sets the approval notes
    /// </summary>
    [MaxLength(1000)]
    public string? ApprovalNotes { get; set; }

    /// <summary>
    /// Gets or sets the number of release attempts
    /// </summary>
    public int ReleaseAttempts { get; set; } = 0;

    /// <summary>
    /// Gets or sets the maximum number of release attempts
    /// </summary>
    public int MaxReleaseAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the last release attempt error
    /// </summary>
    [MaxLength(1000)]
    public string? LastReleaseError { get; set; }

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
    /// Gets or sets the marketplace order for this escrow
    /// </summary>
    public MarketplaceOrder MarketplaceOrder { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of escrow history records
    /// </summary>
    public ICollection<EscrowHistory> EscrowHistory { get; set; } = new List<EscrowHistory>();
}
