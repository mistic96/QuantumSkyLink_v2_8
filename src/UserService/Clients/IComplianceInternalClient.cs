using Refit;

namespace UserService.Clients;

/// <summary>
/// Internal client for compliance service communication
/// </summary>
public interface IComplianceInternalClient
{
    /// <summary>
    /// Validates user compliance status
    /// </summary>
    [Post("/api/internal/compliance/validate")]
    Task<bool> ValidateUserComplianceAsync([Body] ComplianceValidationRequest request);
    
    /// <summary>
    /// Records user compliance event
    /// </summary>
    [Post("/api/internal/compliance/events")]
    Task RecordComplianceEventAsync([Body] ComplianceEventRequest request);
}

public class ComplianceValidationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ComplianceEventRequest
{
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}
