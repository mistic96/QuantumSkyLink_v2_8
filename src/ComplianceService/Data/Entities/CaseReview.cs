using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ComplianceService.Data.Entities;

[Table("CaseReviews")]
[Index(nameof(CaseId), nameof(ReviewType), IsUnique = false)]
[Index(nameof(ReviewedAt), IsUnique = false)]
[Index(nameof(ReviewedBy), IsUnique = false)]
public class CaseReview
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CaseId { get; set; }

    [Required]
    [StringLength(50)]
    public string ReviewType { get; set; } = string.Empty; // ComplianceOfficer, AI, System, Manual

    [Required]
    [StringLength(50)]
    public string ReviewResult { get; set; } = string.Empty; // Approved, Rejected, NeedsMoreInfo, Escalated, Resubmit

    [Required]
    [StringLength(1000)]
    public string ReviewNotes { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string? DetailedAnalysis { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? ConfidenceScore { get; set; }

    [StringLength(200)]
    public string? RecommendedAction { get; set; }

    [Required]
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string ReviewedBy { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NextReviewBy { get; set; }

    public DateTime? NextReviewDate { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [StringLength(100)]
    public string? CorrelationId { get; set; }

    [Column(TypeName = "text")]
    public string? AIAnalysisData { get; set; } // JSON data from AI analysis

    // Navigation properties
    [ForeignKey("CaseId")]
    public virtual ComplianceCase ComplianceCase { get; set; } = null!;
}
