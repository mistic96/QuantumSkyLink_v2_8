using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class DocumentUploadResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string DocumentType { get; set; } = string.Empty;

    [StringLength(500)]
    public string FileName { get; set; } = string.Empty;

    [StringLength(50)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? DocumentNumber { get; set; }

    public DateTime? DocumentExpiryDate { get; set; }

    [StringLength(100)]
    public string? IssuingCountry { get; set; }

    [StringLength(100)]
    public string? IssuingAuthority { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Guid? UploadedBy { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Uploaded"; // Uploaded, Processing, Verified, Rejected

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public bool IsConfidential { get; set; } = true;

    [StringLength(100)]
    public string? VerificationPurpose { get; set; }

    [StringLength(1000)]
    public string? StorageLocation { get; set; }

    [StringLength(100)]
    public string? ChecksumHash { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public Guid? VerifiedBy { get; set; }

    [StringLength(1000)]
    public string? VerificationNotes { get; set; }

    public bool IsExpired { get; set; } = false;
}
