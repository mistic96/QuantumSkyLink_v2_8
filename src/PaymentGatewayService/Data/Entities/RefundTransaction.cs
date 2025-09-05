using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGatewayService.Data.Entities;

[Table("RefundTransactions")]
public class RefundTransaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string RefundId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string OriginalTransactionId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal RefundAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    public RefundStatus Status { get; set; } = RefundStatus.Pending;

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ExternalRefundId { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Metadata { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ProcessingFee { get; set; }

    [StringLength(50)]
    public string? ReferenceNumber { get; set; }

    public bool IsReconciled { get; set; } = false;

    public DateTime? ReconciledAt { get; set; }

    [StringLength(100)]
    public string? RefundMethod { get; set; }

    public int? EstimatedDaysToComplete { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("OriginalTransactionId")]
    public virtual PaymentTransaction? OriginalTransaction { get; set; }
}
