using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Data;
using NotificationService.Data.Entities;
using NotificationService.Models.Requests;

namespace NotificationService.Services;

public class UserNotificationPreferenceBulkService
{
    private readonly NotificationDbContext _context;
    private readonly UserNotificationPreferenceCacheService _cacheService;
    private readonly ILogger<UserNotificationPreferenceBulkService> _logger;

    public UserNotificationPreferenceBulkService(
        NotificationDbContext context,
        UserNotificationPreferenceCacheService cacheService,
        ILogger<UserNotificationPreferenceBulkService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<bool> UpdateMultipleUserPreferencesAsync(List<UpdateUserNotificationPreferencesRequest> requests)
    {
        try
        {
            foreach (var request in requests)
            {
                await UpdateSingleUserPreferencesAsync(request);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating multiple user preferences");
            return false;
        }
    }

    public async Task<bool> BulkUpdateUserPreferencesAsync(List<Guid> userIds, List<UserNotificationPreferenceRequest> preferences)
    {
        try
        {
            foreach (var userId in userIds)
            {
                var request = new UpdateUserNotificationPreferencesRequest
                {
                    UserId = userId,
                    Preferences = preferences
                };

                await UpdateSingleUserPreferencesAsync(request);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk update of user preferences");
            return false;
        }
    }

    public async Task<bool> BulkEnableNotificationTypeAsync(List<Guid> userIds, string notificationType, string channel)
    {
        try
        {
            foreach (var userId in userIds)
            {
                var preference = await _context.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && 
                                             p.NotificationType == notificationType && 
                                             p.Channel == channel);

                if (preference != null)
                {
                    preference.IsEnabled = true;
                    preference.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.UserNotificationPreferences.Add(new UserNotificationPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        NotificationType = notificationType,
                        Channel = channel,
                        IsEnabled = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            
            foreach (var userId in userIds)
            {
                await _cacheService.InvalidateUserPreferencesCacheAsync(userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk enabling notification type");
            return false;
        }
    }

    public async Task<bool> BulkDisableNotificationTypeAsync(List<Guid> userIds, string notificationType, string channel)
    {
        try
        {
            foreach (var userId in userIds)
            {
                var preference = await _context.UserNotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && 
                                             p.NotificationType == notificationType && 
                                             p.Channel == channel);

                if (preference != null)
                {
                    preference.IsEnabled = false;
                    preference.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.UserNotificationPreferences.Add(new UserNotificationPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        NotificationType = notificationType,
                        Channel = channel,
                        IsEnabled = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            
            foreach (var userId in userIds)
            {
                await _cacheService.InvalidateUserPreferencesCacheAsync(userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk disabling notification type");
            return false;
        }
    }

    public async Task<Dictionary<Guid, bool>> CheckBulkNotificationPermissionsAsync(List<Guid> userIds, string notificationType, string channel)
    {
        try
        {
            var results = new Dictionary<Guid, bool>();
            
            // Get all preferences for the specified notification type and channel for all users
            var preferences = await _context.UserNotificationPreferences
                .Where(p => userIds.Contains(p.UserId) && 
                           p.NotificationType == notificationType && 
                           p.Channel == channel)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                var userPreference = preferences.FirstOrDefault(p => p.UserId == userId);
                results[userId] = userPreference?.IsEnabled ?? true; // Default to true if no preference found
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking bulk notification permissions");
            throw;
        }
    }

    public async Task<bool> CopyPreferencesFromUserAsync(Guid sourceUserId, Guid targetUserId)
    {
        try
        {
            var sourcePreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == sourceUserId)
                .ToListAsync();

            if (!sourcePreferences.Any())
                return false;

            var existingTargetPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == targetUserId)
                .ToListAsync();

            if (existingTargetPreferences.Any())
            {
                _context.UserNotificationPreferences.RemoveRange(existingTargetPreferences);
            }

            var newPreferences = sourcePreferences.Select(source => new UserNotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = targetUserId,
                NotificationType = source.NotificationType,
                Channel = source.Channel,
                IsEnabled = source.IsEnabled,
                Frequency = source.Frequency,
                PreferredTime = source.PreferredTime,
                TimeZone = source.TimeZone,
                Settings = source.Settings,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            _context.UserNotificationPreferences.AddRange(newPreferences);
            await _context.SaveChangesAsync();
            await _cacheService.InvalidateUserPreferencesCacheAsync(targetUserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying preferences from user {SourceUserId} to user {TargetUserId}", 
                sourceUserId, targetUserId);
            return false;
        }
    }

    public async Task<bool> MigrateUserPreferencesAsync(Guid oldUserId, Guid newUserId)
    {
        return await CopyPreferencesFromUserAsync(oldUserId, newUserId);
    }

    public async Task<bool> BulkApplyDefaultPreferencesToUsersAsync(List<Guid> userIds, bool overrideExisting = false)
    {
        try
        {
            var defaultPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == Guid.Empty)
                .ToListAsync();

            if (!defaultPreferences.Any())
                return false;

            foreach (var userId in userIds)
            {
                if (overrideExisting)
                {
                    var existingPreferences = await _context.UserNotificationPreferences
                        .Where(p => p.UserId == userId)
                        .ToListAsync();

                    if (existingPreferences.Any())
                    {
                        _context.UserNotificationPreferences.RemoveRange(existingPreferences);
                    }
                }

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
                    Settings = dp.Settings,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                _context.UserNotificationPreferences.AddRange(userPreferences);
            }

            await _context.SaveChangesAsync();

            foreach (var userId in userIds)
            {
                await _cacheService.InvalidateUserPreferencesCacheAsync(userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk applying default preferences to users");
            return false;
        }
    }

    public async Task<int> BulkDeleteUserPreferencesAsync(List<Guid> userIds)
    {
        try
        {
            var preferencesToDelete = await _context.UserNotificationPreferences
                .Where(p => userIds.Contains(p.UserId))
                .ToListAsync();

            var deletedCount = preferencesToDelete.Count;

            if (preferencesToDelete.Any())
            {
                _context.UserNotificationPreferences.RemoveRange(preferencesToDelete);
                await _context.SaveChangesAsync();

                foreach (var userId in userIds)
                {
                    await _cacheService.InvalidateUserPreferencesCacheAsync(userId);
                }
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting user preferences");
            return 0;
        }
    }

    private async Task<bool> UpdateSingleUserPreferencesAsync(UpdateUserNotificationPreferencesRequest request)
    {
        try
        {
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
                        ? System.Text.Json.JsonSerializer.Serialize(preferenceRequest.Settings) 
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
                            ? System.Text.Json.JsonSerializer.Serialize(preferenceRequest.Settings) 
                            : null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserNotificationPreferences.Add(newPreference);
                }
            }

            await _context.SaveChangesAsync();
            await _cacheService.InvalidateUserPreferencesCacheAsync(request.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating single user preferences for user: {UserId}", request.UserId);
            return false;
        }
    }
}
