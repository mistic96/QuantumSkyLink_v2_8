using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class KycStatusResponse
{
    public Guid AccountId { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // NotStarted, InProgress, Completed, Failed, Expired

    [StringLength(50)]
    public string Level { get; set; } = string.Empty; // Basic, Enhanced, Premium

    public decimal CompletionPercentage { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ExpiryDate { get; set; }

    [StringLength(1000)]
    public string? RejectionReason { get; set; }

    public List<string> CompletedSteps { get; set; } = new();

    public List<string> PendingSteps { get; set; } = new();

    public List<string> FailedSteps { get; set; } = new();

    [StringLength(100)]
    public string? CurrentStep { get; set; }

    public int TotalSteps { get; set; }

    public int CompletedStepCount { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    public bool IsExpired { get; set; } = false;

    [StringLength(100)]
    public string? Provider { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public Guid? LastUpdatedBy { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }
}
