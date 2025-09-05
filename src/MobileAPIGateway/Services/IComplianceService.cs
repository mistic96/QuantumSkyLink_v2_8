using MobileAPIGateway.Models.Compliance;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service interface for mobile compliance operations
/// </summary>
public interface IComplianceService
{
    /// <summary>
    /// Get user compliance status for mobile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User compliance status</returns>
    Task<UserComplianceStatusResponse> GetUserComplianceStatusAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user KYC status for mobile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>KYC status information</returns>
    Task<KycStatusResponse> GetKycStatusAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check transaction compliance for mobile
    /// </summary>
    /// <param name="request">Transaction compliance check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction compliance result</returns>
    Task<TransactionComplianceResponse> CheckTransactionComplianceAsync(
        TransactionComplianceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance alerts for mobile user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of compliance alerts</returns>
    Task<IEnumerable<ComplianceAlertResponse>> GetComplianceAlertsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance requirements for mobile user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of compliance requirements</returns>
    Task<IEnumerable<ComplianceRequirementResponse>> GetComplianceRequirementsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit compliance document from mobile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Document submission request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document submission result</returns>
    Task<DocumentSubmissionResponse> SubmitComplianceDocumentAsync(
        Guid userId,
        DocumentSubmissionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance document status for mobile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document status</returns>
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
    Task<ComplianceSummaryResponse> GetComplianceSummaryAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark compliance alert as read from mobile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="alertId">Alert ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> MarkAlertAsReadAsync(
        Guid userId,
        Guid alertId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance history for mobile user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance history</returns>
    Task<ComplianceHistoryResponse> GetComplianceHistoryAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
