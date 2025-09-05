using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class KycResultRequest
{
    [Required]
    public Guid VerificationId { get; set; }

    public bool IsApproved { get; set; } = false;

    [StringLength(100)]
    public string? ExternalVerificationId { get; set; }

    [StringLength(1000)]
    public string? RejectionReason { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Passed, Failed, Pending, Review

    [StringLength(50)]
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical

    public decimal? RiskScore { get; set; }

    [StringLength(1000)]
    public string? Results { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(100)]
    public string? Provider { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public Guid? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [StringLength(1000)]
    public string? ProcessingNotes { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    [StringLength(500)]
    public string? NextSteps { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public List<string> VerificationChecks { get; set; } = new();

    [StringLength(1000)]
    public string? RecommendedActions { get; set; }
}
