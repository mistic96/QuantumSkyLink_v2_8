using NotificationService.Models.Requests;
using NotificationService.Models.Responses;

namespace NotificationService.Services.Interfaces;

public interface INotificationService
{
    // Notification Management
    Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request);
    Task<SendBulkNotificationResponse> SendBulkNotificationAsync(SendBulkNotificationRequest request);
    Task<NotificationResponse?> GetNotificationAsync(Guid notificationId);
    Task<PagedResponse<NotificationResponse>> GetNotificationsAsync(GetNotificationsRequest request);
    Task<bool> UpdateNotificationStatusAsync(Guid notificationId, UpdateNotificationStatusRequest request);
    Task<bool> MarkNotificationAsReadAsync(MarkNotificationReadRequest request);
    Task<bool> CancelNotificationAsync(Guid notificationId);
    Task<NotificationStatsResponse> GetNotificationStatsAsync(Guid? userId = null, DateTime? fromDate = null, DateTime? toDate = null);

    // Template Management
    Task<NotificationTemplateResponse> CreateTemplateAsync(CreateNotificationTemplateRequest request);
    Task<NotificationTemplateResponse?> GetTemplateAsync(Guid templateId);
    Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesAsync(GetNotificationTemplatesRequest request);
    Task<NotificationTemplateResponse?> UpdateTemplateAsync(Guid templateId, UpdateNotificationTemplateRequest request);
    Task<bool> DeleteTemplateAsync(Guid templateId);
    Task<TemplateValidationResponse> ValidateTemplateAsync(Guid templateId, Dictionary<string, object>? variables = null);

    // User Preferences
    Task<bool> UpdateUserPreferencesAsync(UpdateUserNotificationPreferencesRequest request);
    Task<PagedResponse<UserNotificationPreferenceResponse>> GetUserPreferencesAsync(GetUserNotificationPreferencesRequest request);
    Task<bool> ResetUserPreferencesToDefaultAsync(Guid userId);

    // Health and Monitoring
    Task<NotificationHealthResponse> GetHealthAsync();
    Task<PagedResponse<NotificationQueueResponse>> GetQueueStatusAsync(int page = 1, int pageSize = 20);
}
