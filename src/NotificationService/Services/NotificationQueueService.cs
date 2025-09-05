using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NotificationService.Data;
using NotificationService.Data.Entities;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;
using Mapster;

namespace NotificationService.Services;

public class NotificationQueueService : INotificationQueueService
{
    private readonly NotificationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<NotificationQueueService> _logger;
    private readonly INotificationDeliveryService _deliveryService;
    private const string QUEUE_STATS_CACHE_KEY = "queue:stats";
    private const string QUEUE_HEALTH_CACHE_KEY = "queue:health";
    private const string BATCH_PROCESSING_PAUSED_KEY = "queue:batch_paused";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    private const int DEFAULT_MAX_RETRIES = 3;
    private const int RETRY_DELAY_MINUTES = 5;

    public NotificationQueueService(
        NotificationDbContext context,
        IDistributedCache cache,
        ILogger<NotificationQueueService> logger,
        INotificationDeliveryService deliveryService)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _deliveryService = deliveryService;
    }

    public async Task<bool> QueueNotificationAsync(Guid notificationId, DateTime? scheduledAt = null, string priority = "Normal")
    {
        try
        {
            _logger.LogInformation("Queueing notification: {NotificationId}", notificationId);

            // Check if notification exists
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                _logger.LogWarning("Notification not found: {NotificationId}", notificationId);
                return false;
            }

            // Check if already queued
            var existingQueueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.NotificationId == notificationId);

            if (existingQueueItem != null)
            {
                _logger.LogWarning("Notification already queued: {NotificationId}", notificationId);
                return false;
            }

            var queueItem = new NotificationQueue
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                Status = "Pending",
                Priority = priority,
                ScheduledAt = scheduledAt ?? DateTime.UtcNow,
                RetryCount = 0,
                MaxRetries = DEFAULT_MAX_RETRIES,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NotificationQueues.Add(queueItem);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateQueueCacheAsync();

            _logger.LogInformation("Notification queued successfully: {NotificationId}", notificationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing notification: {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<bool> QueueBulkNotificationsAsync(List<Guid> notificationIds, DateTime? scheduledAt = null, string priority = "Normal")
    {
        try
        {
            _logger.LogInformation("Queueing bulk notifications: {Count}", notificationIds.Count);

            var queueItems = new List<NotificationQueue>();

            foreach (var notificationId in notificationIds)
            {
                // Check if notification exists
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification == null)
                {
                    _logger.LogWarning("Notification not found: {NotificationId}", notificationId);
                    continue;
                }

                // Check if already queued
                var existingQueueItem = await _context.NotificationQueues
                    .FirstOrDefaultAsync(q => q.NotificationId == notificationId);

                if (existingQueueItem != null)
                {
                    _logger.LogWarning("Notification already queued: {NotificationId}", notificationId);
                    continue;
                }

                var queueItem = new NotificationQueue
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notificationId,
                    Status = "Pending",
                    Priority = priority,
                    ScheduledAt = scheduledAt ?? DateTime.UtcNow,
                    RetryCount = 0,
                    MaxRetries = DEFAULT_MAX_RETRIES,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                queueItems.Add(queueItem);
            }

            if (queueItems.Any())
            {
                _context.NotificationQueues.AddRange(queueItems);
                await _context.SaveChangesAsync();

                // Invalidate cache
                await InvalidateQueueCacheAsync();
            }

            _logger.LogInformation("Bulk notifications queued successfully: {Count}", queueItems.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing bulk notifications");
            return false;
        }
    }

    public async Task<NotificationQueueResponse?> GetQueueItemAsync(Guid queueId)
    {
        try
        {
            _logger.LogInformation("Getting queue item: {QueueId}", queueId);

            var queueItem = await _context.NotificationQueues
                .Include(q => q.Notification)
                .FirstOrDefaultAsync(q => q.Id == queueId);

            return queueItem?.Adapt<NotificationQueueResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue item: {QueueId}", queueId);
            throw;
        }
    }

    public async Task<PagedResponse<NotificationQueueResponse>> GetQueueItemsAsync(int page = 1, int pageSize = 20, string? status = null, string? priority = null)
    {
        try
        {
            _logger.LogInformation("Getting queue items with filters");

            var query = _context.NotificationQueues
                .Include(q => q.Notification)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(q => q.Status == status);
            }

            if (!string.IsNullOrEmpty(priority))
            {
                query = query.Where(q => q.Priority == priority);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var queueItems = await query
                .OrderBy(q => q.ScheduledAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var queueResponses = queueItems.Adapt<List<NotificationQueueResponse>>();

            return new PagedResponse<NotificationQueueResponse>
            {
                Data = queueResponses,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                HasPreviousPage = page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue items");
            throw;
        }
    }

    public async Task<bool> RemoveFromQueueAsync(Guid queueId)
    {
        try
        {
            _logger.LogInformation("Removing from queue: {QueueId}", queueId);

            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null)
            {
                _logger.LogWarning("Queue item not found: {QueueId}", queueId);
                return false;
            }

            _context.NotificationQueues.Remove(queueItem);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateQueueCacheAsync();

            _logger.LogInformation("Queue item removed successfully: {QueueId}", queueId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from queue: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> CancelQueuedNotificationAsync(Guid notificationId)
    {
        try
        {
            _logger.LogInformation("Cancelling queued notification: {NotificationId}", notificationId);

            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.NotificationId == notificationId);

            if (queueItem == null)
            {
                _logger.LogWarning("Queue item not found for notification: {NotificationId}", notificationId);
                return false;
            }

            if (queueItem.Status == "Processing")
            {
                _logger.LogWarning("Cannot cancel notification that is currently processing: {NotificationId}", notificationId);
                return false;
            }

            queueItem.Status = "Cancelled";
            queueItem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateQueueCacheAsync();

            _logger.LogInformation("Notification cancelled successfully: {NotificationId}", notificationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling notification: {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<List<NotificationQueueResponse>> GetNextItemsToProcessAsync(int maxCount = 10, string? priority = null)
    {
        try
        {
            _logger.LogInformation("Getting next items to process, max count: {MaxCount}", maxCount);

            var query = _context.NotificationQueues
                .Include(q => q.Notification)
                .Where(q => q.Status == "Pending" && q.ScheduledAt <= DateTime.UtcNow);

            if (!string.IsNullOrEmpty(priority))
            {
                query = query.Where(q => q.Priority == priority);
            }

            // Order by priority and scheduled time
            var priorityOrder = new Dictionary<string, int>
            {
                { "Critical", 1 },
                { "High", 2 },
                { "Normal", 3 },
                { "Low", 4 }
            };

            var queueItems = await query
                .OrderBy(q => priorityOrder.ContainsKey(q.Priority) ? priorityOrder[q.Priority] : 5)
                .ThenBy(q => q.ScheduledAt)
                .Take(maxCount)
                .ToListAsync();

            return queueItems.Adapt<List<NotificationQueueResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next items to process");
            throw;
        }
    }

    public async Task<bool> MarkAsProcessingAsync(Guid queueId, string processorId)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null || queueItem.Status != "Pending")
                return false;

            queueItem.Status = "Processing";
            queueItem.ProcessingStartedAt = DateTime.UtcNow;
            queueItem.ProcessorId = processorId;
            queueItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking as processing: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> MarkAsCompletedAsync(Guid queueId)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null)
                return false;

            queueItem.Status = "Completed";
            queueItem.ProcessingCompletedAt = DateTime.UtcNow;
            queueItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await InvalidateQueueCacheAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking as completed: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> MarkAsFailedAsync(Guid queueId, string errorMessage, bool shouldRetry = true)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null)
                return false;

            queueItem.Status = "Failed";
            queueItem.ErrorMessage = errorMessage;
            queueItem.ProcessingCompletedAt = DateTime.UtcNow;
            queueItem.UpdatedAt = DateTime.UtcNow;

            // Schedule retry if within retry limits and shouldRetry is true
            if (shouldRetry && queueItem.RetryCount < queueItem.MaxRetries)
            {
                queueItem.NextRetryAt = DateTime.UtcNow.AddMinutes(RETRY_DELAY_MINUTES * (queueItem.RetryCount + 1));
            }

            await _context.SaveChangesAsync();
            await InvalidateQueueCacheAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking as failed: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> RequeueForRetryAsync(Guid queueId, DateTime? nextRetryAt = null)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null || queueItem.Status != "Failed")
                return false;

            if (queueItem.RetryCount >= queueItem.MaxRetries)
                return false;

            queueItem.Status = "Pending";
            queueItem.RetryCount++;
            queueItem.NextRetryAt = nextRetryAt ?? DateTime.UtcNow.AddMinutes(RETRY_DELAY_MINUTES * queueItem.RetryCount);
            queueItem.ScheduledAt = queueItem.NextRetryAt.Value;
            queueItem.ProcessingStartedAt = null;
            queueItem.ProcessorId = null;
            queueItem.ErrorMessage = null;
            queueItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await InvalidateQueueCacheAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requeuing for retry: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> RescheduleNotificationAsync(Guid queueId, DateTime newScheduledAt)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null || queueItem.Status == "Processing" || queueItem.Status == "Completed")
                return false;

            queueItem.ScheduledAt = newScheduledAt;
            queueItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await InvalidateQueueCacheAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling notification: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<bool> UpdatePriorityAsync(Guid queueId, string newPriority)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null)
                return false;

            queueItem.Priority = newPriority;
            queueItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await InvalidateQueueCacheAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating priority: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<List<NotificationQueueResponse>> GetScheduledNotificationsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.NotificationQueues
                .Include(q => q.Notification)
                .Where(q => q.Status == "Pending");

            if (fromDate.HasValue)
                query = query.Where(q => q.ScheduledAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(q => q.ScheduledAt <= toDate.Value);

            var queueItems = await query
                .OrderBy(q => q.ScheduledAt)
                .ToListAsync();

            return queueItems.Adapt<List<NotificationQueueResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scheduled notifications");
            throw;
        }
    }

    public async Task<List<NotificationQueueResponse>> GetHighPriorityNotificationsAsync(int maxCount = 50)
    {
        try
        {
            var queueItems = await _context.NotificationQueues
                .Include(q => q.Notification)
                .Where(q => q.Status == "Pending" && (q.Priority == "Critical" || q.Priority == "High"))
                .OrderBy(q => q.Priority == "Critical" ? 1 : 2)
                .ThenBy(q => q.ScheduledAt)
                .Take(maxCount)
                .ToListAsync();

            return queueItems.Adapt<List<NotificationQueueResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting high priority notifications");
            throw;
        }
    }

    public async Task<List<NotificationQueueResponse>> GetFailedNotificationsForRetryAsync(int maxCount = 100)
    {
        try
        {
            var queueItems = await _context.NotificationQueues
                .Include(q => q.Notification)
                .Where(q => q.Status == "Failed" && q.RetryCount < q.MaxRetries && 
                           (q.NextRetryAt == null || q.NextRetryAt <= DateTime.UtcNow))
                .OrderBy(q => q.NextRetryAt ?? q.UpdatedAt)
                .Take(maxCount)
                .ToListAsync();

            return queueItems.Adapt<List<NotificationQueueResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed notifications for retry");
            throw;
        }
    }

    public async Task<bool> ShouldRetryQueueItemAsync(Guid queueId)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null || queueItem.Status != "Failed")
                return false;

            return queueItem.RetryCount < queueItem.MaxRetries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if should retry: {QueueId}", queueId);
            return false;
        }
    }

    public async Task<DateTime> CalculateNextRetryTimeAsync(Guid queueId, int currentRetryCount)
    {
        // Exponential backoff: 5, 10, 20, 40 minutes...
        var delayMinutes = RETRY_DELAY_MINUTES * Math.Pow(2, currentRetryCount);
        return DateTime.UtcNow.AddMinutes(delayMinutes);
    }

    public async Task<bool> HasExceededMaxRetriesAsync(Guid queueId)
    {
        try
        {
            var queueItem = await _context.NotificationQueues
                .FirstOrDefaultAsync(q => q.Id == queueId);

            if (queueItem == null)
                return true;

            return queueItem.RetryCount >= queueItem.MaxRetries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking max retries: {QueueId}", queueId);
            return true;
        }
    }

    public async Task<Dictionary<string, int>> GetQueueStatsAsync()
    {
        try
        {
            // Try cache first
            var cachedStats = await _cache.GetStringAsync(QUEUE_STATS_CACHE_KEY);
            if (!string.IsNullOrEmpty(cachedStats))
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(cachedStats)!;
            }

            var stats = new Dictionary<string, int>();

            // Get counts by status
            var statusCounts = await _context.NotificationQueues
                .GroupBy(q => q.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var statusCount in statusCounts)
            {
                stats[statusCount.Status] = statusCount.Count;
            }

            // Get counts by priority
            var priorityCounts = await _context.NotificationQueues
                .GroupBy(q => q.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var priorityCount in priorityCounts)
            {
                stats[$"Priority_{priorityCount.Priority}"] = priorityCount.Count;
            }

            // Cache the stats
            var statsJson = JsonSerializer.Serialize(stats);
            await _cache.SetStringAsync(QUEUE_STATS_CACHE_KEY, statsJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue statistics");
            throw;
        }
    }

    public async Task<int> GetQueueLengthAsync(string? status = null, string? priority = null)
    {
        try
        {
            var query = _context.NotificationQueues.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(q => q.Status == status);
            if (!string.IsNullOrEmpty(priority))
                query = query.Where(q => q.Priority == priority);

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue length");
            throw;
        }
    }

    public async Task<List<NotificationQueueResponse>> GetStuckNotificationsAsync(TimeSpan? processingTimeout = null)
    {
        try
        {
            var timeout = processingTimeout ?? TimeSpan.FromHours(1);
            var cutoffTime = DateTime.UtcNow.Subtract(timeout);

            var stuckItems = await _context.NotificationQueues
                .Include(q => q.Notification)
                .Where(q => q.Status == "Processing" && q.ProcessingStartedAt < cutoffTime)
                .ToListAsync();

            return stuckItems.Adapt<List<NotificationQueueResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stuck notifications");
            throw;
        }
    }

    public async Task<bool> CleanupCompletedItemsAsync(DateTime? olderThan = null)
    {
        try
        {
            var cutoffDate = olderThan ?? DateTime.UtcNow.AddDays(-7);

            var completedItems = await _context.NotificationQueues
                .Where(q => (q.Status == "Completed" || q.Status == "Cancelled") && q.UpdatedAt < cutoffDate)
                .ToListAsync();

            if (completedItems.Any())
            {
                _context.NotificationQueues.RemoveRange(completedItems);
                await _context.SaveChangesAsync();
                await InvalidateQueueCacheAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up completed items");
            return false;
        }
    }

    public async Task<bool> ResetStuckNotificationsAsync(TimeSpan? processingTimeout = null)
    {
        try
        {
            var timeout = processingTimeout ?? TimeSpan.FromHours(1);
            var cutoffTime = DateTime.UtcNow.Subtract(timeout);

            var stuckItems = await _context.NotificationQueues
                .Where(q => q.Status == "Processing" && q.ProcessingStartedAt < cutoffTime)
                .ToListAsync();

            foreach (var item in stuckItems)
            {
                item.Status = "Pending";
                item.ProcessingStartedAt = null;
                item.ProcessorId = null;
                item.UpdatedAt = DateTime.UtcNow;
            }

            if (stuckItems.Any())
            {
                await _context.SaveChangesAsync();
                await InvalidateQueueCacheAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting stuck notifications");
            return false;
        }
    }

    public async Task<int> ProcessBatchAsync(int batchSize = 10, string? priority = null)
    {
        try
        {
            // Check if batch processing is paused
            if (await IsBatchProcessingPausedAsync())
            {
                _logger.LogInformation("Batch processing is paused");
                return 0;
            }

            var pendingNotifications = await GetNextItemsToProcessAsync(batchSize, priority);

            if (!pendingNotifications.Any())
            {
                return 0;
            }

            var processedCount = 0;

            foreach (var queueItem in pendingNotifications)
            {
                try
                {
                    await MarkAsProcessingAsync(queueItem.Id, Environment.MachineName);

                    // Attempt delivery
                    var deliveryResult = await _deliveryService.DeliverNotificationAsync(queueItem.NotificationId);

                    if (deliveryResult.Status == "Sent" || deliveryResult.Status == "Delivered")
                    {
                        await MarkAsCompletedAsync(queueItem.Id);
                        processedCount++;
                    }
                    else
                    {
                        await MarkAsFailedAsync(queueItem.Id, deliveryResult.Message, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing queue item: {QueueId}", queueItem.Id);
                    await MarkAsFailedAsync(queueItem.Id, ex.Message, true);
                }
            }

            return processedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch");
            return 0;
        }
    }

    public async Task<bool> PauseBatchProcessingAsync()
    {
        try
        {
            await _cache.SetStringAsync(BATCH_PROCESSING_PAUSED_KEY, "true");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing batch processing");
            return false;
        }
    }

    public async Task<bool> ResumeBatchProcessingAsync()
    {
        try
        {
            await _cache.RemoveAsync(BATCH_PROCESSING_PAUSED_KEY);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming batch processing");
            return false;
        }
    }

    public async Task<bool> IsBatchProcessingPausedAsync()
    {
        try
        {
            var pausedValue = await _cache.GetStringAsync(BATCH_PROCESSING_PAUSED_KEY);
            return !string.IsNullOrEmpty(pausedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if batch processing is paused");
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetProcessingMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.NotificationQueues.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(q => q.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(q => q.CreatedAt <= toDate.Value);

            var metrics = new Dictionary<string, object>();

            var totalProcessed = await query.CountAsync(q => q.Status == "Completed" || q.Status == "Failed");
            var successfullyProcessed = await query.CountAsync(q => q.Status == "Completed");
            var failed = await query.CountAsync(q => q.Status == "Failed");

            metrics["TotalProcessed"] = totalProcessed;
            metrics["SuccessfullyProcessed"] = successfullyProcessed;
            metrics["Failed"] = failed;
            metrics["SuccessRate"] = totalProcessed > 0 ? (double)successfullyProcessed / totalProcessed : 0;

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing metrics");
            throw;
        }
    }

    public async Task<TimeSpan> GetAverageProcessingTimeAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.NotificationQueues
                .Where(q => q.ProcessingStartedAt.HasValue && q.ProcessingCompletedAt.HasValue);

            if (fromDate.HasValue)
                query = query.Where(q => q.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(q => q.CreatedAt <= toDate.Value);

            var processingTimes = await query
                .Select(q => q.ProcessingCompletedAt!.Value - q.ProcessingStartedAt!.Value)
                .ToListAsync();

            if (!processingTimes.Any())
                return TimeSpan.Zero;

            var averageTicks = (long)processingTimes.Average(t => t.Ticks);
            return new TimeSpan(averageTicks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average processing time");
            throw;
        }
    }

    public async Task<double> GetSuccessRateAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.NotificationQueues.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(q => q.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(q => q.CreatedAt <= toDate.Value);

            var totalProcessed = await query.CountAsync(q => q.Status == "Completed" || q.Status == "Failed");
            var successfullyProcessed = await query.CountAsync(q => q.Status == "Completed");

            return totalProcessed > 0 ? (double)successfullyProcessed / totalProcessed : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting success rate");
            throw;
        }
    }

    public async Task<List<string>> GetTopProcessorsAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.NotificationQueues
                .Where(q => !string.IsNullOrEmpty(q.ProcessorId));

            if (fromDate.HasValue)
                query = query.Where(q => q.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(q => q.CreatedAt <= toDate.Value);

            var topProcessors = await query
                .GroupBy(q => q.ProcessorId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key!)
                .ToListAsync();

            return topProcessors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top processors");
            throw;
        }
    }

    private async Task InvalidateQueueCacheAsync()
    {
        await _cache.RemoveAsync(QUEUE_STATS_CACHE_KEY);
        await _cache.RemoveAsync(QUEUE_HEALTH_CACHE_KEY);
    }
}
