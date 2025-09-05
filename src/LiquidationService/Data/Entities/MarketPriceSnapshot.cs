using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidationService.Data.Entities;

/// <summary>
/// Represents a market price snapshot for assets
/// </summary>
[Table("MarketPriceSnapshots")]
[Index(nameof(AssetSymbol))]
[Index(nameof(OutputSymbol))]
[Index(nameof(LiquidationRequestId))]
[Index(nameof(CreatedAt))]
[Index(nameof(PriceSource))]
public class MarketPriceSnapshot : ITimestampEntity
{
    /// <summary>
    /// Unique identifier for the price snapshot
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the associated liquidation request (optional)
    /// </summary>
    public Guid? LiquidationRequestId { get; set; }

    /// <summary>
    /// Asset symbol (e.g., BTC, ETH)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Output currency/asset symbol (e.g., USD, USDT)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string OutputSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Current market price
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Bid price (highest price buyers are willing to pay)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? BidPrice { get; set; }

    /// <summary>
    /// Ask price (lowest price sellers are willing to accept)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? AskPrice { get; set; }

    /// <summary>
    /// Spread between bid and ask prices
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? Spread { get; set; }

    /// <summary>
    /// 24-hour trading volume
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? Volume24h { get; set; }

    /// <summary>
    /// 24-hour price change percentage
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? Change24hPercent { get; set; }

    /// <summary>
    /// 24-hour high price
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? High24h { get; set; }

    /// <summary>
    /// 24-hour low price
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? Low24h { get; set; }

    /// <summary>
    /// Market capitalization
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MarketCap { get; set; }

    /// <summary>
    /// Available liquidity at current price
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? AvailableLiquidity { get; set; }

    /// <summary>
    /// Source of the price data (e.g., CoinGecko, Binance, Coinbase)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PriceSource { get; set; } = string.Empty;

    /// <summary>
    /// Exchange or platform where the price was obtained
    /// </summary>
    [MaxLength(100)]
    public string? Exchange { get; set; }

    /// <summary>
    /// Trading pair used for the price (e.g., BTC/USD)
    /// </summary>
    [MaxLength(50)]
    public string? TradingPair { get; set; }

    /// <summary>
    /// Confidence level of the price data (0-100)
    /// </summary>
    [Range(0, 100)]
    public int? ConfidenceLevel { get; set; }

    /// <summary>
    /// Whether this price is suitable for liquidation
    /// </summary>
    public bool IsSuitableForLiquidation { get; set; } = true;

    /// <summary>
    /// Reason if price is not suitable for liquidation
    /// </summary>
    [MaxLength(500)]
    public string? UnsuitabilityReason { get; set; }

    /// <summary>
    /// Estimated slippage for large transactions
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? EstimatedSlippage { get; set; }

    /// <summary>
    /// Minimum transaction size for this price
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinTransactionSize { get; set; }

    /// <summary>
    /// Maximum transaction size for this price
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MaxTransactionSize { get; set; }

    /// <summary>
    /// Time when the price was recorded at the source
    /// </summary>
    public DateTime? SourceTimestamp { get; set; }

    /// <summary>
    /// Latency in milliseconds from source to our system
    /// </summary>
    public int? LatencyMs { get; set; }

    /// <summary>
    /// Whether this is a real-time or delayed price
    /// </summary>
    public bool IsRealTime { get; set; } = true;

    /// <summary>
    /// Delay in seconds if not real-time
    /// </summary>
    public int? DelaySeconds { get; set; }

    /// <summary>
    /// Price validity duration in minutes
    /// </summary>
    public int ValidityMinutes { get; set; } = 5;

    /// <summary>
    /// Date and time when the price expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this price has been used for a liquidation
    /// </summary>
    public bool IsUsedForLiquidation { get; set; } = false;

    /// <summary>
    /// Date and time when the price was used for liquidation
    /// </summary>
    public DateTime? UsedForLiquidationAt { get; set; }

    /// <summary>
    /// Additional price metadata (JSON format)
    /// </summary>
    [Column(TypeName = "text")]
    public string? Metadata { get; set; }

    /// <summary>
    /// Raw response from price source (for debugging)
    /// </summary>
    [Column(TypeName = "text")]
    public string? RawResponse { get; set; }

    /// <summary>
    /// Error message if price retrieval failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of retry attempts for price retrieval
    /// </summary>
    public int RetryAttempts { get; set; } = 0;

    /// <summary>
    /// Date and time when the price snapshot was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the price snapshot was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    /// <summary>
    /// Associated liquidation request
    /// </summary>
    [ForeignKey(nameof(LiquidationRequestId))]
    public LiquidationRequest? LiquidationRequest { get; set; }
}
