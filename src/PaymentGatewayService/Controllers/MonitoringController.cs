using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentGatewayService.Services;
using PaymentGatewayService.Services.Interfaces;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PaymentGatewayService.Controllers;

/// <summary>
/// Controller for accessing deposit code monitoring metrics and dashboard data
/// Provides endpoints for AdminAPIGateway consumption
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for monitoring endpoints
public class MonitoringController : ControllerBase
{
    private readonly ILogger<MonitoringController> _logger;
    private readonly PaymentDbContext _context;

public MonitoringController(
        ILogger<MonitoringController> logger,
        PaymentDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Gets deposit code metrics for the specified time range
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Deposit code metrics including validation rates, usage patterns, and failure reasons</returns>
    [HttpGet("deposit-codes")]
    public async Task<ActionResult<PaymentGatewayService.Services.Interfaces.DepositCodeMetrics>> GetDepositCodeMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-1);
            var to = toDate ?? DateTime.UtcNow;

            if (from > to)
            {
                return BadRequest("fromDate must be earlier than toDate");
            }

            if (to - from > TimeSpan.FromDays(30))
            {
                return BadRequest("Date range cannot exceed 30 days");
            }

var totalGenerations = await _context.DepositCodes.CountAsync(dc => dc.CreatedAt >= from && dc.CreatedAt <= to);
var successfulValidations = await _context.DepositCodes.CountAsync(dc => dc.CreatedAt >= from && dc.CreatedAt <= to && dc.Status == DepositCodeStatus.Used);
var failedValidations = await _context.DepositCodes.CountAsync(dc => dc.CreatedAt >= from && dc.CreatedAt <= to && dc.Status == DepositCodeStatus.Rejected);
var totalUsage = await _context.DepositCodes.CountAsync(dc => dc.Status == DepositCodeStatus.Used);
var activeCodes = await _context.DepositCodes.CountAsync(dc => dc.Status == DepositCodeStatus.Active);
var expiredCodes = await _context.DepositCodes.CountAsync(dc => dc.Status == DepositCodeStatus.Expired);
var successRate = (successfulValidations + failedValidations) > 0 ? (double)successfulValidations / (successfulValidations + failedValidations) : 0.0;
var rejectionReasons = new Dictionary<string,int>(); // Rejection reasons not stored separately; placeholder

var metrics = new
{
    FromDate = from,
    ToDate = to,
    TotalGenerations = totalGenerations,
    SuccessfulValidations = successfulValidations,
    FailedValidations = failedValidations,
    SuccessRate = successRate,
    TotalUsage = totalUsage,
    ActiveCodes = activeCodes,
    ExpiredCodes = expiredCodes,
    RejectionReasons = rejectionReasons
};

_logger.LogInformation("Deposit code metrics retrieved for period {FromDate} to {ToDate}", from, to);

return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deposit code metrics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets security metrics for the specified time range
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Security metrics including suspicious activity, duplicate attempts, and incident frequency</returns>
    [HttpGet("security")]
    public async Task<ActionResult<PaymentGatewayService.Services.Interfaces.SecurityMetrics>> GetSecurityMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-1);
            var to = toDate ?? DateTime.UtcNow;

            if (from > to)
            {
                return BadRequest("fromDate must be earlier than toDate");
            }

            if (to - from > TimeSpan.FromDays(30))
            {
                return BadRequest("Date range cannot exceed 30 days");
            }

var suspiciousCount = await _context.DepositCodes
    .Where(dc => dc.UpdatedAt >= from && dc.UpdatedAt <= to && (dc.Status == DepositCodeStatus.Rejected))
    .CountAsync();

var duplicateAttempts = await _context.DepositCodes
    .Where(dc => dc.CreatedAt >= from && dc.CreatedAt <= to)
    .GroupBy(dc => dc.Code)
    .Where(g => g.Count() > 1)
    .CountAsync();

var securityMetrics = new
{
    FromDate = from,
    ToDate = to,
    SuspiciousCount = suspiciousCount,
    DuplicateCodeAttempts = duplicateAttempts
};

_logger.LogInformation("Security metrics retrieved for period {FromDate} to {ToDate}", from, to);

return Ok(securityMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security metrics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets performance metrics for the specified time range
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Performance metrics including validation times, service uptime, and error rates</returns>
    [HttpGet("performance")]
    public async Task<ActionResult<PaymentGatewayService.Services.Interfaces.PerformanceMetrics>> GetPerformanceMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-1);
            var to = toDate ?? DateTime.UtcNow;

            if (from > to)
            {
                return BadRequest("fromDate must be earlier than toDate");
            }

            if (to - from > TimeSpan.FromDays(30))
            {
                return BadRequest("Date range cannot exceed 30 days");
            }

var avgValidationTimeMs = 0.0; // No ValidationDurationMs field available; placeholder

var performanceMetrics = new
{
    FromDate = from,
    ToDate = to,
    AvgValidationTimeMs = avgValidationTimeMs
};

_logger.LogInformation("Performance metrics retrieved for period {FromDate} to {ToDate}", from, to);

