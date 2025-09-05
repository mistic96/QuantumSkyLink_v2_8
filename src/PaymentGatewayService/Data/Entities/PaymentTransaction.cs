using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGatewayService.Data.Entities;

[Table("PaymentTransactions")]
public class PaymentTransaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public PaymentType Type { get; set; } = PaymentType.Deposit;

    public PaymentGatewayType Gateway { get; set; } = PaymentGatewayType.Square;

    public PaymentGatewayType PaymentGateway { get; set; } = PaymentGatewayType.Square;

    [StringLength(100)]
    public string? PaymentGatewayId { get; set; }

    [StringLength(100)]
    public string? GatewayTransactionId { get; set; }

    public PaymentMethodType MethodType { get; set; } = PaymentMethodType.CreditCard;

    [StringLength(100)]
    public string? PaymentMethodId { get; set; }

    [StringLength(100)]
    public string? ExternalTransactionId { get; set; }

    [StringLength(100)]
    public string? ExternalPaymentId { get; set; }

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

    public DateTime? ExpiresAt { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ProcessingFee { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? FeeAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? NetAmount { get; set; }

    [StringLength(50)]
    public string? ReferenceNumber { get; set; }

    public int PaymentAttempts { get; set; } = 0;

    [StringLength(45)]
    public string? ClientIpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public bool IsReconciled { get; set; } = false;

    public DateTime? ReconciledAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
