using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class KycProcessingResultResponse
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(50)]
    public string RiskLevel { get; set; } = string.Empty;

    public decimal? RiskScore { get; set; }

    [StringLength(1000)]
    public string? Results { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    public List<string> PassedChecks { get; set; } = new();

    public List<string> FailedChecks { get; set; } = new();

    public List<string> PendingChecks { get; set; } = new();

    [StringLength(100)]
    public string? Provider { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public Guid? ProcessedBy { get; set; }

    [StringLength(1000)]
    public string? ProcessingNotes { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    [StringLength(500)]
    public string? NextSteps { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsSuccessful { get; set; } = true;

    [StringLength(1000)]
    public string? RecommendedActions { get; set; }

    public decimal CompletionPercentage { get; set; }
}
