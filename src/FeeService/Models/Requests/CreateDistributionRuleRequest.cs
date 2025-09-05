using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class CreateDistributionRuleRequest
{
    [Required]
    [StringLength(100)]
    public string FeeType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string RecipientType { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string RecipientId { get; set; } = string.Empty;

    [Required]
    [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
    public decimal Percentage { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Priority must be greater than 0")]
    public int Priority { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveUntil { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public object? Conditions { get; set; } // Will be serialized to JSON

    public decimal? MinimumAmount { get; set; }

    public decimal? MaximumAmount { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = string.Empty;
}
