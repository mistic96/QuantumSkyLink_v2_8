using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentGatewayService.Data.Entities;

/// <summary>
/// Represents a refund transaction for a payment
/// </summary>
[Table("Refunds")]
[Index(nameof(PaymentId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
[Index(nameof(RequestedBy))]
[Index(nameof(GatewayRefundId))]
public class Refund : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the refund
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the payment ID this refund is for
    /// </summary>
    [Required]
    [ForeignKey("Payment")]
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the refund amount
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency for the refund
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason for the refund
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the refund
    /// </summary>
    [Required]
    public RefundStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the external gateway refund ID
    /// </summary>
    [MaxLength(255)]
    public string? GatewayRefundId { get; set; }

    /// <summary>
    /// Gets or sets the user ID who requested the refund
    /// </summary>
    [Required]
    [ForeignKey("RequestedByUser")]
    public Guid RequestedBy { get; set; }

    /// <summary>
    /// Gets or sets the user ID who approved the refund (if different)
    /// </summary>
    [ForeignKey("ApprovedByUser")]
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// Gets or sets when the refund was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets when the refund was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Gets or sets the full gateway response (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? GatewayResponse { get; set; }

    /// <summary>
    /// Gets or sets the error code if the refund failed
    /// </summary>
    [MaxLength(500)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message if the refund failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the refund fee charged by the gateway
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal RefundFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the net refund amount after fees
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal NetRefundAmount { get; set; }

    /// <summary>
    /// Gets or sets whether this is a partial refund
    /// </summary>
    [Required]
    public bool IsPartialRefund { get; set; } = false;

    /// <summary>
    /// Gets or sets additional notes about the refund
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the refund (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the processing time in milliseconds
    /// </summary>
    public int? ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets when the refund is expected to complete
    /// </summary>
    public DateTime? ExpectedCompletionAt { get; set; }

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
    /// Gets or sets the payment this refund is for
    /// </summary>
    public Payment Payment { get; set; } = null!;
}
