using NotificationService.Models.Requests;
using NotificationService.Models.Responses;

namespace NotificationService.Services.Interfaces;

public interface INotificationDeliveryService
{
    // Core Delivery Operations
    Task<SendNotificationResponse> DeliverNotificationAsync(Guid notificationId);
    Task<bool> DeliverEmailNotificationAsync(Guid notificationId, string recipient, string subject, string body, string? htmlBody = null);
    Task<bool> DeliverSmsNotificationAsync(Guid notificationId, string phoneNumber, string message);
    Task<bool> DeliverPushNotificationAsync(Guid notificationId, string deviceToken, string title, string body, Dictionary<string, object>? data = null);
    Task<bool> DeliverInAppNotificationAsync(Guid notificationId, Guid userId, string title, string body, Dictionary<string, object>? data = null);

    // Delivery Status Management
    Task<bool> UpdateDeliveryStatusAsync(Guid notificationId, string status, string? errorMessage = null, string? externalId = null);
    Task<NotificationDeliveryAttemptResponse> RecordDeliveryAttemptAsync(Guid notificationId, int attemptNumber, string status, string? errorMessage = null, string? provider = null);
    Task<bool> MarkAsDeliveredAsync(Guid notificationId, string? externalId = null);
    Task<bool> MarkAsFailedAsync(Guid notificationId, string errorMessage, bool shouldRetry = true);

    // Retry Logic
    Task<bool> ShouldRetryDeliveryAsync(Guid notificationId);
    Task<DateTime?> CalculateNextRetryTimeAsync(Guid notificationId, int currentRetryCount);
    Task<List<NotificationResponse>> GetNotificationsForRetryAsync(int maxCount = 100);

    // Provider Management
    Task<bool> IsProviderAvailableAsync(string providerName);
    Task<Dictionary<string, bool>> GetProviderHealthStatusAsync();
    Task<string> SelectBestProviderAsync(string notificationType, string? preferredProvider = null);

    // Delivery Analytics
    Task<Dictionary<string, int>> GetDeliveryStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<double> GetDeliverySuccessRateAsync(string? notificationType = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<TimeSpan> GetAverageDeliveryTimeAsync(string? notificationType = null, DateTime? fromDate = null, DateTime? toDate = null);
}
