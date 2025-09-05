using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ComplianceService.Data.Entities;

[Table("ComplianceCases")]
[Index(nameof(UserId), nameof(Status), IsUnique = false)]
[Index(nameof(KycVerificationId), IsUnique = false)]
[Index(nameof(CaseNumber), IsUnique = true)]
[Index(nameof(Priority), nameof(Status), IsUnique = false)]
public class ComplianceCase
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public Guid? KycVerificationId { get; set; }

    [Required]
    [StringLength(50)]
    public string CaseNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string CaseType { get; set; } = string.Empty; // KycFailure, DocumentIssue, ManualReview, Appeal, Investigation

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Open"; // Open, InProgress, PendingDocuments, UnderReview, Resolved, Closed, Escalated

    [Required]
    [StringLength(20)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string? Resolution { get; set; }

    [StringLength(100)]
    public string? AssignedTo { get; set; }

    public DateTime? AssignedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public bool RequiresComplianceOfficerReview { get; set; } = false;

    [Required]
    public bool RequiresAIReview { get; set; } = true;

    public DateTime? LastActivityAt { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }

    // Navigation properties
    [ForeignKey("KycVerificationId")]
    public virtual KycVerification? KycVerification { get; set; }

    public virtual ICollection<CaseDocument> CaseDocuments { get; set; } = new List<CaseDocument>();
    public virtual ICollection<CaseReview> CaseReviews { get; set; } = new List<CaseReview>();
    public virtual ICollection<ComplianceEvent> ComplianceEvents { get; set; } = new List<ComplianceEvent>();
}
