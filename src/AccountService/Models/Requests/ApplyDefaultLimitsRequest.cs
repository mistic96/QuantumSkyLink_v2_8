using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class ApplyDefaultLimitsRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string AccountType { get; set; } = string.Empty; // Individual, Business, Premium

    [Required]
    [StringLength(50)]
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High

    public List<string> LimitTypes { get; set; } = new(); // If empty, apply all default limits

    public bool OverrideExisting { get; set; } = false;

    [StringLength(500)]
    public string? Reason { get; set; }

    public Guid? AppliedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool RequiresApproval { get; set; } = false;

    [StringLength(100)]
    public string? ApprovalReference { get; set; }

    public DateTime? EffectiveDate { get; set; }

    [StringLength(100)]
    public string? RegulatoryBasis { get; set; }
}
