using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class SecurityController : ControllerBase
{
    private readonly ISecurityAuditService _securityAuditService;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(ISecurityAuditService securityAuditService, ILogger<SecurityController> logger)
    {
        _securityAuditService = securityAuditService;
        _logger = logger;
    }

    /// <summary>
    /// Log security event
    /// </summary>
    /// <param name="request">Security event details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created audit log entry</returns>
    [HttpPost("audit")]
    [ProducesResponseType(typeof(SecurityAuditLogResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SecurityAuditLogResponse>> LogSecurityEvent(
        [FromBody] CreateSecurityAuditLogRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var auditLog = await _securityAuditService.CreateSecurityAuditLogAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetSecurityAuditLog), new { id = auditLog.Id }, auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging security event");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get security audit log by ID
    /// </summary>
    /// <param name="id">Audit log ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Security audit log entry</returns>
    [HttpGet("audit/{id:guid}")]
    [ProducesResponseType(typeof(SecurityAuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SecurityAuditLogResponse>> GetSecurityAuditLog(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _securityAuditService.GetSecurityAuditLogAsync(id, cancellationToken);
            return Ok(auditLog);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security audit log {AuditLogId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get security audit logs for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of security audit logs</returns>
    [HttpGet("audit/user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<SecurityAuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SecurityAuditLogResponse>>> GetUserSecurityAuditLogs(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _securityAuditService.GetUserSecurityAuditLogsAsync(userId, cancellationToken);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security audit logs for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get security audit logs by event type
    /// </summary>
    /// <param name="eventType">Event type</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of security audit logs</returns>
    [HttpGet("audit/events/{eventType}")]
    [ProducesResponseType(typeof(IEnumerable<SecurityAuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SecurityAuditLogResponse>>> GetSecurityAuditLogsByEventType(
        string eventType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert string to SecurityEventType enum
            if (!Enum.TryParse<UserService.Models.Security.SecurityEventType>(eventType, true, out var parsedEventType))
            {
                return BadRequest(new { message = "Invalid event type" });
            }
            
            var auditLogs = await _securityAuditService.GetSecurityAuditLogsByEventTypeAsync(parsedEventType, cancellationToken);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security audit logs for event type {EventType}", eventType);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get security audit logs within date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of security audit logs</returns>
    [HttpGet("audit/range")]
    [ProducesResponseType(typeof(IEnumerable<SecurityAuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SecurityAuditLogResponse>>> GetSecurityAuditLogsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest(new { message = "Start date must be before end date" });
            }

            var auditLogs = await _securityAuditService.GetSecurityAuditLogsByDateRangeAsync(startDate, endDate, cancellationToken);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security audit logs for date range {StartDate} to {EndDate}", startDate, endDate);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get security statistics
    /// </summary>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Security statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(SecurityStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SecurityStatisticsResponse>> GetSecurityStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
            {
                return BadRequest(new { message = "Start date must be before end date" });
            }

            var statistics = await _securityAuditService.GetSecurityStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security statistics");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get failed login attempts for user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="hours">Number of hours to look back (default: 24)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of failed login attempts</returns>
    [HttpGet("failed-logins/{userId:guid}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetFailedLoginAttempts(
        Guid userId,
        [FromQuery] int hours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var failedAttempts = await _securityAuditService.GetFailedLoginAttemptsAsync(userId, cancellationToken);
            return Ok(failedAttempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed login attempts for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Check if user account should be locked
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account lock status</returns>
    [HttpGet("account-lock-status/{userId:guid}")]
    [ProducesResponseType(typeof(AccountLockStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountLockStatusResponse>> GetAccountLockStatus(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lockStatus = await _securityAuditService.ShouldLockAccountAsync(userId, cancellationToken);
            return Ok(new AccountLockStatusResponse { ShouldLock = lockStatus });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking account lock status for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
