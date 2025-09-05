using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class UploadDocumentRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string DocumentType { get; set; } = string.Empty; // Passport, License, Utility, Bank, etc.

    [Required]
    [StringLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FileType { get; set; } = string.Empty; // PDF, JPG, PNG, etc.

    [Required]
    public long FileSize { get; set; }

    [Required]
    [StringLength(2000)]
    public string FileContent { get; set; } = string.Empty; // Base64 encoded content

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? DocumentNumber { get; set; }

    public DateTime? DocumentExpiryDate { get; set; }

    [StringLength(100)]
    public string? IssuingCountry { get; set; }

    [StringLength(100)]
    public string? IssuingAuthority { get; set; }

    public Guid? UploadedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool IsConfidential { get; set; } = true;

    [StringLength(100)]
    public string? VerificationPurpose { get; set; }

    public Guid VerificationId { get; set; }

    public byte[]? DocumentData { get; set; }
}
