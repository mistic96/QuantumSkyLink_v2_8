using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGatewayService.Data.Entities;

[Table("DepositCodes")]
public class DepositCode
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    public DepositCodeStatus Status { get; set; } = DepositCodeStatus.Active;

    public bool IsActive { get; set; } = true;

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UsedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? TransactionId { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(2000)]
    public string? Metadata { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? PaymentId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
