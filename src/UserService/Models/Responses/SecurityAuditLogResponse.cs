using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class SecurityAuditLogResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [StringLength(50)]
    public string Severity { get; set; } = string.Empty;

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
    public string? Result { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    [StringLength(100)]
    public string? SessionId { get; set; }

    [StringLength(100)]
    public string? DeviceId { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    public DateTime EventTimestamp { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(100)]
    public string? UserEmail { get; set; }

    [StringLength(200)]
    public string? UserName { get; set; }

    public bool IsHighRisk { get; set; }

    public bool RequiresReview { get; set; }
}
