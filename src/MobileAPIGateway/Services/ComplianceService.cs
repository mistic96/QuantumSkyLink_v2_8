using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Compliance;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service implementation for mobile compliance operations
/// </summary>
public class ComplianceService : IComplianceService
{
    private readonly IComplianceServiceClient _complianceClient;
    private readonly ILogger<ComplianceService> _logger;

    public ComplianceService(
        IComplianceServiceClient complianceClient,
        ILogger<ComplianceService> logger)
    {
        _complianceClient = complianceClient;
        _logger = logger;
    }

    /// <summary>
    /// Get user compliance status for mobile
    /// </summary>
    public async Task<UserComplianceStatusResponse> GetUserComplianceStatusAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting compliance status for user {UserId}", userId);
            
            var response = await _complianceClient.GetUserComplianceStatusAsync(userId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved compliance status for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance status for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get user KYC status for mobile
    /// </summary>
    public async Task<KycStatusResponse> GetKycStatusAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting KYC status for user {UserId}", userId);
            
            var response = await _complianceClient.GetKycStatusAsync(userId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved KYC status for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KYC status for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Check transaction compliance for mobile
    /// </summary>
    public async Task<TransactionComplianceResponse> CheckTransactionComplianceAsync(
        TransactionComplianceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking transaction compliance for user {UserId}, amount {Amount}", 
                request.UserId, request.Amount);
            
            var response = await _complianceClient.CheckTransactionComplianceAsync(request, cancellationToken);
            
            _logger.LogInformation("Transaction compliance check completed for user {UserId}, compliant: {IsCompliant}", 
                request.UserId, response.IsCompliant);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking transaction compliance for user {UserId}", request.UserId);
            throw;
        }
    }

    /// <summary>
    /// Get compliance alerts for mobile user
    /// </summary>
    public async Task<IEnumerable<ComplianceAlertResponse>> GetComplianceAlertsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting compliance alerts for user {UserId}", userId);
            
            var response = await _complianceClient.GetComplianceAlertsAsync(userId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved {Count} compliance alerts for user {UserId}", 
                response.Count(), userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance alerts for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get compliance requirements for mobile user
    /// </summary>
    public async Task<IEnumerable<ComplianceRequirementResponse>> GetComplianceRequirementsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting compliance requirements for user {UserId}", userId);
            
            var response = await _complianceClient.GetComplianceRequirementsAsync(userId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved {Count} compliance requirements for user {UserId}", 
                response.Count(), userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance requirements for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Submit compliance document from mobile
    /// </summary>
    public async Task<DocumentSubmissionResponse> SubmitComplianceDocumentAsync(
        Guid userId,
        DocumentSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting compliance document for user {UserId}, type {DocumentType}", 
                userId, request.DocumentType);
            
            var response = await _complianceClient.SubmitComplianceDocumentAsync(userId, request, cancellationToken);
            
            _logger.LogInformation("Successfully submitted compliance document for user {UserId}, document ID {DocumentId}", 
                userId, response.DocumentId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting compliance document for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get compliance document status for mobile
    /// </summary>
    public async Task<DocumentStatusResponse> GetDocumentStatusAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting document status for user {UserId}, document {DocumentId}", 
                userId, documentId);
            
            var response = await _complianceClient.GetDocumentStatusAsync(userId, documentId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved document status for user {UserId}, document {DocumentId}", 
                userId, documentId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document status for user {UserId}, document {DocumentId}", 
                userId, documentId);
            throw;
        }
    }

    /// <summary>
    /// Get compliance summary for mobile dashboard
    /// </summary>
    public async Task<ComplianceSummaryResponse> GetComplianceSummaryAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting compliance summary for user {UserId}", userId);
            
            var response = await _complianceClient.GetComplianceSummaryAsync(userId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved compliance summary for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance summary for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Mark compliance alert as read from mobile
    /// </summary>
    public async Task<bool> MarkAlertAsReadAsync(
        Guid userId,
        Guid alertId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking alert as read for user {UserId}, alert {AlertId}", 
                userId, alertId);
            
            var response = await _complianceClient.MarkAlertAsReadAsync(userId, alertId, cancellationToken);
            
            _logger.LogInformation("Successfully marked alert as read for user {UserId}, alert {AlertId}", 
                userId, alertId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking alert as read for user {UserId}, alert {AlertId}", 
                userId, alertId);
            throw;
        }
    }

    /// <summary>
    /// Get compliance history for mobile user
    /// </summary>
    public async Task<ComplianceHistoryResponse> GetComplianceHistoryAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting compliance history for user {UserId}, page {Page}, size {PageSize}", 
                userId, page, pageSize);
            
            var response = await _complianceClient.GetComplianceHistoryAsync(userId, page, pageSize, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved compliance history for user {UserId}, {Count} items", 
                userId, response.TotalCount);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance history for user {UserId}", userId);
            throw;
        }
    }
}
