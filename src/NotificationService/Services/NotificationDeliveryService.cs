using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using NotificationService.Data;
using NotificationService.Data.Entities;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;

namespace NotificationService.Services;

public class NotificationDeliveryService : INotificationDeliveryService
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationDeliveryService> _logger;
    private readonly IConfiguration _configuration;
    private readonly INotificationHubService _hubService;
    private readonly ISendGridClient _sendGridClient;

    public NotificationDeliveryService(
        NotificationDbContext context,
        ILogger<NotificationDeliveryService> logger,
        IConfiguration configuration,
        INotificationHubService hubService,
        ISendGridClient sendGridClient)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _hubService = hubService;
        _sendGridClient = sendGridClient;

        // Initialize Twilio
        var twilioAccountSid = _configuration["Twilio:AccountSid"];
        var twilioAuthToken = _configuration["Twilio:AuthToken"];
        if (!string.IsNullOrEmpty(twilioAccountSid) && !string.IsNullOrEmpty(twilioAuthToken))
        {
            TwilioClient.Init(twilioAccountSid, twilioAuthToken);
        }
    }

    // Core Delivery Operations
    public async Task<SendNotificationResponse> DeliverNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = await _context.Notifications
                .Include(n => n.Template)
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null)
            {
                _logger.LogError("Notification {NotificationId} not found", notificationId);
                return new SendNotificationResponse
                {
                    NotificationId = notificationId,
                    Status = "Failed",
                    Message = "Notification not found"
                };
            }

            if (notification.Status != "Pending" && notification.Status != "Queued")
            {
                _logger.LogWarning("Notification {NotificationId} has status {Status}, skipping delivery", 
                    notificationId, notification.Status);
                return new SendNotificationResponse
                {
                    NotificationId = notificationId,
                    Status = notification.Status,
                    Message = $"Notification already has status: {notification.Status}"
                };
            }

            _logger.LogInformation("Delivering notification {NotificationId} of type {Type}", 
                notificationId, notification.Type);

            bool deliveryResult = notification.Type.ToLower() switch
            {
                "email" => await DeliverEmailNotificationAsync(notificationId, 
                    notification.Recipient ?? "", notification.Subject, notification.Body, notification.HtmlBody),
                "sms" => await DeliverSmsNotificationAsync(notificationId, 
                    notification.Recipient ?? "", notification.Body),
                "push" => await DeliverPushNotificationAsync(notificationId, 
                    notification.Recipient ?? "", notification.Subject, notification.Body, 
                    notification.Metadata != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Metadata) : null),
                "inapp" => await DeliverInAppNotificationAsync(notificationId, 
                    notification.UserId, notification.Subject, notification.Body,
                    notification.Metadata != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Metadata) : null),
                _ => throw new NotSupportedException($"Notification type {notification.Type} is not supported")
            };

            if (deliveryResult)
            {
                await UpdateDeliveryStatusAsync(notificationId, "Sent");
                return new SendNotificationResponse
                {
                    NotificationId = notificationId,
                    Status = "Sent",
                    Message = "Notification delivered successfully"
                };
            }
            else
            {
                await MarkAsFailedAsync(notificationId, "Delivery failed", true);
                return new SendNotificationResponse
                {
                    NotificationId = notificationId,
                    Status = "Failed",
                    Message = "Delivery failed"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver notification {NotificationId}", notificationId);
            await MarkAsFailedAsync(notificationId, ex.Message, true);
            return new SendNotificationResponse
            {
                NotificationId = notificationId,
                Status = "Failed",
                Message = ex.Message
            };
        }
    }

    public async Task<bool> DeliverEmailNotificationAsync(Guid notificationId, string recipient, string subject, string body, string? htmlBody = null)
    {
        try
        {
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("Delivering email notification {NotificationId} to {Recipient} (attempt {Attempt})", 
                notificationId, recipient, attemptNumber);

            var fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@quantumskylink.com";
            var fromName = _configuration["SendGrid:FromName"] ?? "QuantumSkyLink";

            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(recipient);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, htmlBody);

            var response = await _sendGridClient.SendEmailAsync(msg);
            var responseTime = DateTime.UtcNow - startTime;

            var isSuccess = response.IsSuccessStatusCode;
            var responseBody = await response.Body.ReadAsStringAsync();

            await RecordDeliveryAttemptAsync(notificationId, attemptNumber, 
                isSuccess ? "Success" : "Failed", 
                isSuccess ? null : $"SendGrid error: {response.StatusCode} - {responseBody}",
                "SendGrid");

            if (isSuccess)
            {
                _logger.LogInformation("Email notification {NotificationId} delivered successfully via SendGrid", notificationId);
                return true;
            }
            else
            {
                _logger.LogError("Failed to deliver email notification {NotificationId} via SendGrid: {StatusCode} - {Response}", 
                    notificationId, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception delivering email notification {NotificationId}", notificationId);
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            await RecordDeliveryAttemptAsync(notificationId, attemptNumber, "Failed", ex.Message, "SendGrid");
            return false;
        }
    }

    public async Task<bool> DeliverSmsNotificationAsync(Guid notificationId, string phoneNumber, string message)
    {
        try
        {
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("Delivering SMS notification {NotificationId} to {PhoneNumber} (attempt {Attempt})", 
                notificationId, phoneNumber, attemptNumber);

            var fromPhoneNumber = _configuration["Twilio:FromPhoneNumber"];
            if (string.IsNullOrEmpty(fromPhoneNumber))
            {
                throw new InvalidOperationException("Twilio FromPhoneNumber not configured");
            }

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(phoneNumber)
            );

            var responseTime = DateTime.UtcNow - startTime;
            var isSuccess = messageResource.Status != MessageResource.StatusEnum.Failed;

            await RecordDeliveryAttemptAsync(notificationId, attemptNumber,
                isSuccess ? "Success" : "Failed",
                isSuccess ? null : $"Twilio error: {messageResource.ErrorMessage}",
                "Twilio");

            if (isSuccess)
            {
                _logger.LogInformation("SMS notification {NotificationId} delivered successfully via Twilio. SID: {MessageSid}", 
                    notificationId, messageResource.Sid);
                await UpdateDeliveryStatusAsync(notificationId, "Sent", null, messageResource.Sid);
                return true;
            }
            else
            {
                _logger.LogError("Failed to deliver SMS notification {NotificationId} via Twilio: {ErrorMessage}", 
                    notificationId, messageResource.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception delivering SMS notification {NotificationId}", notificationId);
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            await RecordDeliveryAttemptAsync(notificationId, attemptNumber, "Failed", ex.Message, "Twilio");
            return false;
        }
    }

    public async Task<bool> DeliverPushNotificationAsync(Guid notificationId, string deviceToken, string title, string body, Dictionary<string, object>? data = null)
    {
        try
        {
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("Delivering push notification {NotificationId} to device {DeviceToken} (attempt {Attempt})", 
                notificationId, deviceToken, attemptNumber);

            // TODO: Implement Firebase Cloud Messaging (FCM) or Apple Push Notification Service (APNS)
            // For now, we'll simulate the delivery
            await Task.Delay(100); // Simulate network call

            var isSuccess = true; // Simulate success for now
            var responseTime = DateTime.UtcNow - startTime;

            await RecordDeliveryAttemptAsync(notificationId, attemptNumber,
                isSuccess ? "Success" : "Failed",
                isSuccess ? null : "Push notification service error",
                "FCM/APNS");

            if (isSuccess)
            {
                _logger.LogInformation("Push notification {NotificationId} delivered successfully", notificationId);
                return true;
            }
            else
            {
                _logger.LogError("Failed to deliver push notification {NotificationId}", notificationId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception delivering push notification {NotificationId}", notificationId);
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            await RecordDeliveryAttemptAsync(notificationId, attemptNumber, "Failed", ex.Message, "FCM/APNS");
            return false;
        }
    }

    public async Task<bool> DeliverInAppNotificationAsync(Guid notificationId, Guid userId, string title, string body, Dictionary<string, object>? data = null)
    {
        try
        {
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("Delivering in-app notification {NotificationId} to user {UserId} (attempt {Attempt})", 
                notificationId, userId, attemptNumber);

            // Get notification details for SignalR
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                var notificationResponse = new NotificationResponse
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Subject = notification.Subject,
                    Body = notification.Body,
                    Priority = notification.Priority,
                    CreatedAt = notification.CreatedAt,
                    Status = "Delivered"
                };

                // Send via SignalR
                await _hubService.SendNotificationToUserAsync(userId, notificationResponse);
            }

            var responseTime = DateTime.UtcNow - startTime;

            await RecordDeliveryAttemptAsync(notificationId, attemptNumber, "Success", null, "SignalR");

            _logger.LogInformation("In-app notification {NotificationId} delivered successfully via SignalR", notificationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception delivering in-app notification {NotificationId}", notificationId);
            var attemptNumber = await GetNextAttemptNumberAsync(notificationId);
            await RecordDeliveryAttemptAsync(notificationId, attemptNumber, "Failed", ex.Message, "SignalR");
            return false;
        }
    }

    // Delivery Status Management
    public async Task<bool> UpdateDeliveryStatusAsync(Guid notificationId, string status, string? errorMessage = null, string? externalId = null)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            notification.Status = status;
            notification.ErrorMessage = errorMessage;
            notification.ExternalId = externalId;

            if (status == "Sent")
                notification.SentAt = DateTime.UtcNow;
            else if (status == "Delivered")
                notification.DeliveredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated notification {NotificationId} status to {Status}", notificationId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update delivery status for notification {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<NotificationDeliveryAttemptResponse> RecordDeliveryAttemptAsync(Guid notificationId, int attemptNumber, string status, string? errorMessage = null, string? provider = null)
    {
        try
        {
            var attempt = new NotificationDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                AttemptNumber = attemptNumber,
                Status = status,
                AttemptedAt = DateTime.UtcNow,
                ErrorMessage = errorMessage,
                Provider = provider
            };

            _context.NotificationDeliveryAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recorded delivery attempt {AttemptNumber} for notification {NotificationId} with status {Status}", 
                attemptNumber, notificationId, status);

            return new NotificationDeliveryAttemptResponse
            {
                Id = attempt.Id,
                NotificationId = attempt.NotificationId,
                AttemptNumber = attempt.AttemptNumber,
                Status = attempt.Status,
                AttemptedAt = attempt.AttemptedAt,
                ErrorMessage = attempt.ErrorMessage,
                Provider = attempt.Provider,
                CreatedAt = attempt.CreatedAt,
                UpdatedAt = attempt.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record delivery attempt for notification {NotificationId}", notificationId);
            throw;
        }
    }

    public async Task<bool> MarkAsDeliveredAsync(Guid notificationId, string? externalId = null)
    {
        return await UpdateDeliveryStatusAsync(notificationId, "Delivered", null, externalId);
    }

    public async Task<bool> MarkAsFailedAsync(Guid notificationId, string errorMessage, bool shouldRetry = true)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            notification.Status = shouldRetry ? "Failed" : "Failed_NoRetry";
            notification.ErrorMessage = errorMessage;
            notification.RetryCount++;

            await _context.SaveChangesAsync();

            _logger.LogWarning("Marked notification {NotificationId} as failed: {ErrorMessage}. Retry: {ShouldRetry}", 
                notificationId, errorMessage, shouldRetry);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as failed", notificationId);
            return false;
        }
    }

    // Retry Logic
    public async Task<bool> ShouldRetryDeliveryAsync(Guid notificationId)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            var maxRetries = int.Parse(_configuration["NotificationSettings:MaxRetryAttempts"] ?? "3");
            return notification.RetryCount < maxRetries && notification.Status == "Failed";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check retry status for notification {NotificationId}", notificationId);
            return false;
        }
    }

    public async Task<DateTime?> CalculateNextRetryTimeAsync(Guid notificationId, int currentRetryCount)
    {
        try
        {
            var retryDelayMinutes = int.Parse(_configuration["NotificationSettings:RetryDelayMinutes"] ?? "5");
            
            // Exponential backoff: 5 minutes, 10 minutes, 20 minutes
            var delayMinutes = retryDelayMinutes * Math.Pow(2, currentRetryCount);
            
            return DateTime.UtcNow.AddMinutes(delayMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate next retry time for notification {NotificationId}", notificationId);
            return null;
        }
    }

    public async Task<List<NotificationResponse>> GetNotificationsForRetryAsync(int maxCount = 100)
    {
        try
        {
            var notifications = await _context.Notifications
                .Where(n => n.Status == "Failed" && n.RetryCount < 3)
                .OrderBy(n => n.CreatedAt)
                .Take(maxCount)
                .ToListAsync();

            return notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Subject = n.Subject,
                Body = n.Body,
                Status = n.Status,
                Priority = n.Priority,
                RetryCount = n.RetryCount,
                CreatedAt = n.CreatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get notifications for retry");
            return new List<NotificationResponse>();
        }
    }

    // Provider Management
    public async Task<bool> IsProviderAvailableAsync(string providerName)
    {
        try
        {
            return providerName.ToLower() switch
            {
                "sendgrid" => !string.IsNullOrEmpty(_configuration["SendGrid:ApiKey"]),
                "twilio" => !string.IsNullOrEmpty(_configuration["Twilio:AccountSid"]) && 
                           !string.IsNullOrEmpty(_configuration["Twilio:AuthToken"]),
                "signalr" => true, // Always available if service is running
                "fcm" => false, // TODO: Implement FCM availability check
                "apns" => false, // TODO: Implement APNS availability check
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check provider availability for {ProviderName}", providerName);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> GetProviderHealthStatusAsync()
    {
        try
        {
            var providers = new[] { "SendGrid", "Twilio", "SignalR", "FCM", "APNS" };
            var healthStatus = new Dictionary<string, bool>();

            foreach (var provider in providers)
            {
                healthStatus[provider] = await IsProviderAvailableAsync(provider);
            }

            return healthStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get provider health status");
            return new Dictionary<string, bool>();
        }
    }

    public async Task<string> SelectBestProviderAsync(string notificationType, string? preferredProvider = null)
    {
        try
        {
            var availableProviders = notificationType.ToLower() switch
            {
                "email" => new[] { "SendGrid" },
                "sms" => new[] { "Twilio" },
                "push" => new[] { "FCM", "APNS" },
                "inapp" => new[] { "SignalR" },
                _ => Array.Empty<string>()
            };

            if (!string.IsNullOrEmpty(preferredProvider) && 
                availableProviders.Contains(preferredProvider, StringComparer.OrdinalIgnoreCase))
            {
                var isPreferredAvailable = await IsProviderAvailableAsync(preferredProvider);
                if (isPreferredAvailable)
                    return preferredProvider;
            }

            foreach (var provider in availableProviders)
            {
                var isAvailable = await IsProviderAvailableAsync(provider);
                if (isAvailable)
                    return provider;
            }

            throw new InvalidOperationException($"No available providers for notification type: {notificationType}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to select best provider for notification type {NotificationType}", notificationType);
            throw;
        }
    }

    // Delivery Analytics
    public async Task<Dictionary<string, int>> GetDeliveryStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.NotificationDeliveryAttempts.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AttemptedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AttemptedAt <= toDate.Value);

            var attempts = await query.ToListAsync();

            return new Dictionary<string, int>
            {
                ["TotalAttempts"] = attempts.Count,
                ["SuccessfulAttempts"] = attempts.Count(a => a.Status == "Success"),
                ["FailedAttempts"] = attempts.Count(a => a.Status == "Failed"),
                ["RetryAttempts"] = attempts.Count(a => a.AttemptNumber > 1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery stats");
            return new Dictionary<string, int>();
        }
    }

    public async Task<double> GetDeliverySuccessRateAsync(string? notificationType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Notifications.AsQueryable();

            if (!string.IsNullOrEmpty(notificationType))
                query = query.Where(n => n.Type == notificationType);

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            var notifications = await query.ToListAsync();
            
            if (notifications.Count == 0)
                return 0;

            var successfulDeliveries = notifications.Count(n => n.Status == "Sent" || n.Status == "Delivered");
            return (double)successfulDeliveries / notifications.Count * 100;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery success rate");
            return 0;
        }
    }

    public async Task<TimeSpan> GetAverageDeliveryTimeAsync(string? notificationType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Notifications.AsQueryable();

            if (!string.IsNullOrEmpty(notificationType))
                query = query.Where(n => n.Type == notificationType);

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            var notifications = await query
                .Where(n => n.SentAt.HasValue)
                .ToListAsync();

            if (notifications.Count == 0)
                return TimeSpan.Zero;

            var averageMilliseconds = notifications
                .Average(n => (n.SentAt!.Value - n.CreatedAt).TotalMilliseconds);

            return TimeSpan.FromMilliseconds(averageMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get average delivery time");
            return TimeSpan.Zero;
        }
    }

    // Helper Methods
    private async Task<int> GetNextAttemptNumberAsync(Guid notificationId)
    {
        try
        {
            var lastAttempt = await _context.NotificationDeliveryAttempts
                .Where(a => a.NotificationId == notificationId)
                .OrderByDescending(a => a.AttemptNumber)
                .FirstOrDefaultAsync();

            return (lastAttempt?.AttemptNumber ?? 0) + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next attempt number for notification {NotificationId}", notificationId);
            return 1;
        }
    }
}
