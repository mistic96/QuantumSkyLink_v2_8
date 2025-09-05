using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class KycWorkflowResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string WorkflowType { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Initiated, InProgress, Completed, Failed, Cancelled

    [StringLength(50)]
    public string CurrentStep { get; set; } = string.Empty;

    public int TotalSteps { get; set; }

    public int CompletedSteps { get; set; }

    public decimal ProgressPercentage { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? ExternalWorkflowId { get; set; }

    [StringLength(100)]
    public string? Provider { get; set; }

    [StringLength(2000)]
    public string? WorkflowData { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid? InitiatedBy { get; set; }

    public Guid? CompletedBy { get; set; }

    [StringLength(1000)]
    public string? CompletionNotes { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    public List<string> RequiredDocuments { get; set; } = new();

    public List<string> CompletedDocuments { get; set; } = new();

    public DateTime? ExpiryDate { get; set; }

    public bool RequiresManualReview { get; set; }
}
