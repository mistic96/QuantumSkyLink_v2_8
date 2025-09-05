using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NotificationService.Data;
using NotificationService.Data.Entities;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;
using Mapster;

namespace NotificationService.Services;

public class UserNotificationPreferenceService : IUserNotificationPreferenceService
{
    private readonly NotificationDbContext _context;
    private readonly UserNotificationPreferenceCacheService _cacheService;
    private readonly UserNotificationPreferenceAnalyticsService _analyticsService;
    private readonly UserNotificationPreferenceBulkService _bulkService;
    private readonly UserNotificationPreferenceValidationService _validationService;
    private readonly ILogger<UserNotificationPreferenceService> _logger;

    public UserNotificationPreferenceService(
        NotificationDbContext context,
        UserNotificationPreferenceCacheService cacheService,
        UserNotificationPreferenceAnalyticsService analyticsService,
        UserNotificationPreferenceBulkService bulkService,
        UserNotificationPreferenceValidationService validationService,
        ILogger<UserNotificationPreferenceService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _analyticsService = analyticsService;
        _bulkService = bulkService;
        _validationService = validationService;
        _logger = logger;
    }

    // Core CRUD Operations
    public async Task<bool> UpdateUserPreferencesAsync(UpdateUserNotificationPreferencesRequest request)
    {
        try
        {
            _logger.LogInformation("Updating user preferences for user: {UserId}", request.UserId);

            var existingPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == request.UserId)
                .ToListAsync();

            var preferencesToRemove = existingPreferences
                .Where(existing => !request.Preferences.Any(req => 
                    req.NotificationType == existing.NotificationType && 
                    req.Channel == existing.Channel))
                .ToList();

            if (preferencesToRemove.Any())
            {
                _context.UserNotificationPreferences.RemoveRange(preferencesToRemove);
            }

            foreach (var preferenceRequest in request.Preferences)
            {
                var existingPreference = existingPreferences
                    .FirstOrDefault(p => p.NotificationType == preferenceRequest.NotificationType && 
                                        p.Channel == preferenceRequest.Channel);

                if (existingPreference != null)
                {
                    existingPreference.IsEnabled = preferenceRequest.IsEnabled;
                    existingPreference.Frequency = preferenceRequest.Frequency;
                    existingPreference.PreferredTime = preferenceRequest.PreferredTime;
                    existingPreference.TimeZone = preferenceRequest.TimeZone;
                    existingPreference.Settings = preferenceRequest.Settings != null 
                        ? JsonSerializer.Serialize(preferenceRequest.Settings) 
                        : null;
                    existingPreference.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newPreference = new UserNotificationPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.UserId,
                        NotificationType = preferenceRequest.NotificationType,
                        Channel = preferenceRequest.Channel,
                        IsEnabled = preferenceRequest.IsEnabled,
                        Frequency = preferenceRequest.Frequency,
                        PreferredTime = preferenceRequest.PreferredTime,
                        TimeZone = preferenceRequest.TimeZone,
                        Settings = preferenceRequest.Settings != null 
                            ? JsonSerializer.Serialize(preferenceRequest.Settings) 
                            : null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserNotificationPreferences.Add(newPreference);
                }
            }

            await _context.SaveChangesAsync();
            await _cacheService.InvalidateUserPreferencesCacheAsync(request.UserId);

            _logger.LogInformation("User preferences updated successfully for user: {UserId}", request.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences for user: {UserId}", request.UserId);
            return false;
        }
    }

    public async Task<List<UserNotificationPreferenceResponse>> GetUserPreferencesAsync(Guid userId, string? notificationType = null, string? channel = null)
    {
        try
        {
            // Try cache first
            var cachedPreferences = await _cacheService.GetUserPreferencesFromCacheAsync(userId);
            List<UserNotificationPreference> preferences;

            if (cachedPreferences != null)
            {
                preferences = cachedPreferences;
            }
            else
            {
                preferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                await _cacheService.SetUserPreferencesCacheAsync(userId, preferences);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(notificationType))
            {
                preferences = preferences.Where(p => p.NotificationType == notificationType).ToList();
            }

            if (!string.IsNullOrEmpty(channel))
            {
                preferences = preferences.Where(p => p.Channel == channel).ToList();
            }

            var responses = preferences.Adapt<List<UserNotificationPreferenceResponse>>();

            // Parse settings JSON
            foreach (var response in responses)
            {
                var preference = preferences.First(p => p.Id == response.Id);
                if (!string.IsNullOrEmpty(preference.Settings))
                {
                    try
                    {
                        response.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(preference.Settings);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse settings JSON for preference: {PreferenceId}", preference.Id);
                        response.Settings = new Dictionary<string, object>();
                    }
                }
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<PagedResponse<UserNotificationPreferenceResponse>> GetUserPreferencesAsync(GetUserNotificationPreferencesRequest request)
    {
        var preferences = await GetUserPreferencesAsync(request.UserId, request.NotificationType, request.Channel);
        return new PagedResponse<UserNotificationPreferenceResponse>
        {
            Data = preferences,
            Page = 1,
            PageSize = preferences.Count,
            TotalCount = preferences.Count,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };
    }

    public async Task<UserNotificationPreferenceResponse?> GetUserPreferenceAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId, notificationType, channel);
            return preferences.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preference for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserPreferenceAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var preference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.NotificationType == notificationType && 
                                         p.Channel == channel);

            if (preference == null)
                return false;

            _context.UserNotificationPreferences.Remove(preference);
            await _context.SaveChangesAsync();
            await _cacheService.InvalidateUserPreferencesCacheAsync(userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user preference");
            return false;
        }
    }

