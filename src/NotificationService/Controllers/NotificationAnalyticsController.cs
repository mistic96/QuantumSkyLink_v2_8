using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin,Service")]
public class NotificationAnalyticsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationQueueService _queueService;
    private readonly ILogger<NotificationAnalyticsController> _logger;

    public NotificationAnalyticsController(
        INotificationService notificationService,
        INotificationQueueService queueService,
        ILogger<NotificationAnalyticsController> logger)
    {
        _notificationService = notificationService;
        _queueService = queueService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive notification statistics
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>Comprehensive notification statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetComprehensiveStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Retrieving comprehensive notification analytics");

            // Validate date range
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            // Get notification stats
            var notificationStats = await _notificationService.GetNotificationStatsAsync(null, fromDate, toDate);
            
            // Get queue stats
            var queueStats = await _queueService.GetQueueStatsAsync();
            
            // Get processing metrics
            var processingMetrics = await _queueService.GetProcessingMetricsAsync(fromDate, toDate);
            
            // Get success rate
            var successRate = await _queueService.GetSuccessRateAsync(fromDate, toDate);
            
            // Get average processing time
            var avgProcessingTime = await _queueService.GetAverageProcessingTimeAsync(fromDate, toDate);

            var comprehensiveStats = new
            {
                Period = new
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    GeneratedAt = DateTime.UtcNow
                },
                NotificationStats = notificationStats,
                QueueStats = queueStats,
                ProcessingMetrics = new
                {
                    SuccessRate = successRate,
                    AverageProcessingTime = avgProcessingTime,
                    AdditionalMetrics = processingMetrics
                },
                Summary = new
                {
                    TotalNotifications = notificationStats.TotalNotifications,
                    DeliverySuccessRate = notificationStats.DeliverySuccessRate,
                    QueueSuccessRate = successRate,
                    AverageProcessingTimeSeconds = avgProcessingTime.TotalSeconds,
                    PendingInQueue = queueStats.ContainsKey("Pending") ? queueStats["Pending"] : 0,
                    FailedInQueue = queueStats.ContainsKey("Failed") ? queueStats["Failed"] : 0
                }
            };
            
            _logger.LogInformation("Retrieved comprehensive notification analytics successfully");
            return Ok(comprehensiveStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comprehensive notification analytics");
            return StatusCode(500, "An error occurred while retrieving notification analytics");
        }
    }

    /// <summary>
    /// Get notification system health status
    /// </summary>
    /// <returns>System health information</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(NotificationHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationHealthResponse>> GetHealthStatus()
    {
        try
        {
            _logger.LogInformation("Retrieving notification system health status");

            var health = await _notificationService.GetHealthAsync();
            
            _logger.LogInformation("Retrieved notification system health status - Healthy: {IsHealthy}", health.IsHealthy);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification system health status");
            return StatusCode(500, "An error occurred while retrieving system health status");
        }
    }

    /// <summary>
    /// Get delivery metrics and performance data
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="channel">Optional channel filter (Email, SMS, Push, InApp)</param>
    /// <returns>Delivery metrics</returns>
    [HttpGet("delivery-metrics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetDeliveryMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? channel = null)
    {
        try
        {
            _logger.LogInformation("Retrieving delivery metrics");

            // Validate date range
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            // Validate channel filter if provided
            if (!string.IsNullOrEmpty(channel))
            {
                var validChannels = new[] { "Email", "SMS", "Push", "InApp" };
                if (!validChannels.Contains(channel))
                {
                    return BadRequest($"Invalid channel filter. Valid channels are: {string.Join(", ", validChannels)}");
                }
            }

            // Get overall notification stats
            var notificationStats = await _notificationService.GetNotificationStatsAsync(null, fromDate, toDate);
            
            // Get processing metrics
            var processingMetrics = await _queueService.GetProcessingMetricsAsync(fromDate, toDate);
            
            // Get queue length for current status
            var currentQueueLength = await _queueService.GetQueueLengthAsync();
            var pendingQueueLength = await _queueService.GetQueueLengthAsync("Pending");
            var failedQueueLength = await _queueService.GetQueueLengthAsync("Failed");

            var deliveryMetrics = new
            {
                Period = new
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    Channel = channel,
                    GeneratedAt = DateTime.UtcNow
                },
                DeliveryStats = new
                {
                    TotalSent = notificationStats.SentNotifications,
                    TotalFailed = notificationStats.FailedNotifications,
                    TotalPending = notificationStats.PendingNotifications,
                    DeliverySuccessRate = notificationStats.DeliverySuccessRate,
                    AverageDeliveryTime = notificationStats.AverageDeliveryTime
                },
                ChannelBreakdown = notificationStats.NotificationsByType,
                QueueMetrics = new
                {
                    CurrentQueueLength = currentQueueLength,
                    PendingItems = pendingQueueLength,
                    FailedItems = failedQueueLength,
                    ProcessingMetrics = processingMetrics
                },
                Performance = new
                {
                    ReadRate = notificationStats.ReadRate,
                    NotificationsByPriority = notificationStats.NotificationsByPriority,
                    NotificationsByStatus = notificationStats.NotificationsByStatus
                }
            };
            
            _logger.LogInformation("Retrieved delivery metrics successfully");
            return Ok(deliveryMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delivery metrics");
            return StatusCode(500, "An error occurred while retrieving delivery metrics");
        }
    }

    /// <summary>
    /// Get user engagement metrics
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="topCount">Number of top items to return (default: 10)</param>
    /// <returns>User engagement metrics</returns>
    [HttpGet("user-engagement")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetUserEngagementMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int topCount = 10)
    {
        try
        {
            _logger.LogInformation("Retrieving user engagement metrics");

            // Validate date range
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            if (topCount <= 0 || topCount > 100)
            {
                return BadRequest("Top count must be between 1 and 100");
            }

            // Get notification stats
            var notificationStats = await _notificationService.GetNotificationStatsAsync(null, fromDate, toDate);
            
            // Get top processors (as a proxy for engagement)
            var topProcessors = await _queueService.GetTopProcessorsAsync(topCount, fromDate, toDate);

            var engagementMetrics = new
            {
                Period = new
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    GeneratedAt = DateTime.UtcNow
                },
                OverallEngagement = new
                {
                    TotalNotifications = notificationStats.TotalNotifications,
                    ReadNotifications = notificationStats.ReadNotifications,
                    ReadRate = notificationStats.ReadRate,
                    DeliverySuccessRate = notificationStats.DeliverySuccessRate
                },
                EngagementByType = notificationStats.NotificationsByType.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        Count = kvp.Value,
                        Percentage = notificationStats.TotalNotifications > 0 
                            ? Math.Round((double)kvp.Value / notificationStats.TotalNotifications * 100, 2)
                            : 0
                    }
                ),
                EngagementByStatus = notificationStats.NotificationsByStatus.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        Count = kvp.Value,
                        Percentage = notificationStats.TotalNotifications > 0 
                            ? Math.Round((double)kvp.Value / notificationStats.TotalNotifications * 100, 2)
                            : 0
                    }
                ),
                TopProcessors = topProcessors,
                Insights = new
                {
                    MostUsedType = notificationStats.NotificationsByType.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key,
                    LeastUsedType = notificationStats.NotificationsByType.OrderBy(kvp => kvp.Value).FirstOrDefault().Key,
                    EngagementTrend = notificationStats.ReadRate > 0.7 ? "High" : 
                                    notificationStats.ReadRate > 0.4 ? "Medium" : "Low"
                }
            };
            
            _logger.LogInformation("Retrieved user engagement metrics successfully");
            return Ok(engagementMetrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user engagement metrics");
            return StatusCode(500, "An error occurred while retrieving user engagement metrics");
        }
    }
}
