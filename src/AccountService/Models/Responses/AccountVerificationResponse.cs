using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class AccountVerificationResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string VerificationType { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? DocumentType { get; set; }

    [StringLength(200)]
    public string? DocumentNumber { get; set; }

    public DateTime? DocumentExpiryDate { get; set; }

    [StringLength(1000)]
    public string? DocumentImageUrl { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    public Guid? InitiatedBy { get; set; }

    public Guid? ReviewedBy { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public bool RequiresManualReview { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [StringLength(1000)]
    public string? ReviewNotes { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public int AttemptCount { get; set; }

    public DateTime? NextRetryDate { get; set; }
}
