using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class ComplianceStatusResponse
{
    public Guid AccountId { get; set; }

    [StringLength(50)]
    public string OverallStatus { get; set; } = string.Empty; // Compliant, NonCompliant, Pending, Review

    [StringLength(50)]
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical

    public decimal ComplianceScore { get; set; }

    public DateTime LastAssessmentDate { get; set; }

    public DateTime? NextAssessmentDue { get; set; }

    public int TotalChecks { get; set; }

    public int PassedChecks { get; set; }

    public int FailedChecks { get; set; }

    public int PendingChecks { get; set; }

    public List<string> CompletedRequirements { get; set; } = new();

    public List<string> PendingRequirements { get; set; } = new();

    public List<string> FailedRequirements { get; set; } = new();

    public int OpenIssues { get; set; }

    public int CriticalIssues { get; set; }

    [StringLength(1000)]
    public string? Summary { get; set; }

    [StringLength(500)]
    public string? Recommendations { get; set; }

    public bool RequiresImmediateAttention { get; set; } = false;

    public DateTime? LastKycUpdate { get; set; }

    public DateTime? LastAmlCheck { get; set; }

    [StringLength(100)]
    public string? AssessedBy { get; set; }

    [StringLength(2000)]
    public string? AdditionalNotes { get; set; }
}
