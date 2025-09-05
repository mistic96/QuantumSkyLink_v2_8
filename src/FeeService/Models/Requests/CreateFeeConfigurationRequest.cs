using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class CreateFeeConfigurationRequest
{
    [Required]
    [StringLength(100)]
    public string FeeType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [StringLength(255)]
    public string? EntityId { get; set; }

    [Required]
    [StringLength(50)]
    public string CalculationType { get; set; } = string.Empty; // "Flat", "Percentage", "Tiered"

    public decimal? FlatFeeAmount { get; set; }

    [Range(0, 100)]
    public decimal? PercentageRate { get; set; }

    public decimal? MinimumFee { get; set; }

    public decimal? MaximumFee { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveUntil { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public object? TieredStructure { get; set; } // Will be serialized to JSON

    public object? DiscountRules { get; set; } // Will be serialized to JSON

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = string.Empty;
}
