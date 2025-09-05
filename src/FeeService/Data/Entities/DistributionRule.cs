using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Data.Entities;

[Table("DistributionRules")]
[Index(nameof(FeeType), nameof(IsActive))]
[Index(nameof(Priority), nameof(IsActive))]
[Index(nameof(EffectiveFrom), nameof(EffectiveUntil))]
public class DistributionRule
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FeeType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string RecipientType { get; set; } = string.Empty; // "Treasury", "Staking", "Liquidity", "Burn"

    [Required]
    [MaxLength(255)]
    public string RecipientId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(8,6)")]
    public decimal Percentage { get; set; }

    [Required]
    public int Priority { get; set; } // Lower number = higher priority

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveUntil { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Conditions { get; set; } // JSON for conditional distribution logic

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinimumAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MaximumAmount { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [Required]
    [MaxLength(255)]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<FeeDistribution> FeeDistributions { get; set; } = new List<FeeDistribution>();
}
