using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class DefaultLimitRequirementResponse
{
    [StringLength(100)]
    public string AccountType { get; set; } = string.Empty;

    [StringLength(50)]
    public string RiskLevel { get; set; } = string.Empty;

    public List<string> RequiredLimitTypes { get; set; } = new();

    public List<string> OptionalLimitTypes { get; set; } = new();

    public Dictionary<string, decimal> DefaultAmounts { get; set; } = new();

    public Dictionary<string, string> LimitPeriods { get; set; } = new();

    [StringLength(1000)]
    public string? Instructions { get; set; }

    [StringLength(500)]
    public string? RegulatoryBasis { get; set; }

    public bool RequiresApproval { get; set; } = false;

    [StringLength(100)]
    public string? ApprovalLevel { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdated { get; set; }

    [StringLength(2000)]
    public string? AdditionalNotes { get; set; }

    public Dictionary<string, string> LimitDescriptions { get; set; } = new();

    public Dictionary<string, decimal> MinimumAmounts { get; set; } = new();

    public Dictionary<string, decimal> MaximumAmounts { get; set; } = new();
}
