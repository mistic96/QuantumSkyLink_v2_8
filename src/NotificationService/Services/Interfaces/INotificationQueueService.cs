using NotificationService.Models.Requests;
using NotificationService.Models.Responses;

namespace NotificationService.Services.Interfaces;

public interface INotificationQueueService
{
    // Queue Management
    Task<bool> QueueNotificationAsync(Guid notificationId, DateTime? scheduledAt = null, string priority = "Normal");
    Task<bool> QueueBulkNotificationsAsync(List<Guid> notificationIds, DateTime? scheduledAt = null, string priority = "Normal");
    Task<NotificationQueueResponse?> GetQueueItemAsync(Guid queueId);
    Task<PagedResponse<NotificationQueueResponse>> GetQueueItemsAsync(int page = 1, int pageSize = 20, string? status = null, string? priority = null);
    Task<bool> RemoveFromQueueAsync(Guid queueId);
    Task<bool> CancelQueuedNotificationAsync(Guid notificationId);

    // Queue Processing
    Task<List<NotificationQueueResponse>> GetNextItemsToProcessAsync(int maxCount = 10, string? priority = null);
    Task<bool> MarkAsProcessingAsync(Guid queueId, string processorId);
    Task<bool> MarkAsCompletedAsync(Guid queueId);
    Task<bool> MarkAsFailedAsync(Guid queueId, string errorMessage, bool shouldRetry = true);
    Task<bool> RequeueForRetryAsync(Guid queueId, DateTime? nextRetryAt = null);

    // Scheduling and Priority
    Task<bool> RescheduleNotificationAsync(Guid queueId, DateTime newScheduledAt);
    Task<bool> UpdatePriorityAsync(Guid queueId, string newPriority);
    Task<List<NotificationQueueResponse>> GetScheduledNotificationsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<NotificationQueueResponse>> GetHighPriorityNotificationsAsync(int maxCount = 50);

    // Retry Logic
    Task<List<NotificationQueueResponse>> GetFailedNotificationsForRetryAsync(int maxCount = 100);
    Task<bool> ShouldRetryQueueItemAsync(Guid queueId);
    Task<DateTime> CalculateNextRetryTimeAsync(Guid queueId, int currentRetryCount);
    Task<bool> HasExceededMaxRetriesAsync(Guid queueId);

    // Queue Health and Monitoring
    Task<Dictionary<string, int>> GetQueueStatsAsync();
    Task<int> GetQueueLengthAsync(string? status = null, string? priority = null);
    Task<List<NotificationQueueResponse>> GetStuckNotificationsAsync(TimeSpan? processingTimeout = null);
    Task<bool> CleanupCompletedItemsAsync(DateTime? olderThan = null);
    Task<bool> ResetStuckNotificationsAsync(TimeSpan? processingTimeout = null);

    // Batch Operations
    Task<int> ProcessBatchAsync(int batchSize = 10, string? priority = null);
    Task<bool> PauseBatchProcessingAsync();
    Task<bool> ResumeBatchProcessingAsync();
    Task<bool> IsBatchProcessingPausedAsync();

    // Queue Analytics
    Task<Dictionary<string, object>> GetProcessingMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<TimeSpan> GetAverageProcessingTimeAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<double> GetSuccessRateAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<string>> GetTopProcessorsAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
}
