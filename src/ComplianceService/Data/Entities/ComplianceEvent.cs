using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ComplianceService.Data.Entities;

[Table("ComplianceEvents")]
[Index(nameof(UserId), nameof(EventType), IsUnique = false)]
[Index(nameof(Timestamp), IsUnique = false)]
[Index(nameof(Severity), IsUnique = false)]
[Index(nameof(CorrelationId), IsUnique = false)]
public class ComplianceEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public Guid? KycVerificationId { get; set; }

    public Guid? CaseId { get; set; }

    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty; // KycInitiated, KycCompleted, CaseCreated, DocumentSubmitted, ReviewCompleted

    [Required]
    [StringLength(20)]
    public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Source { get; set; } = "ComplianceService";

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Result { get; set; } = string.Empty; // Success, Failure, Warning, Info

    [Column(TypeName = "text")]
    public string? EventData { get; set; } // JSON data

    [Required]
    [StringLength(100)]
    public string CorrelationId { get; set; } = string.Empty;

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    public bool RequiresInvestigation { get; set; } = false;

    [Required]
    public bool IsResolved { get; set; } = false;

    public DateTime? ResolvedAt { get; set; }

    [StringLength(100)]
    public string? ResolvedBy { get; set; }

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }

    // Navigation properties
    [ForeignKey("KycVerificationId")]
    public virtual KycVerification? KycVerification { get; set; }

    [ForeignKey("CaseId")]
    public virtual ComplianceCase? ComplianceCase { get; set; }
}
