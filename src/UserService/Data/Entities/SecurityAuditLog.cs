using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UserService.Models.Security;

namespace UserService.Data.Entities;

[Table("SecurityAuditLogs")]
[Index(nameof(UserId), nameof(Timestamp))]
[Index(nameof(EventType), nameof(Timestamp))]
[Index(nameof(IpAddress), nameof(Timestamp))]
public class SecurityAuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }

    [Required]
    public SecurityEventType EventType { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Resource { get; set; }

    [MaxLength(100)]
    public string? Action { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Required]
    public AuditLogLevel Level { get; set; } = AuditLogLevel.Information;

    [Column(TypeName = "jsonb")]
    public string? Details { get; set; }

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    [MaxLength(100)]
    public string? SessionId { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? PerformedBy { get; set; }

    public bool IsSuccessful { get; set; } = true;

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    // Navigation Properties
    public virtual User? User { get; set; }
}
