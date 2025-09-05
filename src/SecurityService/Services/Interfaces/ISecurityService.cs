using SecurityService.Data.Entities;
using SecurityService.Models.Requests;
using SecurityService.Models.Responses;

namespace SecurityService.Services.Interfaces;

public interface ISecurityService
{
    // Security Policy Management
    Task<SecurityPolicyResponse> CreateSecurityPolicyAsync(Guid userId, CreateSecurityPolicyRequest request, string correlationId);
    Task<SecurityPolicyResponse> GetSecurityPolicyAsync(Guid policyId, Guid userId);
    Task<SecurityPolicyListResponse> GetUserSecurityPoliciesAsync(Guid userId, int page = 1, int pageSize = 50);
    Task<SecurityPolicyResponse> UpdateSecurityPolicyAsync(Guid policyId, Guid userId, CreateSecurityPolicyRequest request, string correlationId);
    Task DeleteSecurityPolicyAsync(Guid policyId, Guid userId, string correlationId);
    Task<bool> ValidateSecurityPolicyAsync(Guid userId, string policyType, object context);

    // Multi-Signature Management
    Task<MultiSignatureResponse> CreateMultiSigRequestAsync(Guid userId, CreateMultiSigRequest request, string correlationId);
    Task<MultiSignatureResponse> GetMultiSigRequestAsync(Guid requestId, Guid userId);
    Task<MultiSignatureListResponse> GetUserMultiSigRequestsAsync(Guid userId, string? status = null, int page = 1, int pageSize = 50);
    Task<MultiSignatureResponse> ApproveMultiSigRequestAsync(Guid userId, ApproveMultiSigRequest request, string correlationId, string ipAddress, string userAgent);
    Task<MultiSignatureResponse> ExecuteMultiSigRequestAsync(Guid requestId, Guid userId, string correlationId);
    Task<bool> ValidateMultiSigRequirementAsync(Guid accountId, string operationType, decimal? amount = null);

    // MFA Management
    Task<string> GenerateMfaTokenAsync(Guid userId, string tokenType, string purpose, string correlationId, string ipAddress, string userAgent);
    Task<bool> ValidateMfaTokenAsync(Guid userId, ValidateMfaRequest request, string correlationId, string ipAddress, string userAgent);
    Task<List<string>> GenerateBackupCodesAsync(Guid userId, string correlationId);
    Task<bool> ValidateBackupCodeAsync(Guid userId, string backupCode, string correlationId, string ipAddress, string userAgent);
    Task InvalidateUserMfaTokensAsync(Guid userId, string tokenType, string correlationId);

    // Security Event Management
    Task LogSecurityEventAsync(Guid userId, string eventType, string severity, string description, string source, string action, string result, object eventData, string correlationId, string ipAddress, string userAgent);
    Task<List<SecurityEvent>> GetUserSecurityEventsAsync(Guid userId, string? eventType = null, string? severity = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);
    Task<List<SecurityEvent>> GetCriticalSecurityEventsAsync(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);
    Task ResolveSecurityEventAsync(Guid eventId, Guid resolvedBy, string resolutionNotes, string correlationId);

    // Security Validation
    Task<bool> ValidateUserAccessAsync(Guid userId, string resource, string action);
    Task<bool> IsUserSecurityComplianceValidAsync(Guid userId);
    Task<Dictionary<string, object>> GetUserSecurityStatusAsync(Guid userId);
}
