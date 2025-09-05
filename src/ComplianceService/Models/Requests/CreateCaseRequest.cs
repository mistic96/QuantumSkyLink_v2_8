using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Models.Requests;

public class CreateCaseRequest
{
    public Guid? KycVerificationId { get; set; }

    [Required]
    [StringLength(50)]
    public string CaseType { get; set; } = string.Empty; // KycFailure, DocumentIssue, ManualReview, Appeal, Investigation

    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? AssignedTo { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}

public class SubmitCaseDocumentRequest
{
    [Required]
    [StringLength(100)]
    public string DocumentType { get; set; } = string.Empty; // Identity, Address, SourceOfFunds, Additional, Appeal

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    [StringLength(1000)]
    public string? Comments { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}

public class ReviewCaseRequest
{
    [Required]
    [StringLength(50)]
    public string ReviewType { get; set; } = string.Empty; // ComplianceOfficer, AI, System, Manual

    [Required]
    [StringLength(50)]
    public string ReviewResult { get; set; } = string.Empty; // Approved, Rejected, NeedsMoreInfo, Escalated, Resubmit

    [Required]
    [StringLength(1000)]
    public string ReviewNotes { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? DetailedAnalysis { get; set; }

    [Range(0.0, 1.0)]
    public decimal? ConfidenceScore { get; set; }

    [StringLength(200)]
    public string? RecommendedAction { get; set; }

    [StringLength(100)]
    public string? NextReviewBy { get; set; }

    public DateTime? NextReviewDate { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}
