using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ComplianceService.Data.Entities;

[Table("KycVerifications")]
[Index(nameof(UserId), nameof(Status), IsUnique = false)]
[Index(nameof(ComplyCubeClientId), IsUnique = true)]
[Index(nameof(CorrelationId), IsUnique = false)]
public class KycVerification
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string ComplyCubeClientId { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ComplyCubeCheckId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Initiated"; // Initiated, EmailSent, DocumentsSubmitted, Approved, Rejected, NeedsReview, Expired

    [Required]
    [StringLength(50)]
    public string KycLevel { get; set; } = "Basic"; // Basic, Enhanced, Premium

    [Required]
    [StringLength(100)]
    public string TriggerReason { get; set; } = string.Empty; // RoleUpgrade, HighValueTransaction, Periodic, Manual

    [Column(TypeName = "text")]
    public string? ComplyCubeResult { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal? RiskScore { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(1000)]
    public string? Comments { get; set; }

    [Required]
    [StringLength(100)]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ComplianceCase> ComplianceCases { get; set; } = new List<ComplianceCase>();
    public virtual ICollection<ComplianceEvent> ComplianceEvents { get; set; } = new List<ComplianceEvent>();
}