    public async Task<bool> ResetUserPreferencesToDefaultAsync(Guid userId)
    {
        try
        {
            var userPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == userId)
                .ToListAsync();

            if (userPreferences.Any())
            {
                _context.UserNotificationPreferences.RemoveRange(userPreferences);
                await _context.SaveChangesAsync();
                await _cacheService.InvalidateUserPreferencesCacheAsync(userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting user preferences to default for user: {UserId}", userId);
            return false;
        }
    }

    // Preference Validation - Delegate to ValidationService
    public async Task<bool> IsNotificationAllowedAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var userPreferences = await GetUserPreferencesAsync(userId, notificationType, channel);

            var specificPreference = userPreferences
                .FirstOrDefault(p => p.NotificationType == notificationType && p.Channel == channel);

            if (specificPreference != null)
            {
                return specificPreference.IsEnabled;
            }

            var typePreference = userPreferences
                .FirstOrDefault(p => p.NotificationType == notificationType);

            if (typePreference != null)
            {
                return typePreference.IsEnabled;
            }

            var defaultPreferences = await GetDefaultPreferencesAsync();
            var defaultPreference = defaultPreferences
                .FirstOrDefault(p => p.NotificationType == notificationType && p.Channel == channel);

            if (defaultPreference != null)
            {
                return defaultPreference.IsEnabled;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if notification is allowed for user: {UserId}", userId);
            return true;
        }
    }

