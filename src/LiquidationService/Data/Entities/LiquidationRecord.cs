using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiquidationService.Data.Entities;

/// <summary>
/// Represents a liquidation record in the system
/// </summary>
[Table("LiquidationRecords")]
public class LiquidationRecord : ITimestampEntity
{
    /// <summary>
    /// Unique identifier for the liquidation record
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The liquidation request ID this record is associated with
    /// </summary>
    [Required]
    public Guid LiquidationRequestId { get; set; }

    /// <summary>
    /// The status of the liquidation
    /// </summary>
    [Required]
    public LiquidationStatus Status { get; set; }

    /// <summary>
    /// The user ID associated with this liquidation
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The asset being liquidated
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AssetId { get; set; } = string.Empty;

    /// <summary>
    /// The amount being liquidated
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The liquidation price
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? LiquidationPrice { get; set; }

    /// <summary>
    /// The proceeds from the liquidation
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? Proceeds { get; set; }

    /// <summary>
    /// Transaction hash if applicable
    /// </summary>
    [MaxLength(100)]
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Error message if liquidation failed
    /// </summary>
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata in JSON format
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// When the record was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the record was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the liquidation request
    /// </summary>
    [ForeignKey(nameof(LiquidationRequestId))]
    public virtual LiquidationRequest? LiquidationRequest { get; set; }
}

/// <summary>
/// Enumeration of liquidation statuses
/// </summary>
public enum LiquidationStatus
{
    /// <summary>
    /// Liquidation is pending
    /// </summary>
    Pending,

    /// <summary>
    /// Liquidation is in progress
    /// </summary>
    InProgress,

    /// <summary>
    /// Liquidation completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Liquidation failed
    /// </summary>
    Failed,

    /// <summary>
    /// Liquidation was cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Liquidation is partially complete
    /// </summary>
    PartiallyComplete
}
