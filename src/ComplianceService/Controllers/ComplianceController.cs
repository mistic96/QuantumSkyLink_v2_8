using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComplianceService.Models.Requests;
using ComplianceService.Models.Responses;
using ComplianceService.Services.Interfaces;

namespace ComplianceService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceService _complianceService;
    private readonly ILogger<ComplianceController> _logger;

    public ComplianceController(IComplianceService complianceService, ILogger<ComplianceController> logger)
    {
        _complianceService = complianceService;
        _logger = logger;
    }

    #region KYC Endpoints

    [HttpPost("kyc/initiate")]
    public async Task<ActionResult<KycStatusResponse>> InitiateKyc([FromBody] InitiateKycRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.InitiateKycAsync(userId, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating KYC verification");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("kyc/status")]
    public async Task<ActionResult<KycStatusResponse>> GetKycStatus([FromQuery] Guid? verificationId = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.GetKycStatusAsync(userId, verificationId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KYC status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("kyc/history")]
    public async Task<ActionResult<KycListResponse>> GetKycHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.GetUserKycHistoryAsync(userId, page, pageSize);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting KYC history");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("kyc/compliance")]
    public async Task<ActionResult<bool>> CheckKycCompliance([FromQuery] string? requiredLevel = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isCompliant = await _complianceService.IsUserKycCompliantAsync(userId, requiredLevel);
            return Ok(new { isCompliant, requiredLevel });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking KYC compliance");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region Case Management Endpoints

    [HttpPost("cases")]
    public async Task<ActionResult<CaseResponse>> CreateCase([FromBody] CreateCaseRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.CreateCaseAsync(userId, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating compliance case");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("cases/{caseId}")]
    public async Task<ActionResult<CaseResponse>> GetCase(Guid caseId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.GetCaseAsync(caseId, userId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance case {CaseId}", caseId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("cases")]
    public async Task<ActionResult<CaseListResponse>> GetUserCases(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.GetUserCasesAsync(userId, status, page, pageSize);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user cases");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("cases/{caseId}/documents")]
    public async Task<ActionResult<CaseResponse>> SubmitCaseDocument(Guid caseId, [FromBody] SubmitCaseDocumentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.SubmitCaseDocumentAsync(caseId, userId, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting case document for case {CaseId}", caseId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("cases/{caseId}/review")]
    public async Task<ActionResult<CaseResponse>> ReviewCase(Guid caseId, [FromBody] ReviewCaseRequest request)
    {
        try
        {
            var reviewerId = GetCurrentUserId();
            var response = await _complianceService.ReviewCaseAsync(caseId, reviewerId, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing case {CaseId}", caseId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region Compliance Status Endpoints

    [HttpGet("status")]
    public async Task<ActionResult<ComplianceStatusResponse>> GetComplianceStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _complianceService.GetComplianceStatusAsync(userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateComplianceRequirements([FromBody] ValidateComplianceRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isValid = await _complianceService.ValidateComplianceRequirementsAsync(userId, request.OperationType, request.Amount);
            return Ok(new { isValid, operationType = request.OperationType, amount = request.Amount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating compliance requirements");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region Administrative Endpoints

    [HttpGet("admin/cases/review")]
    [Authorize(Roles = "ComplianceOfficer,Admin")]
    public async Task<ActionResult<CaseListResponse>> GetCasesForReview(
        [FromQuery] string reviewType = "ComplianceOfficer",
        [FromQuery] string? priority = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var response = await _complianceService.GetCasesForReviewAsync(reviewType, priority, page, pageSize);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cases for review");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("admin/events")]
    [Authorize(Roles = "ComplianceOfficer,Admin")]
    public async Task<ActionResult> GetComplianceEvents(
        [FromQuery] Guid? userId = null,
        [FromQuery] string? eventType = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var events = await _complianceService.GetComplianceEventsAsync(userId, eventType, severity, fromDate, toDate, page, pageSize);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance events");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("admin/kyc/{verificationId}/status")]
    [Authorize(Roles = "ComplianceOfficer,Admin")]
    public async Task<ActionResult> UpdateKycStatus(Guid verificationId, [FromBody] UpdateKycStatusAdminRequest request)
    {
        try
        {
            await _complianceService.UpdateKycStatusAsync(verificationId, request.Status, request.FailureReason, request.RiskScore);
            return Ok(new { message = "KYC status updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating KYC status for verification {VerificationId}", verificationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }

    private string GetCorrelationId()
    {
        return HttpContext.TraceIdentifier ?? Guid.NewGuid().ToString();
    }

    private string GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return HttpContext.Request.Headers.UserAgent.ToString() ?? "Unknown";
    }

    #endregion
}

// Additional request models for controller endpoints
public class ValidateComplianceRequest
{
    public string OperationType { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
}

public class UpdateKycStatusAdminRequest
{
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public decimal? RiskScore { get; set; }
}
