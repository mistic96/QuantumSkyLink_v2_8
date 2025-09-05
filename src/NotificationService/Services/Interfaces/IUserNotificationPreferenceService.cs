using NotificationService.Models.Requests;
using NotificationService.Models.Responses;

namespace NotificationService.Services.Interfaces;

public interface IUserNotificationPreferenceService
{
    // User Preference Management
    Task<bool> UpdateUserPreferencesAsync(UpdateUserNotificationPreferencesRequest request);
    Task<PagedResponse<UserNotificationPreferenceResponse>> GetUserPreferencesAsync(GetUserNotificationPreferencesRequest request);
    Task<UserNotificationPreferenceResponse?> GetUserPreferenceAsync(Guid userId, string notificationType, string channel);
    Task<bool> DeleteUserPreferenceAsync(Guid userId, string notificationType, string channel);
    Task<bool> ResetUserPreferencesToDefaultAsync(Guid userId);

    // Preference Validation
    Task<bool> IsNotificationAllowedAsync(Guid userId, string notificationType, string channel);
    Task<bool> ShouldSendNotificationAsync(Guid userId, string notificationType, string channel, DateTime? scheduledTime = null);
    Task<List<string>> GetEnabledChannelsForUserAsync(Guid userId, string notificationType);
    Task<Dictionary<string, bool>> GetUserChannelPreferencesAsync(Guid userId, string notificationType);

    // Default Preferences
    Task<List<UserNotificationPreferenceResponse>> GetDefaultPreferencesAsync();
    Task<bool> CreateDefaultPreferencesForUserAsync(Guid userId);
    Task<bool> UpdateDefaultPreferenceAsync(string notificationType, string channel, bool isEnabled, string? frequency = null);
    Task<bool> ApplyDefaultPreferencesToUserAsync(Guid userId, bool overrideExisting = false);

    // Frequency and Timing
    Task<string?> GetUserPreferredFrequencyAsync(Guid userId, string notificationType, string channel);
    Task<TimeOnly?> GetUserPreferredTimeAsync(Guid userId, string notificationType, string channel);
    Task<string?> GetUserTimeZoneAsync(Guid userId, string notificationType, string channel);
    Task<bool> IsWithinUserPreferredTimeAsync(Guid userId, string notificationType, string channel, DateTime currentTime);

    // Batch Operations
    Task<bool> UpdateMultipleUserPreferencesAsync(List<UpdateUserNotificationPreferencesRequest> requests);
    Task<bool> BulkEnableNotificationTypeAsync(List<Guid> userIds, string notificationType, string channel);
    Task<bool> BulkDisableNotificationTypeAsync(List<Guid> userIds, string notificationType, string channel);
    Task<Dictionary<Guid, bool>> CheckBulkNotificationPermissionsAsync(List<Guid> userIds, string notificationType, string channel);

    // Analytics and Reporting
    Task<Dictionary<string, int>> GetPreferenceStatsAsync();
    Task<Dictionary<string, double>> GetChannelAdoptionRatesAsync();
    Task<Dictionary<string, double>> GetNotificationTypeOptInRatesAsync();
    Task<List<Guid>> GetUsersWithDisabledNotificationsAsync(string? notificationType = null, string? channel = null);

    // Migration and Maintenance
    Task<bool> MigrateUserPreferencesAsync(Guid fromUserId, Guid toUserId);
    Task<int> CleanupOrphanedPreferencesAsync();
    Task<bool> ValidateUserPreferencesAsync(Guid userId);
    Task<List<string>> GetMissingPreferencesForUserAsync(Guid userId);

    // Channel-Specific Settings
    Task<Dictionary<string, object>?> GetChannelSettingsAsync(Guid userId, string notificationType, string channel);
    Task<bool> UpdateChannelSettingsAsync(Guid userId, string notificationType, string channel, Dictionary<string, object> settings);
    Task<bool> ValidateChannelSettingsAsync(string channel, Dictionary<string, object> settings);
}
