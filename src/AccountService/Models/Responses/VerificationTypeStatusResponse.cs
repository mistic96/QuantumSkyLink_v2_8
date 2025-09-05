using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class VerificationTypeStatusResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string VerificationType { get; set; } = string.Empty; // Identity, Address, Income, etc.

    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // NotStarted, InProgress, Completed, Failed, Expired

    [StringLength(50)]
    public string Level { get; set; } = string.Empty; // Basic, Enhanced, Premium

    public decimal CompletionPercentage { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsCompleted { get; set; } = false;

    public bool IsExpired { get; set; } = false;

    [StringLength(1000)]
    public string? FailureReason { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<string> RequiredDocuments { get; set; } = new();

    public List<string> SubmittedDocuments { get; set; } = new();

    public List<string> PendingRequirements { get; set; } = new();

    [StringLength(100)]
    public string? Provider { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public Guid? LastUpdatedBy { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

    public bool RequiresManualReview { get; set; } = false;

    [StringLength(2000)]
    public string? AdditionalData { get; set; }
}
