using ComplianceService.Models.Requests;
using ComplianceService.Models.Responses;
using ComplianceService.Data.Entities;

namespace ComplianceService.Services.Interfaces;

public interface IComplianceService
{
    // KYC Management
    Task<KycStatusResponse> InitiateKycAsync(Guid userId, InitiateKycRequest request);
    Task<KycStatusResponse> GetKycStatusAsync(Guid userId, Guid? verificationId = null);
    Task<KycListResponse> GetUserKycHistoryAsync(Guid userId, int page = 1, int pageSize = 50);
    Task<bool> IsUserKycCompliantAsync(Guid userId, string? requiredLevel = null);
    Task UpdateKycStatusAsync(Guid verificationId, string status, string? failureReason = null, decimal? riskScore = null);

    // Case Management
    Task<CaseResponse> CreateCaseAsync(Guid userId, CreateCaseRequest request);
    Task<CaseResponse> GetCaseAsync(Guid caseId, Guid userId);
    Task<CaseListResponse> GetUserCasesAsync(Guid userId, string? status = null, int page = 1, int pageSize = 50);
    Task<CaseResponse> SubmitCaseDocumentAsync(Guid caseId, Guid userId, SubmitCaseDocumentRequest request);
    Task<CaseResponse> ReviewCaseAsync(Guid caseId, Guid reviewerId, ReviewCaseRequest request);
    Task<CaseResponse> UpdateCaseStatusAsync(Guid caseId, string status, string? resolution = null);

    // Compliance Status
    Task<ComplianceStatusResponse> GetComplianceStatusAsync(Guid userId);
    Task<bool> ValidateComplianceRequirementsAsync(Guid userId, string operationType, decimal? amount = null);

    // Event Logging
    Task LogComplianceEventAsync(Guid userId, string eventType, string severity, string description, 
        string source, string action, string result, object? eventData, string correlationId, 
        string? ipAddress = null, string? userAgent = null, Guid? kycVerificationId = null, Guid? caseId = null);

    // Webhook Processing
    Task ProcessComplyCubeWebhookAsync(string webhookPayload, string signature);

    // Administrative Functions
    Task<CaseListResponse> GetCasesForReviewAsync(string reviewType, string? priority = null, int page = 1, int pageSize = 50);
    Task<List<ComplianceEvent>> GetComplianceEventsAsync(Guid? userId = null, string? eventType = null, 
        string? severity = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);
}
