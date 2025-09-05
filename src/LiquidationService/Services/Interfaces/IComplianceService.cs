using LiquidationService.Models.Responses;

namespace LiquidationService.Services.Interfaces;

/// <summary>
/// Service interface for compliance checks and risk management
/// </summary>
public interface IComplianceService
{
    /// <summary>
    /// Perform KYC verification for a liquidation request
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check response</returns>
    Task<ComplianceCheckResponse> PerformKycCheckAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform AML (Anti-Money Laundering) check
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="amount">Transaction amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check response</returns>
    Task<ComplianceCheckResponse> PerformAmlCheckAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        decimal amount, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform sanctions screening
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check response</returns>
    Task<ComplianceCheckResponse> PerformSanctionsScreeningAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform risk assessment for a liquidation request
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="assetSymbol">Asset being liquidated</param>
    /// <param name="amount">Amount being liquidated</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check response</returns>
    Task<ComplianceCheckResponse> PerformRiskAssessmentAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        string assetSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance check by ID
    /// </summary>
    /// <param name="checkId">Compliance check ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance check response</returns>
    Task<ComplianceCheckResponse?> GetComplianceCheckAsync(
        Guid checkId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all compliance checks for a liquidation request
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of compliance checks</returns>
    Task<IEnumerable<ComplianceCheckResponse>> GetComplianceChecksForRequestAsync(
        Guid liquidationRequestId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Override a compliance check result (admin function)
    /// </summary>
    /// <param name="checkId">Compliance check ID</param>
    /// <param name="newResult">New compliance result</param>
    /// <param name="reason">Override reason</param>
    /// <param name="reviewedBy">Admin user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated compliance check response</returns>
    Task<ComplianceCheckResponse> OverrideComplianceCheckAsync(
        Guid checkId, 
        string newResult, 
        string reason, 
        Guid reviewedBy, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a liquidation request passes all compliance requirements
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Overall compliance status</returns>
    Task<bool> IsComplianceApprovedAsync(
        Guid liquidationRequestId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance statistics for reporting
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compliance statistics</returns>
    Task<object> GetComplianceStatisticsAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform comprehensive compliance check (all checks combined)
    /// </summary>
    /// <param name="liquidationRequestId">Liquidation request ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="assetSymbol">Asset being liquidated</param>
    /// <param name="amount">Amount being liquidated</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all compliance check results</returns>
    Task<IEnumerable<ComplianceCheckResponse>> PerformComprehensiveComplianceCheckAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        string assetSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending compliance checks requiring manual review
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated compliance checks requiring review</returns>
    Task<PaginatedResponse<ComplianceCheckResponse>> GetPendingReviewsAsync(
        int page = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default);
}
