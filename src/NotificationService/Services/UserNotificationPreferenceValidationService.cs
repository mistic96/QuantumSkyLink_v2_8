using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NotificationService.Data;
using NotificationService.Data.Entities;

namespace NotificationService.Services;

public class UserNotificationPreferenceValidationService
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<UserNotificationPreferenceValidationService> _logger;

    public UserNotificationPreferenceValidationService(
        NotificationDbContext context,
        ILogger<UserNotificationPreferenceValidationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ValidateUserPreferencesAsync(Guid userId)
    {
        try
        {
            var preferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var duplicates = preferences
                .GroupBy(p => new { p.NotificationType, p.Channel })
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicates.Any())
            {
                _logger.LogWarning("Found duplicate preferences for user: {UserId}", userId);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user preferences");
            return false;
        }
    }

    public async Task<bool> ValidateChannelSettingsAsync(string channel, Dictionary<string, object> settings)
    {
        try
        {
            if (settings == null || !settings.Any())
                return true;

            // Basic validation - can be extended based on channel requirements
            switch (channel.ToLower())
            {
                case "email":
                    return ValidateEmailSettings(settings);
                case "sms":
                    return ValidateSmsSettings(settings);
                case "push":
                    return ValidatePushSettings(settings);
                case "in-app":
                    return ValidateInAppSettings(settings);
                default:
                    return true; // Allow unknown channels with any settings
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating channel settings");
            return false;
        }
    }

    public async Task<List<string>> GetMissingPreferencesForUserAsync(Guid userId)
    {
        try
        {
            var userPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == userId)
                .Select(p => new { p.NotificationType, p.Channel })
                .ToListAsync();

            var defaultPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId == Guid.Empty)
                .Select(p => new { p.NotificationType, p.Channel })
                .ToListAsync();

            var missing = defaultPreferences
                .Where(dp => !userPreferences.Any(up => up.NotificationType == dp.NotificationType && up.Channel == dp.Channel))
                .Select(dp => $"{dp.NotificationType}:{dp.Channel}")
                .ToList();

            return missing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting missing preferences for user");
            throw;
        }
    }

    public async Task<int> CleanupOrphanedPreferencesAsync()
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-1);
            var orphanedPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty && p.UpdatedAt < cutoffDate)
                .ToListAsync();

            var count = orphanedPreferences.Count;
            if (orphanedPreferences.Any())
            {
                _context.UserNotificationPreferences.RemoveRange(orphanedPreferences);
                await _context.SaveChangesAsync();
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up orphaned preferences");
            return 0;
        }
    }

    public async Task<bool> IsFrequencyAllowedAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var preference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.NotificationType == notificationType && 
                                         p.Channel == channel);

            if (preference == null || string.IsNullOrEmpty(preference.Frequency))
            {
                return true; // No frequency restriction
            }

            // Basic frequency validation - can be extended with more sophisticated logic
            return ValidateFrequency(preference.Frequency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking frequency for user: {UserId}", userId);
            return true; // Default to allowing if error occurs
        }
    }

    public async Task<TimeOnly?> GetPreferredTimeForUserAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var preference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.NotificationType == notificationType && 
                                         p.Channel == channel);

            return preference?.PreferredTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preferred time for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<string?> GetTimeZoneForUserAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var preference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.NotificationType == notificationType && 
                                         p.Channel == channel);

            return preference?.TimeZone;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timezone for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> IsWithinUserPreferredTimeAsync(Guid userId, string notificationType, string channel, DateTime scheduledTime)
    {
        try
        {
            var preferredTime = await GetPreferredTimeForUserAsync(userId, notificationType, channel);
            if (!preferredTime.HasValue)
                return true;

            var timeZone = await GetTimeZoneForUserAsync(userId, notificationType, channel);
            var userTime = timeZone != null 
                ? TimeZoneInfo.ConvertTimeBySystemTimeZoneId(scheduledTime, timeZone)
                : scheduledTime;

            var userTimeOnly = TimeOnly.FromDateTime(userTime);
            var timeDifference = Math.Abs((userTimeOnly - preferredTime.Value).TotalMinutes);

            return timeDifference <= 60; // Within 1 hour of preferred time
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking preferred time");
            return true; // Default to allowing if error occurs
        }
    }

    public async Task<Dictionary<string, object>?> GetChannelSettingsAsync(Guid userId, string notificationType, string channel)
    {
        try
        {
            var preference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.NotificationType == notificationType && 
                                         p.Channel == channel);

            if (preference?.Settings == null)
                return null;

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(preference.Settings);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse settings JSON for preference: {PreferenceId}", preference.Id);
                return new Dictionary<string, object>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel settings");
            throw;
        }
    }

    public async Task<bool> UpdateChannelSettingsAsync(Guid userId, string notificationType, string channel, Dictionary<string, object> settings)
    {
        try
        {
            if (!await ValidateChannelSettingsAsync(channel, settings))
            {
                _logger.LogWarning("Invalid channel settings for channel: {Channel}", channel);
                return false;
            }

            var preference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && 
                                         p.NotificationType == notificationType && 
                                         p.Channel == channel);

            if (preference == null)
                return false;

            preference.Settings = JsonSerializer.Serialize(settings);
            preference.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel settings");
            return false;
        }
    }

    public async Task<List<Guid>> GetUsersWithInvalidPreferencesAsync()
    {
        try
        {
            var usersWithDuplicates = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => new { p.UserId, p.NotificationType, p.Channel })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key.UserId)
                .Distinct()
                .ToListAsync();

            return usersWithDuplicates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with invalid preferences");
            throw;
        }
    }

    public async Task<int> FixDuplicatePreferencesAsync()
    {
        try
        {
            var duplicateGroups = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => new { p.UserId, p.NotificationType, p.Channel })
                .Where(g => g.Count() > 1)
                .ToListAsync();

            var fixedCount = 0;

            foreach (var group in duplicateGroups)
            {
                var preferences = group.OrderByDescending(p => p.UpdatedAt).ToList();
                var toKeep = preferences.First();
                var toRemove = preferences.Skip(1).ToList();

                _context.UserNotificationPreferences.RemoveRange(toRemove);
                fixedCount += toRemove.Count;
            }

            if (fixedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return fixedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing duplicate preferences");
            return 0;
        }
    }

    private bool ValidateEmailSettings(Dictionary<string, object> settings)
    {
        // Email-specific validation
        if (settings.ContainsKey("format"))
        {
            var format = settings["format"]?.ToString()?.ToLower();
            if (format != null && format != "html" && format != "text")
                return false;
        }

        return true;
    }

    private bool ValidateSmsSettings(Dictionary<string, object> settings)
    {
        // SMS-specific validation
        if (settings.ContainsKey("maxLength"))
        {
            if (settings["maxLength"] is JsonElement element && element.TryGetInt32(out int maxLength))
            {
                if (maxLength <= 0 || maxLength > 1600) // SMS length limits
                    return false;
            }
        }

        return true;
    }

    private bool ValidatePushSettings(Dictionary<string, object> settings)
    {
        // Push notification-specific validation
        if (settings.ContainsKey("sound"))
        {
            var sound = settings["sound"]?.ToString();
            if (string.IsNullOrEmpty(sound))
                return false;
        }

        return true;
    }

    private bool ValidateInAppSettings(Dictionary<string, object> settings)
    {
        // In-app notification-specific validation
        if (settings.ContainsKey("displayDuration"))
        {
            if (settings["displayDuration"] is JsonElement element && element.TryGetInt32(out int duration))
            {
                if (duration <= 0 || duration > 30000) // Max 30 seconds
                    return false;
            }
        }

        return true;
    }

    private bool ValidateFrequency(string frequency)
    {
        var validFrequencies = new[] { "immediate", "hourly", "daily", "weekly", "monthly" };
        return validFrequencies.Contains(frequency.ToLower());
    }
}
