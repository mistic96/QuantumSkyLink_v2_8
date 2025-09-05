using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class ComplianceCheckResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string CheckType { get; set; } = string.Empty; // AML, Sanctions, PEP, etc.

    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Passed, Failed, Pending, Review

    [StringLength(50)]
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical

    public decimal RiskScore { get; set; }

    [StringLength(1000)]
    public string? CheckResults { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(100)]
    public string? Provider { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public Guid? CheckedBy { get; set; }

    [StringLength(1000)]
    public string? ReviewNotes { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    public List<string> Flags { get; set; } = new();

    public List<string> Recommendations { get; set; } = new();

    public DateTime? ExpiryDate { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    public bool IsCompliant { get; set; }

    public List<string> Issues { get; set; } = new();
}
