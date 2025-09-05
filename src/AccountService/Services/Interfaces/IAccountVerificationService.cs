using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;

namespace AccountService.Services.Interfaces;

public interface IAccountVerificationService
{
    // Verification Management
    Task<AccountVerificationResponse> CreateVerificationAsync(CreateAccountVerificationRequest request, CancellationToken cancellationToken = default);
    Task<AccountVerificationResponse> GetVerificationAsync(Guid verificationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountVerificationResponse>> GetAccountVerificationsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountVerificationResponse>> GetUserVerificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AccountVerificationResponse?> GetLatestVerificationAsync(Guid accountId, VerificationType verificationType, CancellationToken cancellationToken = default);

    // Verification Status Management
    Task<AccountVerificationResponse> UpdateVerificationStatusAsync(Guid verificationId, VerificationStatus status, string? notes = null, CancellationToken cancellationToken = default);
    Task<AccountVerificationResponse> ApproveVerificationAsync(Guid verificationId, string approvedBy, string? notes = null, CancellationToken cancellationToken = default);
    Task<AccountVerificationResponse> RejectVerificationAsync(Guid verificationId, string rejectedBy, string reason, CancellationToken cancellationToken = default);

    // Document Management
    Task<AccountVerificationResponse> UploadDocumentAsync(Guid verificationId, string documentUrl, string documentType, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(Guid verificationId, CancellationToken cancellationToken = default);

    // Verification Validation
    Task<bool> VerificationExistsAsync(Guid verificationId, CancellationToken cancellationToken = default);
    Task<bool> UserOwnsVerificationAsync(Guid userId, Guid verificationId, CancellationToken cancellationToken = default);
    Task<bool> IsAccountVerifiedAsync(Guid accountId, VerificationType verificationType, CancellationToken cancellationToken = default);
    Task<bool> IsAccountFullyVerifiedAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Verification Requirements
    Task<IEnumerable<VerificationRequirementResponse>> GetVerificationRequirementsAsync(AccountType accountType, CancellationToken cancellationToken = default);
    Task<VerificationStatusSummaryResponse> GetAccountVerificationStatusAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountVerificationResponse>> GetPendingVerificationsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    // KYC Processing
    Task<AccountVerificationResponse> InitiateKycVerificationAsync(Guid accountId, KycVerificationRequest request, CancellationToken cancellationToken = default);
    Task<AccountVerificationResponse> ProcessKycResultAsync(Guid verificationId, KycResultRequest request, CancellationToken cancellationToken = default);
    Task<KycStatusResponse> GetKycStatusAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Compliance Checks
    Task<ComplianceCheckResponse> PerformComplianceCheckAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<ComplianceCheckResponse> PerformComplianceCheckAsync(Guid accountId, string checkType, Dictionary<string, object>? additionalData, CancellationToken cancellationToken = default);
    Task<bool> IsAccountCompliantAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplianceIssueResponse>> GetComplianceIssuesAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Additional KYC Workflow Methods
    Task<KycWorkflowResponse> InitiateKycWorkflowAsync(Guid accountId, string kycLevel, CancellationToken cancellationToken = default);
    Task<KycWorkflowResponse> GetKycWorkflowStatusAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<KycProcessingResultResponse> ProcessKycResultsAsync(Guid accountId, Dictionary<string, object> kycResults, Dictionary<string, object>? complianceData, CancellationToken cancellationToken = default);

    // Additional Status and Compliance Methods
    Task<ComplianceStatusResponse> GetComplianceStatusAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<VerificationStatusSummaryResponse> GetVerificationStatusSummaryAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<VerificationRequirementsResponse> GetVerificationRequirementsAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Document Management with Extended Signatures
    Task<DocumentUploadResponse> UploadDocumentAsync(Guid verificationId, string documentType, byte[] documentData, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(Guid verificationId, Guid documentId, CancellationToken cancellationToken = default);
}
