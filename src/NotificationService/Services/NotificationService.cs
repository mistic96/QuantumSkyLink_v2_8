using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Mapster;
using System.Text.Json;
using NotificationService.Data;
using NotificationService.Data.Entities;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;

namespace NotificationService.Services;

public class NotificationService : INotificationService
{
    private readonly NotificationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<NotificationService> _logger;
    private readonly INotificationDeliveryService _deliveryService;
    private readonly INotificationTemplateService _templateService;
    private readonly INotificationQueueService _queueService;
    private readonly IUserNotificationPreferenceService _preferenceService;
    private readonly INotificationHubService _hubService;

    public NotificationService(
        NotificationDbContext context,
        IDistributedCache cache,
        ILogger<NotificationService> logger,
        INotificationDeliveryService deliveryService,
        INotificationTemplateService templateService,
        INotificationQueueService queueService,
        IUserNotificationPreferenceService preferenceService,
        INotificationHubService hubService)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _deliveryService = deliveryService;
        _templateService = templateService;
        _queueService = queueService;
        _preferenceService = preferenceService;
        _hubService = hubService;
    }

    // Notification Management
    public async Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending notification to user {UserId} of type {Type}", request.UserId, request.Type);

            // Check user preferences
            var isAllowed = await _preferenceService.IsNotificationAllowedAsync(request.UserId, request.Type, request.Channel ?? request.Type);
            if (!isAllowed)
            {
                _logger.LogWarning("Notification blocked by user preferences for user {UserId}, type {Type}", request.UserId, request.Type);
                return new SendNotificationResponse
                {
                    NotificationId = Guid.Empty,
                    Status = "Blocked",
                    Message = "Notification blocked by user preferences"
                };
            }

            // Create notification entity
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                TemplateId = request.TemplateId,
                Type = request.Type,
                Subject = request.Subject,
                Body = request.Body,
                HtmlBody = request.HtmlBody,
                Priority = request.Priority,
                Recipient = request.Recipient,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
                ScheduledAt = request.ScheduledAt,
                Channel = request.Channel,
                Status = "Pending"
            };

            // Render template if specified
            if (request.TemplateId.HasValue && request.Variables != null)
            {
                var renderedContent = await _templateService.RenderTemplateContentAsync(request.TemplateId.Value, request.Variables);
                notification.Subject = renderedContent.subject;
                notification.Body = renderedContent.body;
                notification.HtmlBody = renderedContent.htmlBody;
            }

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Queue for delivery
            var scheduledAt = request.ScheduledAt ?? DateTime.UtcNow;
            await _queueService.QueueNotificationAsync(notification.Id, scheduledAt, request.Priority);

            // If immediate delivery and not scheduled, attempt delivery now
            if (!request.ScheduledAt.HasValue)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _deliveryService.DeliverNotificationAsync(notification.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to deliver notification {NotificationId}", notification.Id);
                    }
                });
            }

            return new SendNotificationResponse
            {
                NotificationId = notification.Id,
                Status = "Queued",
                Message = "Notification queued for delivery",
                ScheduledAt = scheduledAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<SendBulkNotificationResponse> SendBulkNotificationAsync(SendBulkNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending bulk notification to {UserCount} users of type {Type}", request.UserIds.Count, request.Type);

            var response = new SendBulkNotificationResponse
            {
                TotalRequested = request.UserIds.Count
            };

            var tasks = request.UserIds.Select(async userId =>
            {
                try
                {
                    var individualRequest = new SendNotificationRequest
                    {
                        UserId = userId,
                        TemplateId = request.TemplateId,
                        Type = request.Type,
                        Subject = request.Subject,
                        Body = request.Body,
                        HtmlBody = request.HtmlBody,
                        Priority = request.Priority,
                        Variables = request.Variables,
                        ScheduledAt = request.ScheduledAt,
                        Channel = request.Channel,
                        Metadata = request.Metadata
                    };

                    var result = await SendNotificationAsync(individualRequest);
                    response.Results.Add(result);

                    if (result.Status == "Queued")
                        response.SuccessfullyQueued++;
                    else
                        response.Failed++;
                }
                catch (Exception ex)
                {
                    response.Failed++;
                    response.Errors.Add($"User {userId}: {ex.Message}");
                    _logger.LogError(ex, "Failed to send notification to user {UserId} in bulk operation", userId);
                }
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation("Bulk notification completed: {Successful} successful, {Failed} failed", 
                response.SuccessfullyQueued, response.Failed);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send bulk notification");
            throw;
        }
    }

    public async Task<NotificationResponse?> GetNotificationAsync(Guid notificationId)
    {
        try
        {
            var cacheKey = $"notification:{notificationId}";
            var cachedNotification = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedNotification))
            {
                return JsonSerializer.Deserialize<NotificationResponse>(cachedNotification);
            }

            var notification = await _context.Notifications
                .Include(n => n.Template)
                .Include(n => n.DeliveryAttempts)
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
                return null;

            var response = notification.Adapt<NotificationResponse>();

            // Cache for 5 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification {NotificationId}", notificationId);
            throw;
        }
    }

    public async Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(GetNotificationsRequest request)
    {
        try
        {
            var query = _context.Notifications
                .Include(n => n.Template)
                .AsQueryable();

            // Apply filters
            if (request.UserId.HasValue)
                query = query.Where(n => n.UserId == request.UserId.Value);

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(n => n.Type == request.Type);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(n => n.Status == request.Status);

            if (!string.IsNullOrEmpty(request.Priority))
                query = query.Where(n => n.Priority == request.Priority);

            if (request.FromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(n => n.CreatedAt <= request.ToDate.Value);

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "subject" => request.SortDirection?.ToLower() == "desc" 
                    ? query.OrderByDescending(n => n.Subject)
                    : query.OrderBy(n => n.Subject),
                "status" => request.SortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(n => n.Status)
                    : query.OrderBy(n => n.Status),
                "priority" => request.SortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(n => n.Priority)
                    : query.OrderBy(n => n.Priority),
                _ => request.SortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(n => n.CreatedAt)
                    : query.OrderBy(n => n.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var notifications = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var response = new PagedResponse<NotificationResponse>
            {
                Data = notifications.Adapt<List<NotificationResponse>>(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page < (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasPreviousPage = request.Page > 1
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications");
            throw;
        }
    }

    public async Task<bool> UpdateNotificationStatusAsync(Guid notificationId, UpdateNotificationStatusRequest request)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            notification.Status = request.Status;
            notification.ErrorMessage = request.ErrorMessage;

            if (request.Status == "Sent")
                notification.SentAt = DateTime.UtcNow;
            else if (request.Status == "Delivered")
                notification.DeliveredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync($"notification:{notificationId}");

            // Notify via SignalR if status changed to delivered
            if (request.Status == "Delivered")
            {
                await _hubService.NotifyNotificationDeliveredAsync(notification.UserId, notificationId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notification status for {NotificationId}", notificationId);
            throw;
        }
    }

    public async Task<bool> MarkNotificationAsReadAsync(MarkNotificationReadRequest request)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId);

            if (notification == null)
                return false;

            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _cache.RemoveAsync($"notification:{request.NotificationId}");

            // Notify via SignalR
            await _hubService.NotifyNotificationReadAsync(request.UserId, request.NotificationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification as read {NotificationId}", request.NotificationId);
            throw;
        }
    }

    public async Task<bool> CancelNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            if (notification.Status == "Sent" || notification.Status == "Delivered")
            {
                _logger.LogWarning("Cannot cancel notification {NotificationId} with status {Status}", 
                    notificationId, notification.Status);
                return false;
            }

            notification.Status = "Cancelled";
            await _context.SaveChangesAsync();

            // Cancel in queue
            await _queueService.CancelQueuedNotificationAsync(notificationId);

            // Invalidate cache
            await _cache.RemoveAsync($"notification:{notificationId}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel notification {NotificationId}", notificationId);
            throw;
        }
    }

    public async Task<NotificationStatsResponse> GetNotificationStatsAsync(Guid? userId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var cacheKey = $"notification_stats:{userId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
            var cachedStats = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedStats))
            {
                return JsonSerializer.Deserialize<NotificationStatsResponse>(cachedStats);
            }

            var query = _context.Notifications.AsQueryable();

            if (userId.HasValue)
                query = query.Where(n => n.UserId == userId.Value);

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            var notifications = await query.ToListAsync();

            var stats = new NotificationStatsResponse
            {
                TotalNotifications = notifications.Count,
                PendingNotifications = notifications.Count(n => n.Status == "Pending"),
                SentNotifications = notifications.Count(n => n.Status == "Sent" || n.Status == "Delivered"),
                FailedNotifications = notifications.Count(n => n.Status == "Failed"),
                ReadNotifications = notifications.Count(n => n.ReadAt.HasValue),
                NotificationsByType = notifications.GroupBy(n => n.Type).ToDictionary(g => g.Key, g => g.Count()),
                NotificationsByStatus = notifications.GroupBy(n => n.Status).ToDictionary(g => g.Key, g => g.Count()),
                NotificationsByPriority = notifications.GroupBy(n => n.Priority).ToDictionary(g => g.Key, g => g.Count()),
                DeliverySuccessRate = notifications.Count > 0 
                    ? (double)notifications.Count(n => n.Status == "Sent" || n.Status == "Delivered") / notifications.Count * 100 
                    : 0,
                ReadRate = notifications.Count > 0 
                    ? (double)notifications.Count(n => n.ReadAt.HasValue) / notifications.Count * 100 
                    : 0,
                AverageDeliveryTime = notifications.Where(n => n.SentAt.HasValue).Any()
                    ? TimeSpan.FromMilliseconds(notifications.Where(n => n.SentAt.HasValue)
                        .Average(n => (n.SentAt!.Value - n.CreatedAt).TotalMilliseconds))
                    : TimeSpan.Zero
            };

            // Cache for 10 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats), cacheOptions);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notification stats");
            throw;
        }
    }

    // Template Management - Delegate to Template Service
    public async Task<NotificationTemplateResponse> CreateTemplateAsync(CreateNotificationTemplateRequest request)
        => await _templateService.CreateTemplateAsync(request);

    public async Task<NotificationTemplateResponse?> GetTemplateAsync(Guid templateId)
        => await _templateService.GetTemplateAsync(templateId);

    public async Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesAsync(GetNotificationTemplatesRequest request)
        => await _templateService.GetTemplatesAsync(request);

    public async Task<NotificationTemplateResponse?> UpdateTemplateAsync(Guid templateId, UpdateNotificationTemplateRequest request)
        => await _templateService.UpdateTemplateAsync(templateId, request);

    public async Task<bool> DeleteTemplateAsync(Guid templateId)
        => await _templateService.DeleteTemplateAsync(templateId);

    public async Task<TemplateValidationResponse> ValidateTemplateAsync(Guid templateId, Dictionary<string, object>? variables = null)
        => await _templateService.ValidateTemplateAsync(templateId, variables);

    // User Preferences - Delegate to Preference Service
    public async Task<bool> UpdateUserPreferencesAsync(UpdateUserNotificationPreferencesRequest request)
        => await _preferenceService.UpdateUserPreferencesAsync(request);

    public async Task<PagedResponse<UserNotificationPreferenceResponse>> GetUserPreferencesAsync(GetUserNotificationPreferencesRequest request)
        => await _preferenceService.GetUserPreferencesAsync(request);

    public async Task<bool> ResetUserPreferencesToDefaultAsync(Guid userId)
        => await _preferenceService.ResetUserPreferencesToDefaultAsync(userId);

    // Health and Monitoring
    public async Task<NotificationHealthResponse> GetHealthAsync()
    {
        try
        {
            var health = new NotificationHealthResponse
            {
                IsHealthy = true,
                Status = "Healthy",
                CheckedAt = DateTime.UtcNow
            };

            // Check database connectivity
            try
            {
                await _context.Database.CanConnectAsync();
                health.Details["Database"] = "Connected";
            }
            catch (Exception ex)
            {
                health.IsHealthy = false;
                health.Status = "Unhealthy";
                health.Issues.Add($"Database connection failed: {ex.Message}");
                health.Details["Database"] = "Disconnected";
            }

            // Check cache connectivity
            try
            {
                await _cache.SetStringAsync("health_check", "test", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                });
                await _cache.RemoveAsync("health_check");
                health.Details["Cache"] = "Connected";
            }
            catch (Exception ex)
            {
                health.IsHealthy = false;
                health.Status = "Unhealthy";
                health.Issues.Add($"Cache connection failed: {ex.Message}");
                health.Details["Cache"] = "Disconnected";
            }

            // Check queue health
            var queueStats = await _queueService.GetQueueStatsAsync();
            health.Details["QueueLength"] = queueStats.GetValueOrDefault("Queued", 0);
            health.Details["ProcessingItems"] = queueStats.GetValueOrDefault("Processing", 0);

            if (queueStats.GetValueOrDefault("Queued", 0) > 1000)
            {
                health.Issues.Add("Queue length is high");
            }

            return health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get health status");
            return new NotificationHealthResponse
            {
                IsHealthy = false,
                Status = "Error",
                CheckedAt = DateTime.UtcNow,
                Issues = { $"Health check failed: {ex.Message}" }
            };
        }
    }

    public async Task<PagedResponse<NotificationQueueResponse>> GetQueueStatusAsync(int page = 1, int pageSize = 20)
        => await _queueService.GetQueueItemsAsync(page, pageSize);
}
