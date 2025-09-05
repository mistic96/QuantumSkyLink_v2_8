using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidationService.Data.Entities;

/// <summary>
/// Represents a user request to liquidate assets
/// </summary>
[Table("LiquidationRequests")]
[Index(nameof(UserId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
[Index(nameof(AssetSymbol))]
public class LiquidationRequest : ITimestampEntity
{
    /// <summary>
    /// Unique identifier for the liquidation request
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user making the liquidation request
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Symbol of the asset to be liquidated (e.g., BTC, ETH)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Amount of the asset to be liquidated
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal AssetAmount { get; set; }

    /// <summary>
    /// Type of output desired (Fiat, Stablecoin, Cryptocurrency)
    /// </summary>
    [Required]
    public LiquidationOutputType OutputType { get; set; }

    /// <summary>
    /// Symbol of the desired output currency/asset
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string OutputSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Type of destination for the liquidated funds
    /// </summary>
    [Required]
    public DestinationType DestinationType { get; set; }

    /// <summary>
    /// Destination address or account identifier
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Additional destination details (e.g., bank account details)
    /// </summary>
    [MaxLength(1000)]
    public string? DestinationDetails { get; set; }

    /// <summary>
    /// Current status of the liquidation request
    /// </summary>
    [Required]
    public LiquidationRequestStatus Status { get; set; } = LiquidationRequestStatus.Pending;

    /// <summary>
    /// Market price of the asset at the time of request
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MarketPriceAtRequest { get; set; }

    /// <summary>
    /// Estimated output amount based on market price
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? EstimatedOutputAmount { get; set; }

    /// <summary>
    /// Actual output amount after execution
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? ActualOutputAmount { get; set; }

    /// <summary>
    /// Fees charged for the liquidation
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? Fees { get; set; }

    /// <summary>
    /// Exchange rate used for the liquidation
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// ID of the assigned liquidity provider
    /// </summary>
    public Guid? LiquidityProviderId { get; set; }

    /// <summary>
    /// KYC verification status
    /// </summary>
    public bool KycVerified { get; set; } = false;

    /// <summary>
    /// Compliance check status
    /// </summary>
    public bool ComplianceApproved { get; set; } = false;

    /// <summary>
    /// Asset eligibility verification status
    /// </summary>
    public bool AssetEligibilityVerified { get; set; } = false;

    /// <summary>
    /// Risk level assessment
    /// </summary>
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Medium;

    /// <summary>
    /// Requires multi-signature approval
    /// </summary>
    public bool RequiresMultiSignature { get; set; } = false;

    /// <summary>
    /// Multi-signature approval status
    /// </summary>
    public bool MultiSignatureApproved { get; set; } = false;

    /// <summary>
    /// Blockchain transaction hash for the liquidation
    /// </summary>
    [MaxLength(100)]
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Reference to external payment/transfer transaction
    /// </summary>
    [MaxLength(100)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Reason for rejection or failure
    /// </summary>
    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the request was submitted
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the request was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the liquidation was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Date and time when the request expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Associated liquidity provider
    /// </summary>
    [ForeignKey(nameof(LiquidityProviderId))]
    public LiquidityProvider? LiquidityProvider { get; set; }

    /// <summary>
    /// Associated compliance checks
    /// </summary>
    public ICollection<ComplianceCheck> ComplianceChecks { get; set; } = new List<ComplianceCheck>();

    /// <summary>
    /// Associated liquidation transactions
    /// </summary>
    public ICollection<LiquidationTransaction> LiquidationTransactions { get; set; } = new List<LiquidationTransaction>();

    /// <summary>
    /// Associated market price snapshots
    /// </summary>
    public ICollection<MarketPriceSnapshot> MarketPriceSnapshots { get; set; } = new List<MarketPriceSnapshot>();
}
