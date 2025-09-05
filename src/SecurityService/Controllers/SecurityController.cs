using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityService.Data.Entities;
using SecurityService.Models.Requests;
using SecurityService.Models.Responses;
using SecurityService.Services.Interfaces;

namespace SecurityService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SecurityController : ControllerBase
{
    private readonly ISecurityService _securityService;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(ISecurityService securityService, ILogger<SecurityController> logger)
    {
        _securityService = securityService;
        _logger = logger;
    }

    #region Security Policy Endpoints

    [HttpPost("policies")]
    public async Task<ActionResult<SecurityPolicyResponse>> CreateSecurityPolicy([FromBody] CreateSecurityPolicyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            var policy = await _securityService.CreateSecurityPolicyAsync(userId, request, correlationId);
            return Ok(policy);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security policy");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("policies/{policyId}")]
    public async Task<ActionResult<SecurityPolicyResponse>> GetSecurityPolicy(Guid policyId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var policy = await _securityService.GetSecurityPolicyAsync(policyId, userId);
            return Ok(policy);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security policy {PolicyId}", policyId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("policies")]
    public async Task<ActionResult<SecurityPolicyListResponse>> GetUserSecurityPolicies([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var policies = await _securityService.GetUserSecurityPoliciesAsync(userId, page, pageSize);
            return Ok(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user security policies");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPut("policies/{policyId}")]
    public async Task<ActionResult<SecurityPolicyResponse>> UpdateSecurityPolicy(Guid policyId, [FromBody] CreateSecurityPolicyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            var policy = await _securityService.UpdateSecurityPolicyAsync(policyId, userId, request, correlationId);
            return Ok(policy);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating security policy {PolicyId}", policyId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpDelete("policies/{policyId}")]
    public async Task<ActionResult> DeleteSecurityPolicy(Guid policyId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            await _securityService.DeleteSecurityPolicyAsync(policyId, userId, correlationId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting security policy {PolicyId}", policyId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("policies/validate")]
    public async Task<ActionResult<bool>> ValidateSecurityPolicy([FromBody] ValidateSecurityPolicyRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isValid = await _securityService.ValidateSecurityPolicyAsync(userId, request.PolicyType, request.Context);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating security policy");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region Multi-Signature Endpoints

    [HttpPost("multisig/requests")]
    public async Task<ActionResult<MultiSignatureResponse>> CreateMultiSigRequest([FromBody] CreateMultiSigRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            var multiSigRequest = await _securityService.CreateMultiSigRequestAsync(userId, request, correlationId);
            return Ok(multiSigRequest);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating multi-sig request");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("multisig/requests/{requestId}")]
    public async Task<ActionResult<MultiSignatureResponse>> GetMultiSigRequest(Guid requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = await _securityService.GetMultiSigRequestAsync(requestId, userId);
            return Ok(request);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multi-sig request {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("multisig/requests")]
    public async Task<ActionResult<MultiSignatureListResponse>> GetUserMultiSigRequests([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var requests = await _securityService.GetUserMultiSigRequestsAsync(userId, status, page, pageSize);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user multi-sig requests");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("multisig/approve")]
    public async Task<ActionResult<MultiSignatureResponse>> ApproveMultiSigRequest([FromBody] ApproveMultiSigRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();

            var response = await _securityService.ApproveMultiSigRequestAsync(userId, request, correlationId, ipAddress, userAgent);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving multi-sig request");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("multisig/execute/{requestId}")]
    public async Task<ActionResult<MultiSignatureResponse>> ExecuteMultiSigRequest(Guid requestId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            var response = await _securityService.ExecuteMultiSigRequestAsync(requestId, userId, correlationId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing multi-sig request {RequestId}", requestId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("multisig/validate")]
    public async Task<ActionResult<bool>> ValidateMultiSigRequirement([FromBody] ValidateMultiSigRequirementRequest request)
    {
        try
        {
            var isRequired = await _securityService.ValidateMultiSigRequirementAsync(request.AccountId, request.OperationType, request.Amount);
            return Ok(new { isRequired });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating multi-sig requirement");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region MFA Endpoints

    [HttpPost("mfa/generate")]
    public async Task<ActionResult<string>> GenerateMfaToken([FromBody] GenerateMfaTokenRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();

            var token = await _securityService.GenerateMfaTokenAsync(userId, request.TokenType, request.Purpose, correlationId, ipAddress, userAgent);
            return Ok(new { token });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating MFA token");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("mfa/validate")]
    public async Task<ActionResult<bool>> ValidateMfaToken([FromBody] ValidateMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();

            var isValid = await _securityService.ValidateMfaTokenAsync(userId, request, correlationId, ipAddress, userAgent);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MFA token");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("mfa/backup-codes")]
    public async Task<ActionResult<List<string>>> GenerateBackupCodes()
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            var backupCodes = await _securityService.GenerateBackupCodesAsync(userId, correlationId);
            return Ok(new { backupCodes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("mfa/backup-codes/validate")]
    public async Task<ActionResult<bool>> ValidateBackupCode([FromBody] ValidateBackupCodeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();
            var ipAddress = GetClientIpAddress();
            var userAgent = GetUserAgent();

            var isValid = await _securityService.ValidateBackupCodeAsync(userId, request.BackupCode, correlationId, ipAddress, userAgent);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating backup code");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("mfa/invalidate")]
    public async Task<ActionResult> InvalidateUserMfaTokens([FromBody] InvalidateMfaTokensRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            await _securityService.InvalidateUserMfaTokensAsync(userId, request.TokenType, correlationId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating MFA tokens");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region Security Event Endpoints

    [HttpGet("events")]
    public async Task<ActionResult<List<SecurityEvent>>> GetUserSecurityEvents(
        [FromQuery] string? eventType = null,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var events = await _securityService.GetUserSecurityEventsAsync(userId, eventType, severity, fromDate, toDate, page, pageSize);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user security events");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("events/critical")]
    public async Task<ActionResult<List<SecurityEvent>>> GetCriticalSecurityEvents(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var events = await _securityService.GetCriticalSecurityEventsAsync(fromDate, toDate, page, pageSize);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical security events");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("events/{eventId}/resolve")]
    public async Task<ActionResult> ResolveSecurityEvent(Guid eventId, [FromBody] ResolveSecurityEventRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var correlationId = GetCorrelationId();

            await _securityService.ResolveSecurityEventAsync(eventId, userId, request.ResolutionNotes, correlationId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving security event {EventId}", eventId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    #endregion

    #region Security Validation Endpoints

    [HttpPost("validate/access")]
    public async Task<ActionResult<bool>> ValidateUserAccess([FromBody] ValidateUserAccessRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var hasAccess = await _securityService.ValidateUserAccessAsync(userId, request.Resource, request.Action);
            return Ok(new { hasAccess });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user access");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("compliance")]
    public async Task<ActionResult<bool>> IsUserSecurityComplianceValid()
    {
        try
        {
            var userId = GetCurrentUserId();
            var isCompliant = await _securityService.IsUserSecurityComplianceValidAsync(userId);
            return Ok(new { isCompliant });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user security compliance");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<Dictionary<string, object>>> GetUserSecurityStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var status = await _securityService.GetUserSecurityStatusAsync(userId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user security status");
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
public class ValidateSecurityPolicyRequest
{
    public string PolicyType { get; set; } = string.Empty;
    public object Context { get; set; } = new();
}

public class ValidateMultiSigRequirementRequest
{
    public Guid AccountId { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
}

public class GenerateMfaTokenRequest
{
    public string TokenType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}

public class ValidateBackupCodeRequest
{
    public string BackupCode { get; set; } = string.Empty;
}

public class InvalidateMfaTokensRequest
{
    public string TokenType { get; set; } = string.Empty;
}

public class ResolveSecurityEventRequest
{
    public string ResolutionNotes { get; set; } = string.Empty;
}

public class ValidateUserAccessRequest
{
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
