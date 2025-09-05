using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SecurityService.Data.Entities;

[Table("SecurityEvents")]
[Index(nameof(UserId), nameof(EventType), nameof(Timestamp), IsUnique = false)]
[Index(nameof(EventType), nameof(Severity), nameof(Timestamp), IsUnique = false)]
[Index(nameof(CorrelationId), IsUnique = false)]
[Index(nameof(Timestamp), IsUnique = false)]
public class SecurityEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty; // MfaAttempt, MultiSigRequest, PolicyViolation, etc.

    [Required]
    [MaxLength(50)]
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty; // SecurityService, UserService, etc.

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty; // Login, Transfer, PolicyChange, etc.

    [Required]
    [MaxLength(50)]
    public string Result { get; set; } = string.Empty; // Success, Failure, Blocked, Pending

    [Required]
    [Column(TypeName = "jsonb")]
    public string EventData { get; set; } = "{}";

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string IpAddress { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string UserAgent { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SessionId { get; set; }

    [MaxLength(100)]
    public string? DeviceId { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    [Required]
    public bool RequiresInvestigation { get; set; } = false;

    [Required]
    public bool IsResolved { get; set; } = false;

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(100)]
    public string? ResolvedBy { get; set; }

    [MaxLength(1000)]
    public string? ResolutionNotes { get; set; }

    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";
}
