using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/queue")]
[Authorize(Roles = "Admin,Service")]
public class NotificationQueueController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationQueueService _queueService;
    private readonly ILogger<NotificationQueueController> _logger;

    public NotificationQueueController(
        INotificationService notificationService,
        INotificationQueueService queueService,
        ILogger<NotificationQueueController> logger)
    {
        _notificationService = notificationService;
        _queueService = queueService;
        _logger = logger;
    }

    /// <summary>
    /// Get notification queue status with filtering and pagination
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <param name="priority">Optional priority filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated queue status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<NotificationQueueResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<NotificationQueueResponse>>> GetQueueStatus(
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Retrieving notification queue status - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            if (pageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            // Validate status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                var validStatuses = new[] { "Pending", "Processing", "Completed", "Failed", "Cancelled", "Retrying" };
                if (!validStatuses.Contains(status))
                {
                    return BadRequest($"Invalid status filter. Valid statuses are: {string.Join(", ", validStatuses)}");
                }
            }

            // Validate priority filter if provided
            if (!string.IsNullOrEmpty(priority))
            {
                var validPriorities = new[] { "Low", "Normal", "High", "Critical" };
                if (!validPriorities.Contains(priority))
                {
                    return BadRequest($"Invalid priority filter. Valid priorities are: {string.Join(", ", validPriorities)}");
                }
            }

            var result = await _queueService.GetQueueItemsAsync(page, pageSize, status, priority);
            
            _logger.LogInformation("Retrieved {Count} queued notifications (Page {Page} of {TotalPages})", 
                result.Data.Count, result.Page, result.TotalPages);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting queue status");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification queue status");
            return StatusCode(500, "An error occurred while retrieving notification queue status");
        }
    }

    /// <summary>
    /// Get notification queue statistics
    /// </summary>
    /// <returns>Queue statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Dictionary<string, int>>> GetQueueStats()
    {
        try
        {
            _logger.LogInformation("Retrieving notification queue statistics");

            var stats = await _queueService.GetQueueStatsAsync();
            
            _logger.LogInformation("Retrieved notification queue statistics successfully");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification queue statistics");
            return StatusCode(500, "An error occurred while retrieving notification queue statistics");
        }
    }

    /// <summary>
    /// Retry a failed notification
    /// </summary>
    /// <param name="id">Queue item ID</param>
    /// <returns>Success result</returns>
    [HttpPost("{id:guid}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RetryFailedNotification(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrying failed notification queue item {QueueItemId}", id);

            // First check if the queue item exists
            var queueItem = await _queueService.GetQueueItemAsync(id);
            if (queueItem == null)
            {
                _logger.LogWarning("Queue item {QueueItemId} not found", id);
                return NotFound($"Queue item with ID {id} not found");
            }

            // Check if the item can be retried
            var canRetry = await _queueService.ShouldRetryQueueItemAsync(id);
            if (!canRetry)
            {
                _logger.LogWarning("Queue item {QueueItemId} cannot be retried (may have exceeded max retries)", id);
                return Conflict("Queue item cannot be retried (may have exceeded maximum retry attempts)");
            }

            var success = await _queueService.RequeueForRetryAsync(id);
            if (!success)
            {
                _logger.LogWarning("Failed to requeue notification {QueueItemId} for retry", id);
                return BadRequest("Failed to requeue notification for retry");
            }

            _logger.LogInformation("Notification queue item {QueueItemId} retry initiated successfully", id);
            return Ok(new { message = "Notification retry initiated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying notification queue item {QueueItemId}", id);
            return StatusCode(500, "An error occurred while retrying the notification");
        }
    }

    /// <summary>
    /// Trigger manual queue processing
    /// </summary>
    /// <param name="batchSize">Number of items to process (default: 50, max: 100)</param>
    /// <returns>Success result</returns>
    [HttpPost("process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> TriggerQueueProcessing([FromQuery] int batchSize = 50)
    {
        try
        {
            _logger.LogInformation("Triggering manual notification queue processing with batch size {BatchSize}", batchSize);

            if (batchSize <= 0 || batchSize > 100)
            {
                return BadRequest("Batch size must be between 1 and 100");
            }

            var processedCount = await _queueService.ProcessBatchAsync(batchSize);
            
            _logger.LogInformation("Manual queue processing completed - Processed: {ProcessedCount}", processedCount);
            return Ok(new 
            { 
                message = "Queue processing triggered successfully",
                processedCount = processedCount,
                batchSize = batchSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering notification queue processing");
            return StatusCode(500, "An error occurred while triggering queue processing");
        }
    }
}
