namespace UserService.Models.Security;

public class SecurityAuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public SecurityEventType EventType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public AuditLogLevel Level { get; set; }
    public string? Details { get; set; }
    public string? CorrelationId { get; set; }
    public string? SessionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? PerformedBy { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}
