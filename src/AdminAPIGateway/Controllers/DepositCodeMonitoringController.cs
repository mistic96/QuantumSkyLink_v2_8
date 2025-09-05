using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace AdminAPIGateway.Controllers;

/// <summary>
/// Admin controller for deposit code monitoring and dashboard data
/// Proxies requests to PaymentGatewayService monitoring endpoints
/// </summary>
[ApiController]
[Route("api/admin/monitoring/deposit-codes")]
[Authorize(Roles = "Admin,SecurityOfficer")] // Require admin or security officer role
public class DepositCodeMonitoringController : ControllerBase
{
    private readonly ILogger<DepositCodeMonitoringController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public DepositCodeMonitoringController(
        ILogger<DepositCodeMonitoringController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Gets deposit code metrics for admin dashboard
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Deposit code validation metrics and usage patterns</returns>
    [HttpGet("metrics")]
    [Authorize(Policy = "RequireViewMetricsPermission")]
    public async Task<IActionResult> GetDepositCodeMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            // Validate date range security
            if (fromDate.HasValue && toDate.HasValue)
            {
                if (fromDate.Value > toDate.Value)
                {
                    return BadRequest(new { error = "fromDate cannot be greater than toDate" });
                }
                
                // Limit query range to prevent DoS
                var maxRange = TimeSpan.FromDays(90);
                if (toDate.Value - fromDate.Value > maxRange)
                {
                    return BadRequest(new { error = $"Date range cannot exceed {maxRange.Days} days" });
                }
            }

            // Validate dates are not too far in the future
            var maxFutureDate = DateTime.UtcNow.AddDays(1);
            if (fromDate > maxFutureDate || toDate > maxFutureDate)
            {
                return BadRequest(new { error = "Dates cannot be more than 1 day in the future" });
            }

            using var httpClient = _httpClientFactory.CreateClient("PaymentGatewayService");
            
            var queryParams = new List<string>();
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await httpClient.GetAsync($"/api/monitoring/deposit-codes{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Deposit code metrics retrieved successfully from PaymentGatewayService");
                return Content(content, "application/json");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve deposit code metrics. Status: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deposit code metrics");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets security metrics for admin dashboard
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Security metrics including suspicious activity and incident frequency</returns>
    [HttpGet("security")]
    public async Task<IActionResult> GetSecurityMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("PaymentGatewayService");
            
            var queryParams = new List<string>();
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await httpClient.GetAsync($"/api/monitoring/security{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Security metrics retrieved successfully from PaymentGatewayService");
                return Content(content, "application/json");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve security metrics. Status: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security metrics");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets performance metrics for admin dashboard
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Performance metrics including validation times and service uptime</returns>
    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformanceMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("PaymentGatewayService");
            
            var queryParams = new List<string>();
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await httpClient.GetAsync($"/api/monitoring/performance{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Performance metrics retrieved successfully from PaymentGatewayService");
                return Content(content, "application/json");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve performance metrics. Status: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets comprehensive dashboard metrics for admin console
    /// </summary>
    /// <param name="fromDate">Start date for metrics (default: 24 hours ago)</param>
    /// <param name="toDate">End date for metrics (default: now)</param>
    /// <returns>Combined dashboard metrics for real-time monitoring</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("PaymentGatewayService");
            
            var queryParams = new List<string>();
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-ddTHH:mm:ssZ}");
            
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await httpClient.GetAsync($"/api/monitoring/dashboard{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Dashboard metrics retrieved successfully from PaymentGatewayService");
                return Content(content, "application/json");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve dashboard metrics. Status: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard metrics");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Triggers manual alert check (admin function)
    /// </summary>
    /// <returns>Result of alert check operation</returns>
    [HttpPost("alerts/check")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TriggerAlertCheck()
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("PaymentGatewayService");
            var response = await httpClient.PostAsync("/api/monitoring/alerts/check", null);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Manual alert check triggered successfully");
                return Content(content, "application/json");
            }
            else
            {
                _logger.LogWarning("Failed to trigger alert check. Status: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering alert check");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets health status of monitoring infrastructure
    /// </summary>
    /// <returns>Health status of all monitoring components</returns>
    [HttpGet("health")]
    public async Task<IActionResult> GetMonitoringHealth()
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("PaymentGatewayService");
            var response = await httpClient.GetAsync("/api/monitoring/health");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Monitoring health status retrieved successfully");
                return Content(content, "application/json");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve monitoring health status. Status: {StatusCode}", response.StatusCode);
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monitoring health status");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Gets monitoring configuration and settings
    /// </summary>
    /// <returns>Current monitoring configuration for admin review</returns>
    [HttpGet("config")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetMonitoringConfiguration()
    {
        try
        {
            var config = new
            {
                AlertingEnabled = true,
                CheckIntervalMinutes = 5,
                Thresholds = new
                {
                    ValidationSuccessRate = new { Warning = 95.0, Critical = 85.0 },
                    ValidationFailuresPerHour = new { Warning = 10, Critical = 20 },
                    SecurityEventsPerHour = new { Warning = 5, Critical = 10 },
                    AverageValidationTimeMs = new { Warning = 500, Critical = 1000 },
                    ServiceUptimePercent = new { Warning = 99.9, Critical = 99.0 }
                },
                MetricsRetention = new
                {
                    DetailedMetricsDays = 7,
                    SummaryMetricsDays = 90,
                    AuditTrailDays = 365
                },
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Monitoring configuration retrieved");
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monitoring configuration");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}