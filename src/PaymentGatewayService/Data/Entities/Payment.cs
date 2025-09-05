using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGatewayService.Data.Entities;

[Table("Payments")]
public class Payment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public PaymentType Type { get; set; } = PaymentType.Deposit;

    public PaymentGatewayType PaymentGateway { get; set; } = PaymentGatewayType.Square;

    public Guid? PaymentGatewayId { get; set; }

    [StringLength(100)]
    public string? GatewayTransactionId { get; set; }

    public Guid? PaymentMethodId { get; set; }

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
    public decimal? FeeAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? NetAmount { get; set; }

    public int PaymentAttempts { get; set; } = 0;

    [StringLength(45)]
    public string? ClientIpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public Guid? ProviderId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
