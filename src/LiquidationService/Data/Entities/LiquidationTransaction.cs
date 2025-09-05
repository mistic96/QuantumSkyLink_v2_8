using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidationService.Data.Entities;

/// <summary>
/// Represents a liquidation transaction execution
/// </summary>
[Table("LiquidationTransactions")]
[Index(nameof(LiquidationRequestId))]
[Index(nameof(LiquidityProviderId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
[Index(nameof(TransactionHash))]
public class LiquidationTransaction : ITimestampEntity
{
    /// <summary>
    /// Unique identifier for the liquidation transaction
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the associated liquidation request
    /// </summary>
    [Required]
    public Guid LiquidationRequestId { get; set; }

    /// <summary>
    /// ID of the liquidity provider executing the transaction
    /// </summary>
    [Required]
    public Guid LiquidityProviderId { get; set; }

    /// <summary>
    /// Current status of the transaction
    /// </summary>
    [Required]
    public LiquidationTransactionStatus Status { get; set; } = LiquidationTransactionStatus.Pending;

    /// <summary>
    /// Asset symbol being liquidated
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Amount of asset being liquidated
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal AssetAmount { get; set; }

    /// <summary>
    /// Output currency/asset symbol
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string OutputSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Output amount after liquidation
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? OutputAmount { get; set; }

    /// <summary>
    /// Exchange rate used for the liquidation
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Market price at execution time
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MarketPriceAtExecution { get; set; }

    /// <summary>
    /// Slippage percentage
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? SlippagePercentage { get; set; }

    /// <summary>
    /// Provider fee amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? ProviderFee { get; set; }

    /// <summary>
    /// Platform fee amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? PlatformFee { get; set; }

    /// <summary>
    /// Network/gas fee amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? NetworkFee { get; set; }

    /// <summary>
    /// Total fees charged
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? TotalFees { get; set; }

    /// <summary>
    /// Net amount received after fees
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? NetAmount { get; set; }

    /// <summary>
    /// Blockchain transaction hash
    /// </summary>
    [MaxLength(100)]
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Block number where transaction was confirmed
    /// </summary>
    public long? BlockNumber { get; set; }

    /// <summary>
    /// Number of confirmations
    /// </summary>
    public int? Confirmations { get; set; }

    /// <summary>
    /// Smart contract address used
    /// </summary>
    [MaxLength(100)]
    public string? SmartContractAddress { get; set; }

    /// <summary>
    /// Source wallet address
    /// </summary>
    [MaxLength(100)]
    public string? SourceAddress { get; set; }

    /// <summary>
    /// Destination wallet address
    /// </summary>
    [MaxLength(100)]
    public string? DestinationAddress { get; set; }

    /// <summary>
    /// External payment transaction ID (for fiat transfers)
    /// </summary>
    [MaxLength(100)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Payment gateway used (for fiat transfers)
    /// </summary>
    [MaxLength(100)]
    public string? PaymentGateway { get; set; }

    /// <summary>
    /// Estimated execution time in minutes
    /// </summary>
    public int? EstimatedExecutionTimeMinutes { get; set; }

    /// <summary>
    /// Actual execution time in minutes
    /// </summary>
    public int? ActualExecutionTimeMinutes { get; set; }

    /// <summary>
    /// Date and time when execution started
    /// </summary>
    public DateTime? ExecutionStartedAt { get; set; }

    /// <summary>
    /// Date and time when execution completed
    /// </summary>
    public DateTime? ExecutionCompletedAt { get; set; }

    /// <summary>
    /// Date and time when transaction was confirmed on blockchain
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryAttempts { get; set; } = 0;

    /// <summary>
    /// Maximum number of retries allowed
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Date and time for next retry attempt
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Error message if transaction failed
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Failure reason category
    /// </summary>
    [MaxLength(100)]
    public string? FailureReason { get; set; }

    /// <summary>
    /// Whether the transaction is reversible
    /// </summary>
    public bool IsReversible { get; set; } = false;

    /// <summary>
    /// Date and time when reversal is possible until
    /// </summary>
    public DateTime? ReversibleUntil { get; set; }

    /// <summary>
    /// Whether the transaction has been reversed
    /// </summary>
    public bool IsReversed { get; set; } = false;

    /// <summary>
    /// Reversal transaction hash
    /// </summary>
    [MaxLength(100)]
    public string? ReversalTransactionHash { get; set; }

    /// <summary>
    /// Date and time when transaction was reversed
    /// </summary>
    public DateTime? ReversedAt { get; set; }

    /// <summary>
    /// Reason for reversal
    /// </summary>
    [MaxLength(1000)]
    public string? ReversalReason { get; set; }

    /// <summary>
    /// Additional transaction metadata (JSON format)
    /// </summary>
    [Column(TypeName = "text")]
    public string? Metadata { get; set; }

    /// <summary>
    /// Transaction notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the transaction was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the transaction was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Associated liquidation request
    /// </summary>
    [ForeignKey(nameof(LiquidationRequestId))]
    public LiquidationRequest? LiquidationRequest { get; set; }

    /// <summary>
    /// Associated liquidity provider
    /// </summary>
    [ForeignKey(nameof(LiquidityProviderId))]
    public LiquidityProvider? LiquidityProvider { get; set; }
}
