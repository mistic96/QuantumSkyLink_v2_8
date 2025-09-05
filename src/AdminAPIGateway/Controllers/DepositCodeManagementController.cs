using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminAPIGateway.Services;
using System.ComponentModel.DataAnnotations;

namespace AdminAPIGateway.Controllers;

/// <summary>
/// Admin controller for managing duplicate deposit codes and review workflows
/// </summary>
[ApiController]
[Route("api/admin/deposit-codes")]
[Authorize(Policy = "AdminOnly")]
public class DepositCodeManagementController : ControllerBase
{
    private readonly IPaymentGatewayClient _paymentGatewayClient;
    private readonly ILogger<DepositCodeManagementController> _logger;

    public DepositCodeManagementController(
        IPaymentGatewayClient paymentGatewayClient,
        ILogger<DepositCodeManagementController> logger)
    {
        _paymentGatewayClient = paymentGatewayClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all deposit codes pending admin review (duplicates, suspicious activity)
    /// </summary>
    [HttpGet("pending-review")]
    [ProducesResponseType(typeof(PaginatedResponse<DepositCodeReviewItem>), 200)]
    public async Task<IActionResult> GetPendingReviewCodes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] string? filterReason = null)
    {
        try
        {
            var response = await _paymentGatewayClient.GetPendingReviewCodesAsync(
                page, pageSize, sortBy, sortDescending, filterReason);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending review codes");
            return StatusCode(500, new { error = "Failed to retrieve pending review codes" });
        }
    }

    /// <summary>
    /// Get detailed information about a specific deposit code under review
    /// </summary>
    [HttpGet("review/{depositCodeId}")]
    [ProducesResponseType(typeof(DepositCodeReviewDetail), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDepositCodeReviewDetail(Guid depositCodeId)
    {
        try
        {
            var detail = await _paymentGatewayClient.GetDepositCodeReviewDetailAsync(depositCodeId);
            
            if (detail == null)
            {
                return NotFound(new { error = "Deposit code not found or not under review" });
            }
            
            return Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deposit code review detail for {DepositCodeId}", depositCodeId);
            return StatusCode(500, new { error = "Failed to retrieve deposit code details" });
        }
    }

    /// <summary>
    /// Approve a deposit code that was held for review
    /// </summary>
    [HttpPost("review/{depositCodeId}/approve")]
    [ProducesResponseType(typeof(DepositCodeApprovalResult), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ApproveDepositCode(
        Guid depositCodeId,
        [FromBody] DepositCodeApprovalRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var adminId = User.FindFirst("sub")?.Value ?? "system";
            request.ApprovedBy = adminId;
            request.ApprovedAt = DateTime.UtcNow;
            
            var result = await _paymentGatewayClient.ApproveDepositCodeAsync(depositCodeId, request);
            
            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
            
            _logger.LogInformation("Deposit code {DepositCodeId} approved by {AdminId}", depositCodeId, adminId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving deposit code {DepositCodeId}", depositCodeId);
            return StatusCode(500, new { error = "Failed to approve deposit code" });
        }
    }

    /// <summary>
    /// Reject a deposit code and initiate refund process
    /// </summary>
    [HttpPost("review/{depositCodeId}/reject")]
    [ProducesResponseType(typeof(DepositCodeRejectionResult), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RejectDepositCode(
        Guid depositCodeId,
        [FromBody] DepositCodeRejectionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var adminId = User.FindFirst("sub")?.Value ?? "system";
            request.RejectedBy = adminId;
            request.RejectedAt = DateTime.UtcNow;
            
            var result = await _paymentGatewayClient.RejectDepositCodeAsync(depositCodeId, request);
            
            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
            
            _logger.LogInformation("Deposit code {DepositCodeId} rejected by {AdminId}. Refund initiated: {RefundInitiated}", 
                depositCodeId, adminId, result.RefundInitiated);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting deposit code {DepositCodeId}", depositCodeId);
            return StatusCode(500, new { error = "Failed to reject deposit code" });
        }
    }

    /// <summary>
    /// Perform bulk actions on multiple deposit codes
    /// </summary>
    [HttpPost("bulk-action")]
    [ProducesResponseType(typeof(BulkActionResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> BulkDepositCodeAction([FromBody] BulkDepositCodeActionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.DepositCodeIds.Count == 0)
        {
            return BadRequest(new { error = "No deposit codes specified" });
        }

        if (request.DepositCodeIds.Count > 100)
        {
            return BadRequest(new { error = "Maximum 100 deposit codes can be processed at once" });
        }

        try
        {
            var adminId = User.FindFirst("sub")?.Value ?? "system";
            request.PerformedBy = adminId;
            request.PerformedAt = DateTime.UtcNow;
            
            var result = await _paymentGatewayClient.BulkDepositCodeActionAsync(request);
            
            _logger.LogInformation("Bulk action {Action} performed on {Count} deposit codes by {AdminId}. Success: {SuccessCount}, Failed: {FailedCount}", 
                request.Action, request.DepositCodeIds.Count, adminId, result.SuccessCount, result.FailedCount);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk action on deposit codes");
            return StatusCode(500, new { error = "Failed to perform bulk action" });
        }
    }

    /// <summary>
    /// Get deposit code duplicate patterns and statistics
    /// </summary>
    [HttpGet("duplicate-statistics")]
    [ProducesResponseType(typeof(DuplicateStatistics), 200)]
    public async Task<IActionResult> GetDuplicateStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            
            if (start > end)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }
            
            var statistics = await _paymentGatewayClient.GetDuplicateStatisticsAsync(start, end);
            
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving duplicate statistics");
            return StatusCode(500, new { error = "Failed to retrieve duplicate statistics" });
        }
    }

    /// <summary>
    /// Search deposit codes with advanced filters
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(PaginatedResponse<DepositCodeSearchResult>), 200)]
    public async Task<IActionResult> SearchDepositCodes([FromBody] DepositCodeSearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var results = await _paymentGatewayClient.SearchDepositCodesAsync(request);
            
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching deposit codes");
            return StatusCode(500, new { error = "Failed to search deposit codes" });
        }
    }

    /// <summary>
    /// Get audit trail for a specific deposit code
    /// </summary>
    [HttpGet("{depositCodeId}/audit-trail")]
    [ProducesResponseType(typeof(List<DepositCodeAuditEntry>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDepositCodeAuditTrail(Guid depositCodeId)
    {
        try
        {
            var auditTrail = await _paymentGatewayClient.GetDepositCodeAuditTrailAsync(depositCodeId);
            
            if (auditTrail == null || !auditTrail.Any())
            {
                return NotFound(new { error = "No audit trail found for deposit code" });
            }
            
            return Ok(auditTrail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for deposit code {DepositCodeId}", depositCodeId);
            return StatusCode(500, new { error = "Failed to retrieve audit trail" });
        }
    }

    /// <summary>
    /// Export deposit code data for reporting
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ExportDepositCodes([FromBody] DepositCodeExportRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var exportData = await _paymentGatewayClient.ExportDepositCodesAsync(request);
            
            var contentType = request.Format switch
            {
                ExportFormat.CSV => "text/csv",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.JSON => "application/json",
                _ => "text/csv"
            };
            
            var fileName = $"deposit_codes_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{request.Format.ToString().ToLower()}";
            
            return File(exportData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting deposit codes");
            return StatusCode(500, new { error = "Failed to export deposit codes" });
        }
    }
}

/// <summary>
/// Models for deposit code management
/// </summary>
public class DepositCodeReviewItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReviewReason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? HeldSince { get; set; }
    public int DuplicateCount { get; set; }
}

public class DepositCodeReviewDetail
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReviewReason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public UserInfo? User { get; set; }
    public PaymentInfo? AssociatedPayment { get; set; }
    public List<DepositCodeDuplicate> Duplicates { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? HeldSince { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string KycStatus { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}

public class PaymentInfo
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DepositCodeDuplicate
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DepositCodeApprovalRequest
{
    [Required]
    public string ApprovalReason { get; set; } = string.Empty;
    
    public string? ApprovalNotes { get; set; }
    
    public bool ProcessDuplicatesAsWell { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class DepositCodeApprovalResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid DepositCodeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool PaymentProcessed { get; set; }
    public int DuplicatesProcessed { get; set; }
}

public class DepositCodeRejectionRequest
{
    [Required]
    public string RejectionReason { get; set; } = string.Empty;
    
    public string? RejectionNotes { get; set; }
    
    public bool InitiateRefund { get; set; } = true;
    
    public string? CustomerCommunication { get; set; }
    
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
}

public class DepositCodeRejectionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid DepositCodeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool RefundInitiated { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundTransactionId { get; set; }
}

public class BulkDepositCodeActionRequest
{
    [Required]
    public List<Guid> DepositCodeIds { get; set; } = new();
    
    [Required]
    public BulkAction Action { get; set; }
    
    public string? Reason { get; set; }
    
    public string? Notes { get; set; }
    
    public string? PerformedBy { get; set; }
    public DateTime? PerformedAt { get; set; }
}

public enum BulkAction
{
    Approve,
    Reject,
    Hold,
    Release,
    MarkReviewed
}

public class BulkActionResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkActionItemResult> Results { get; set; } = new();
}

public class BulkActionItemResult
{
    public Guid DepositCodeId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DuplicateStatistics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDuplicates { get; set; }
    public int UniqueDuplicateSets { get; set; }
    public int ResolvedDuplicates { get; set; }
    public int PendingDuplicates { get; set; }
    public List<DuplicatePattern> TopPatterns { get; set; } = new();
    public Dictionary<string, int> DuplicatesByStatus { get; set; } = new();
}

public class DuplicatePattern
{
    public string Pattern { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> Examples { get; set; } = new();
}

public class DepositCodeSearchRequest
{
    public string? Code { get; set; }
    public List<string>? Statuses { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Currency { get; set; }
    public bool? HasDuplicates { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "createdAt";
    public bool SortDescending { get; set; } = true;
}

public class DepositCodeSearchResult
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool HasDuplicates { get; set; }
}

public class DepositCodeAuditEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Dictionary<string, object>? Changes { get; set; }
}

public class DepositCodeExportRequest
{
    public DepositCodeSearchRequest SearchCriteria { get; set; } = new();
    public ExportFormat Format { get; set; } = ExportFormat.CSV;
    public List<string> IncludeFields { get; set; } = new();
}

public enum ExportFormat
{
    CSV,
    Excel,
    JSON
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
