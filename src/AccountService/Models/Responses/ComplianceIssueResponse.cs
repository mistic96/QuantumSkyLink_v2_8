using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class ComplianceIssueResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(1000)]
    public string Issue { get; set; } = string.Empty;

    public DateTime IdentifiedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string IssueType { get; set; } = string.Empty; // AML, Sanctions, KYC, Document, etc.

    [StringLength(50)]
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical

    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Escalated

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Resolution { get; set; }

    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public Guid? DetectedBy { get; set; }

    public Guid? AssignedTo { get; set; }

    public Guid? ResolvedBy { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public List<string> RequiredActions { get; set; } = new();

    public List<string> CompletedActions { get; set; } = new();

    [StringLength(50)]
    public string Priority { get; set; } = "Normal";

    public bool RequiresImmediateAction { get; set; } = false;

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    public DateTime? DueDate { get; set; }

    [StringLength(100)]
    public string? RegulatoryReference { get; set; }
}
