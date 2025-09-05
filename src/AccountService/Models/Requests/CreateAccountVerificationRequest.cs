using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class CreateAccountVerificationRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string VerificationType { get; set; } = string.Empty; // KYC, AML, Identity, Document, etc.

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

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

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    [StringLength(50)]
    public string Priority { get; set; } = "Normal";

    [StringLength(100)]
    public string? ExternalVerificationId { get; set; }

    [StringLength(500)]
    public string? DocumentPath { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }

    [StringLength(2000)]
    public string? Metadata { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
