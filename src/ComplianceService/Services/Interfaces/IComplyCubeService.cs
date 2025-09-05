using ComplianceService.Models.ComplyCube;
using Refit;

namespace ComplianceService.Services.Interfaces;

public interface IComplyCubeService
{
    // Client Management
    [Post("/clients")]
    Task<ComplyCubeClientResponse> CreateClientAsync([Body] ComplyCubeClientCreateRequest request);

    [Get("/clients/{clientId}")]
    Task<ComplyCubeClientResponse> GetClientAsync(string clientId);

    [Put("/clients/{clientId}")]
    Task<ComplyCubeClientResponse> UpdateClientAsync(string clientId, [Body] ComplyCubeClientCreateRequest request);

    // Check Management
    [Post("/checks")]
    Task<ComplyCubeCheckResponse> CreateCheckAsync([Body] ComplyCubeCheckCreateRequest request);

    [Get("/checks/{checkId}")]
    Task<ComplyCubeCheckResponse> GetCheckAsync(string checkId);

    [Get("/clients/{clientId}/checks")]
    Task<List<ComplyCubeCheckResponse>> GetClientChecksAsync(string clientId);

    // Risk Assessment
    [Post("/risk-assessments")]
    Task<RiskAssessmentResponse> CreateRiskAssessmentAsync([Body] RiskAssessmentRequest request);

    [Get("/risk-assessments/{assessmentId}")]
    Task<RiskAssessmentResponse> GetRiskAssessmentAsync(string assessmentId);

    // Webhook Management
    [Post("/webhooks")]
    Task<WebhookResponse> CreateWebhookAsync([Body] CreateWebhookRequest request);

    [Get("/webhooks")]
    Task<List<WebhookResponse>> GetWebhooksAsync();

    [Delete("/webhooks/{webhookId}")]
    Task DeleteWebhookAsync(string webhookId);
}

// Additional models for webhook management
public class CreateWebhookRequest
{
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new(); // check.completed, check.failed, review.updated
    public bool IsActive { get; set; } = true;
}

public class WebhookResponse
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Service client interfaces for integration with other services
public interface IUserServiceClient
{
    [Get("/api/users/{userId}")]
    Task<UserResponse> GetUserAsync(Guid userId);

    [Put("/api/users/{userId}/kyc-status")]
    Task UpdateUserKycStatusAsync(Guid userId, [Body] UpdateKycStatusRequest request);
}

public interface IAccountServiceClient
{
    [Get("/api/accounts/user/{userId}")]
    Task<List<AccountResponse>> GetUserAccountsAsync(Guid userId);

    [Get("/api/accounts/{accountId}")]
    Task<AccountResponse> GetAccountAsync(Guid accountId);
}

public interface ISecurityServiceClient
{
    [Post("/api/security/events")]
    Task LogSecurityEventAsync([Body] LogSecurityEventRequest request);

    [Get("/api/security/status/{userId}")]
    Task<SecurityStatusResponse> GetUserSecurityStatusAsync(Guid userId);
}

public interface INotificationServiceClient
{
    [Post("/api/notifications/email")]
    Task SendEmailAsync([Body] SendEmailRequest request);

    [Post("/api/notifications/sms")]
    Task SendSmsAsync([Body] SendSmsRequest request);
}

// Response models for service integration
public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;
    public string KycStatus { get; set; } = string.Empty;
}

public class AccountResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class UpdateKycStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? KycLevel { get; set; }
    public decimal? RiskScore { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class LogSecurityEventRequest
{
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public object? EventData { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class SecurityStatusResponse
{
    public Guid UserId { get; set; }
    public bool IsCompliant { get; set; }
    public Dictionary<string, object> Status { get; set; } = new();
}

public class SendEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
}

public class SendSmsRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public Dictionary<string, object>? TemplateData { get; set; }
}
