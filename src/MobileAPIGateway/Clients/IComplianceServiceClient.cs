using Refit;
using MobileAPIGateway.Models.Compliance;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Client interface for ComplianceService integration
/// </summary>
public interface IComplianceServiceClient
{
    /// <summary>
    /// Get user compliance status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User compliance status</returns>
    [Get("/api/compliance/user/{userId}/status")]
    Task<UserComplianceStatusResponse> GetUserComplianceStatusAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user KYC status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>KYC status information</returns>
    [Get("/api/compliance/user/{userId}/kyc")]
    Task<KycStatusResponse> GetKycStatusAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check transaction compliance
    /// </summary>
    /// <param name="request">Transaction compliance check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction compliance result</returns>
    [Post("/api/compliance/transaction/check")]
    Task<TransactionComplianceResponse> CheckTransactionComplianceAsync(
        [Body] TransactionComplianceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance alerts for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of compliance alerts</returns>
    [Get("/api/compliance/user/{userId}/alerts")]
    Task<IEnumerable<ComplianceAlertResponse>> GetComplianceAlertsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance requirements for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of compliance requirements</returns>
    [Get("/api/compliance/user/{userId}/requirements")]
    Task<IEnumerable<ComplianceRequirementResponse>> GetComplianceRequirementsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit compliance document
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Document submission request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document submission result</returns>
    [Post("/api/compliance/user/{userId}/documents")]
    Task<DocumentSubmissionResponse> SubmitComplianceDocumentAsync(
        Guid userId,
        [Body] DocumentSubmissionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance document status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document status</returns>
    [Get("/api/compliance/user/{userId}/documents/{documentId}")]
    Task<DocumentStatusResponse> GetDocumentStatusAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance summary for mobile dashboard
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance summary</returns>
    [Get("/api/compliance/user/{userId}/summary")]
    Task<ComplianceSummaryResponse> GetComplianceSummaryAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark compliance alert as read
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="alertId">Alert ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [Put("/api/compliance/user/{userId}/alerts/{alertId}/read")]
    Task<bool> MarkAlertAsReadAsync(
        Guid userId,
        Guid alertId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance history for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance history</returns>
    [Get("/api/compliance/user/{userId}/history")]
    Task<ComplianceHistoryResponse> GetComplianceHistoryAsync(
        Guid userId,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);
}