    public async Task<bool> ShouldSendNotificationAsync(Guid userId, string notificationType, string channel, DateTime? scheduledTime = null)
    {
        try
        {
            if (!await IsNotificationAllowedAsync(userId, notificationType, channel))
                return false;

            if (!await _validationService.IsFrequencyAllowedAsync(userId, notificationType, channel))
                return false;

            if (scheduledTime.HasValue)
            {
                return await _validationService.IsWithinUserPreferredTimeAsync(userId, notificationType, channel, scheduledTime.Value);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if should send notification");
            return true;
        }
    }

    public async Task<List<string>> GetEnabledChannelsForUserAsync(Guid userId, string notificationType)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId, notificationType);
            return preferences.Where(p => p.IsEnabled).Select(p => p.Channel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enabled channels for user: {UserId}, type: {Type}", userId, notificationType);
            throw;
        }
    }

    public async Task<Dictionary<string, bool>> GetUserChannelPreferencesAsync(Guid userId, string notificationType)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId, notificationType);
            return preferences.ToDictionary(p => p.Channel, p => p.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user channel preferences");
            throw;
        }
    }

    // Default Preferences
    public async Task<List<UserNotificationPreferenceResponse>> GetDefaultPreferencesAsync()
    {
        try
        {
            var cachedDefaults = await _cacheService.GetDefaultPreferencesFromCacheAsync();
            List<UserNotificationPreference> defaultPreferences;

            if (cachedDefaults != null)
            {
                defaultPreferences = cachedDefaults;
            }
            else
            {
                defaultPreferences = await _context.UserNotificationPreferences
                    .Where(p => p.UserId == Guid.Empty)
                    .ToListAsync();

                await _cacheService.SetDefaultPreferencesCacheAsync(defaultPreferences);
            }

            var responses = defaultPreferences.Adapt<List<UserNotificationPreferenceResponse>>();

            foreach (var response in responses)
            {
                var preference = defaultPreferences.First(p => p.Id == response.Id);
                if (!string.IsNullOrEmpty(preference.Settings))
                {
                    try
                    {
                        response.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(preference.Settings);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse settings JSON for default preference: {PreferenceId}", preference.Id);
                        response.Settings = new Dictionary<string, object>();
                    }
                }
            }

            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default preferences");
            throw;
        }
    }

    public async Task<bool> CreateDefaultPreferencesForUserAsync(Guid userId)
    {
        try
        {
            var defaultPreferences = await GetDefaultPreferencesAsync();
            var userPreferences = defaultPreferences.Select(dp => new UserNotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NotificationType = dp.NotificationType,
                Channel = dp.Channel,
                IsEnabled = dp.IsEnabled,
                Frequency = dp.Frequency,
                PreferredTime = dp.PreferredTime,
                TimeZone = dp.TimeZone,
                Settings = dp.Settings != null ? JsonSerializer.Serialize(dp.Settings) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            _context.UserNotificationPreferences.AddRange(userPreferences);
            await _context.SaveChangesAsync();
            await _cacheService.InvalidateUserPreferencesCacheAsync(userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default preferences for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateDefaultPreferenceAsync(string notificationType, string channel, bool isEnabled, string? frequency = null)
    {
        try
        {
            var defaultPreference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == Guid.Empty && 
                                         p.NotificationType == notificationType && 
                                         p.Channel == channel);

            if (defaultPreference == null)
                return false;

            defaultPreference.IsEnabled = isEnabled;
            if (frequency != null)
                defaultPreference.Frequency = frequency;
            defaultPreference.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _cacheService.InvalidateDefaultPreferencesCacheAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating default preference");
            return false;
        }
    }

    public async Task<bool> ApplyDefaultPreferencesToUserAsync(Guid userId, bool overrideExisting = false)
    {
        try
        {
            if (overrideExisting)
            {
                await ResetUserPreferencesToDefaultAsync(userId);
            }
            return await CreateDefaultPreferencesForUserAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying default preferences to user: {UserId}", userId);
            return false;
        }
    }

    // Frequency and Timing - Delegate to ValidationService
    public async Task<string?> GetUserPreferredFrequencyAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var preference = await GetUserPreferenceAsync(userId, notificationType, channel);
            return preference?.Frequency;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferred frequency");
            return null;
        }
    }

    public async Task<TimeOnly?> GetUserPreferredTimeAsync(Guid userId, string notificationType, string channel)
        => await _validationService.GetPreferredTimeForUserAsync(userId, notificationType, channel);

    public async Task<string?> GetUserTimeZoneAsync(Guid userId, string notificationType, string channel)
        => await _validationService.GetTimeZoneForUserAsync(userId, notificationType, channel);

    public async Task<bool> IsWithinUserPreferredTimeAsync(Guid userId, string notificationType, string channel, DateTime currentTime)
        => await _validationService.IsWithinUserPreferredTimeAsync(userId, notificationType, channel, currentTime);

    // Batch Operations - Delegate to BulkService
    public async Task<bool> UpdateMultipleUserPreferencesAsync(List<UpdateUserNotificationPreferencesRequest> requests)
        => await _bulkService.UpdateMultipleUserPreferencesAsync(requests);

    public async Task<bool> BulkEnableNotificationTypeAsync(List<Guid> userIds, string notificationType, string channel)
        => await _bulkService.BulkEnableNotificationTypeAsync(userIds, notificationType, channel);

    public async Task<bool> BulkDisableNotificationTypeAsync(List<Guid> userIds, string notificationType, string channel)
        => await _bulkService.BulkDisableNotificationTypeAsync(userIds, notificationType, channel);

    public async Task<Dictionary<Guid, bool>> CheckBulkNotificationPermissionsAsync(List<Guid> userIds, string notificationType, string channel)
        => await _bulkService.CheckBulkNotificationPermissionsAsync(userIds, notificationType, channel);

    // Analytics and Reporting - Delegate to AnalyticsService
    public async Task<Dictionary<string, int>> GetPreferenceStatsAsync()
        => await _analyticsService.GetPreferenceStatsAsync();

    public async Task<Dictionary<string, double>> GetChannelAdoptionRatesAsync()
        => await _analyticsService.GetChannelAdoptionRatesAsync();

    public async Task<Dictionary<string, double>> GetNotificationTypeOptInRatesAsync()
        => await _analyticsService.GetNotificationTypeOptInRatesAsync();

    public async Task<List<Guid>> GetUsersWithDisabledNotificationsAsync(string? notificationType = null, string? channel = null)
        => await _analyticsService.GetUsersWithDisabledNotificationsAsync(notificationType, channel);

    // Migration and Maintenance - Delegate to BulkService and ValidationService
    public async Task<bool> MigrateUserPreferencesAsync(Guid fromUserId, Guid toUserId)
        => await _bulkService.MigrateUserPreferencesAsync(fromUserId, toUserId);

    public async Task<int> CleanupOrphanedPreferencesAsync()
        => await _validationService.CleanupOrphanedPreferencesAsync();

    public async Task<bool> ValidateUserPreferencesAsync(Guid userId)
        => await _validationService.ValidateUserPreferencesAsync(userId);

    public async Task<List<string>> GetMissingPreferencesForUserAsync(Guid userId)
        => await _validationService.GetMissingPreferencesForUserAsync(userId);

    // Channel-Specific Settings - Delegate to ValidationService
    public async Task<Dictionary<string, object>?> GetChannelSettingsAsync(Guid userId, string notificationType, string channel)
        => await _validationService.GetChannelSettingsAsync(userId, notificationType, channel);

    public async Task<bool> UpdateChannelSettingsAsync(Guid userId, string notificationType, string channel, Dictionary<string, object> settings)
        => await _validationService.UpdateChannelSettingsAsync(userId, notificationType, channel, settings);

    public async Task<bool> ValidateChannelSettingsAsync(string channel, Dictionary<string, object> settings)
        => await _validationService.ValidateChannelSettingsAsync(channel, settings);
}
