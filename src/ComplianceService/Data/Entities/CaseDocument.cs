using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ComplianceService.Data.Entities;

[Table("CaseDocuments")]
[Index(nameof(CaseId), nameof(DocumentType), IsUnique = false)]
[Index(nameof(UploadedAt), IsUnique = false)]
public class CaseDocument
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CaseId { get; set; }

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
    public long FileSize { get; set; }

    [Column(TypeName = "bytea")]
    public byte[]? FileData { get; set; }

    [StringLength(500)]
    public string? FileUrl { get; set; } // Alternative to storing binary data

    [StringLength(1000)]
    public string? Comments { get; set; }

    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string UploadedBy { get; set; } = string.Empty;

    [Required]
    public bool IsProcessed { get; set; } = false;

    public DateTime? ProcessedAt { get; set; }

    [StringLength(100)]
    public string? ProcessedBy { get; set; }

    [StringLength(500)]
    public string? ProcessingResult { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [StringLength(100)]
    public string? CorrelationId { get; set; }

    // Navigation properties
    [ForeignKey("CaseId")]
    public virtual ComplianceCase ComplianceCase { get; set; } = null!;
}
