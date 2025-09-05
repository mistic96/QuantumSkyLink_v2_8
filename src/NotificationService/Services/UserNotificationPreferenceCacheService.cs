using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NotificationService.Data.Entities;

namespace NotificationService.Services;

public class UserNotificationPreferenceCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserNotificationPreferenceCacheService> _logger;
    private const string USER_PREFERENCES_CACHE_KEY = "user_preferences:{0}";
    private const string DEFAULT_PREFERENCES_CACHE_KEY = "default_preferences";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public UserNotificationPreferenceCacheService(
        IDistributedCache cache,
        ILogger<UserNotificationPreferenceCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<UserNotificationPreference>?> GetUserPreferencesFromCacheAsync(Guid userId)
    {
        try
        {
            var cacheKey = string.Format(USER_PREFERENCES_CACHE_KEY, userId);
            var cachedPreferences = await _cache.GetStringAsync(cacheKey);
            
            if (!string.IsNullOrEmpty(cachedPreferences))
            {
                return JsonSerializer.Deserialize<List<UserNotificationPreference>>(cachedPreferences);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting user preferences from cache for user: {UserId}", userId);
            return null;
        }
    }

    public async Task SetUserPreferencesCacheAsync(Guid userId, List<UserNotificationPreference> preferences)
    {
        try
        {
            var cacheKey = string.Format(USER_PREFERENCES_CACHE_KEY, userId);
            var preferencesJson = JsonSerializer.Serialize(preferences);
            await _cache.SetStringAsync(cacheKey, preferencesJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting user preferences cache for user: {UserId}", userId);
        }
    }

    public async Task<List<UserNotificationPreference>?> GetDefaultPreferencesFromCacheAsync()
    {
        try
        {
            var cachedDefaults = await _cache.GetStringAsync(DEFAULT_PREFERENCES_CACHE_KEY);
            
            if (!string.IsNullOrEmpty(cachedDefaults))
            {
                return JsonSerializer.Deserialize<List<UserNotificationPreference>>(cachedDefaults);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting default preferences from cache");
            return null;
        }
    }

    public async Task SetDefaultPreferencesCacheAsync(List<UserNotificationPreference> defaultPreferences)
    {
        try
        {
            var defaultsJson = JsonSerializer.Serialize(defaultPreferences);
            await _cache.SetStringAsync(DEFAULT_PREFERENCES_CACHE_KEY, defaultsJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting default preferences cache");
        }
    }

    public async Task InvalidateUserPreferencesCacheAsync(Guid userId)
    {
        try
        {
            var cacheKey = string.Format(USER_PREFERENCES_CACHE_KEY, userId);
            await _cache.RemoveAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating user preferences cache for user: {UserId}", userId);
        }
    }

    public async Task InvalidateDefaultPreferencesCacheAsync()
    {
        try
        {
            await _cache.RemoveAsync(DEFAULT_PREFERENCES_CACHE_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating default preferences cache");
        }
    }

    public string GetUserPreferencesCacheKey(Guid userId)
    {
        return string.Format(USER_PREFERENCES_CACHE_KEY, userId);
    }

    public string GetDefaultPreferencesCacheKey()
    {
        return DEFAULT_PREFERENCES_CACHE_KEY;
    }
}
