using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Data.Entities;

[Table("FeeDistributions")]
[Index(nameof(FeeTransactionId), nameof(RecipientType))]
[Index(nameof(Status), nameof(CreatedAt))]
[Index(nameof(RecipientId), nameof(Status))]
public class FeeDistribution
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string RecipientType { get; set; } = string.Empty; // "Treasury", "Staking", "Liquidity", "Burn"

    [Required]
    [MaxLength(255)]
    public string RecipientId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(8,6)")]
    public decimal Percentage { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // "Pending", "Completed", "Failed"

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    [MaxLength(255)]
    public string? TransactionHash { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; } // JSON for additional distribution data

    // Foreign Keys
    [ForeignKey("FeeTransaction")]
    public Guid FeeTransactionId { get; set; }

    [ForeignKey("DistributionRule")]
    public Guid? DistributionRuleId { get; set; }

    // Navigation properties
    public virtual FeeTransaction FeeTransaction { get; set; } = null!;
    public virtual DistributionRule? DistributionRule { get; set; }
}