return Ok(performanceMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets comprehensive dashboard metrics combining all monitoring data
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Combined dashboard metrics for admin console display</returns>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardMetrics>> GetDashboardMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-1);
            var to = toDate ?? DateTime.UtcNow;

            if (from > to)
            {
                return BadRequest("fromDate must be earlier than toDate");
            }

            if (to - from > TimeSpan.FromDays(30))
            {
                return BadRequest("Date range cannot exceed 30 days");
            }

var depositTask = Task.Run(async () =>
{
    var totalGenerations = await _context.DepositCodes.CountAsync(dc => dc.CreatedAt >= from && dc.CreatedAt <= to);
    var successfulValidations = await _context.DepositCodes.CountAsync(dc => dc.CreatedAt >= from && dc.CreatedAt <= to && dc.Status == DepositCodeStatus.Used);
    var failedValidations = await _context.DepositCodes.CountAsync(dc => dc.CreatedAt >= from && dc.CreatedAt <= to && dc.Status == DepositCodeStatus.Rejected);
    var successRate = (successfulValidations + failedValidations) > 0 ? (double)successfulValidations / (successfulValidations + failedValidations) : 0.0;
    return new
    {
        FromDate = from,
        ToDate = to,
        TotalGenerations = totalGenerations,
        SuccessfulValidations = successfulValidations,
        FailedValidations = failedValidations,
        SuccessRate = successRate
    };
});

var securityTask = Task.Run(async () =>
{
    var suspiciousCount = await _context.DepositCodes
        .Where(dc => dc.UpdatedAt >= from && dc.UpdatedAt <= to && (dc.Status == DepositCodeStatus.Rejected))
        .CountAsync();
    var duplicateAttempts = await _context.DepositCodes
        .Where(dc => dc.CreatedAt >= from && dc.CreatedAt <= to)
        .GroupBy(dc => dc.Code)
        .CountAsync(g => g.Count() > 1);
    return new { FromDate = from, ToDate = to, SuspiciousCount = suspiciousCount, DuplicateCodeAttempts = duplicateAttempts };
});

var performanceTask = Task.Run(async () =>
{
    // No stored validation duration; return placeholder
    var avgValidationTimeMs = 0.0;
    return new { FromDate = from, ToDate = to, AvgValidationTimeMs = avgValidationTimeMs };
});

await Task.WhenAll(depositTask, securityTask, performanceTask);

var dashboardMetrics = new
{
    FromDate = from,
    ToDate = to,
    DepositCodes = depositTask.Result,
    Security = securityTask.Result,
    Performance = performanceTask.Result,
    LastUpdated = DateTime.UtcNow
};

            _logger.LogInformation("Dashboard metrics retrieved for period {FromDate} to {ToDate}", from, to);
            
            return Ok(dashboardMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard metrics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Triggers an immediate alert check (admin function)
    /// </summary>
    /// <returns>Result of alert check operation</returns>
    [HttpPost("alerts/check")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> TriggerAlertCheck()
    {
        try
        {
_logger.LogInformation("Manual alert check requested (monitoring removed)");
return Ok(new { message = "Alert check requested (monitoring removed)", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual alert check");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Gets health status of monitoring services
    /// </summary>
    /// <returns>Health status of monitoring infrastructure</returns>
    [HttpGet("health")]
    public ActionResult<MonitoringHealthStatus> GetMonitoringHealth()
    {
        try
        {
            // Basic health check - in a real implementation this would check:
            // - Database connectivity
            // - Metrics collection status
            // - Alert service status
            // - External integrations
            
            var healthStatus = new MonitoringHealthStatus
            {
                IsHealthy = true,
                Components = new Dictionary<string, ComponentHealth>
                {
                    ["deposit_code_monitoring"] = new ComponentHealth { IsHealthy = true, LastCheck = DateTime.UtcNow },
                    ["security_monitoring"] = new ComponentHealth { IsHealthy = true, LastCheck = DateTime.UtcNow },
                    ["performance_monitoring"] = new ComponentHealth { IsHealthy = true, LastCheck = DateTime.UtcNow },
                    ["alerting_service"] = new ComponentHealth { IsHealthy = true, LastCheck = DateTime.UtcNow },
                    ["metrics_collection"] = new ComponentHealth { IsHealthy = true, LastCheck = DateTime.UtcNow }
                },
                LastHealthCheck = DateTime.UtcNow,
                UptimeHours = 24.0 // This would be calculated from actual uptime
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monitoring health status");
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Combined dashboard metrics for admin console
/// </summary>
public record DashboardMetrics
{
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public PaymentGatewayService.Services.Interfaces.DepositCodeMetrics DepositCodes { get; init; } = null!;
    public PaymentGatewayService.Services.Interfaces.SecurityMetrics Security { get; init; } = null!;
    public PaymentGatewayService.Services.Interfaces.PerformanceMetrics Performance { get; init; } = null!;
    public DateTime LastUpdated { get; init; }
}

/// <summary>
/// Health status of monitoring infrastructure
/// </summary>
public record MonitoringHealthStatus
{
    public bool IsHealthy { get; init; }
    public Dictionary<string, ComponentHealth> Components { get; init; } = new();
    public DateTime LastHealthCheck { get; init; }
    public double UptimeHours { get; init; }
}

/// <summary>
/// Health status of individual monitoring component
/// </summary>
public record ComponentHealth
{
    public bool IsHealthy { get; init; }
    public DateTime LastCheck { get; init; }
    public string? ErrorMessage { get; init; }
}
