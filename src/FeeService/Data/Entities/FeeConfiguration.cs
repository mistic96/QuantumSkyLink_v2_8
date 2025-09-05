using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Data.Entities;

[Table("FeeConfigurations")]
[Index(nameof(FeeType), nameof(EntityType), nameof(IsActive))]
[Index(nameof(EffectiveFrom), nameof(EffectiveUntil))]
public class FeeConfiguration
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FeeType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string CalculationType { get; set; } = string.Empty; // "Flat", "Percentage", "Tiered"

    [Column(TypeName = "decimal(18,8)")]
    public decimal? FlatFeeAmount { get; set; }

    [Column(TypeName = "decimal(8,6)")]
    public decimal? PercentageRate { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinimumFee { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MaximumFee { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveUntil { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public string? TieredStructure { get; set; } // JSON for complex tiered fee structures

    [Column(TypeName = "jsonb")]
    public string? DiscountRules { get; set; } // JSON for discount configurations

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
    public virtual ICollection<FeeTransaction> FeeTransactions { get; set; } = new List<FeeTransaction>();
    public virtual ICollection<FeeCalculationResult> CalculationResults { get; set; } = new List<FeeCalculationResult>();
}
