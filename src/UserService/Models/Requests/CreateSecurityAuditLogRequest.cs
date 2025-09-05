using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class CreateSecurityAuditLogRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty; // Login, Logout, PasswordChange, PermissionChange, etc.

    [Required]
    [StringLength(50)]
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(100)]
    public string? Resource { get; set; }

    [StringLength(50)]
    public string? Action { get; set; }

    [StringLength(50)]
    public string? Result { get; set; } // Success, Failure, Blocked

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    [StringLength(100)]
    public string? SessionId { get; set; }

    [StringLength(100)]
    public string? DeviceId { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    public DateTime? EventTimestamp { get; set; }
}
