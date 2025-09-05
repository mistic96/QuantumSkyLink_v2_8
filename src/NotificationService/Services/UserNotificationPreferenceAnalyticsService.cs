using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationService.Data;

namespace NotificationService.Services;

public class UserNotificationPreferenceAnalyticsService
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<UserNotificationPreferenceAnalyticsService> _logger;

    public UserNotificationPreferenceAnalyticsService(
        NotificationDbContext context,
        ILogger<UserNotificationPreferenceAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Dictionary<string, int>> GetPreferenceStatsAsync()
    {
        try
        {
            var stats = new Dictionary<string, int>();

            var totalUsers = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .Select(p => p.UserId)
                .Distinct()
                .CountAsync();

            var enabledPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty && p.IsEnabled)
                .CountAsync();

            var totalPreferences = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .CountAsync();

            stats["TotalUsers"] = totalUsers;
            stats["EnabledPreferences"] = enabledPreferences;
            stats["TotalPreferences"] = totalPreferences;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preference stats");
            throw;
        }
    }

    public async Task<Dictionary<string, double>> GetChannelAdoptionRatesAsync()
    {
        try
        {
            var adoptionRates = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => p.Channel)
                .Select(g => new { Channel = g.Key, EnabledCount = g.Count(p => p.IsEnabled), TotalCount = g.Count() })
                .ToDictionaryAsync(x => x.Channel, x => x.TotalCount > 0 ? (double)x.EnabledCount / x.TotalCount : 0);

            return adoptionRates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel adoption rates");
            throw;
        }
    }

    public async Task<Dictionary<string, double>> GetNotificationTypeOptInRatesAsync()
    {
        try
        {
            var optInRates = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => p.NotificationType)
                .Select(g => new { Type = g.Key, EnabledCount = g.Count(p => p.IsEnabled), TotalCount = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.TotalCount > 0 ? (double)x.EnabledCount / x.TotalCount : 0);

            return optInRates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification type opt-in rates");
            throw;
        }
    }

    public async Task<List<Guid>> GetUsersWithDisabledNotificationsAsync(string? notificationType = null, string? channel = null)
    {
        try
        {
            var query = _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty && !p.IsEnabled);

            if (!string.IsNullOrEmpty(notificationType))
                query = query.Where(p => p.NotificationType == notificationType);
            if (!string.IsNullOrEmpty(channel))
                query = query.Where(p => p.Channel == channel);

            return await query.Select(p => p.UserId).Distinct().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with disabled notifications");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetPreferenceCountsByChannelAsync()
    {
        try
        {
            var channelCounts = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => p.Channel)
                .Select(g => new { Channel = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Channel, x => x.Count);

            return channelCounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preference counts by channel");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetPreferenceCountsByNotificationTypeAsync()
    {
        try
        {
            var typeCounts = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => p.NotificationType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            return typeCounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preference counts by notification type");
            throw;
        }
    }

    public async Task<Dictionary<string, double>> GetAveragePreferencesPerUserAsync()
    {
        try
        {
            var userPreferenceCounts = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalUsers = userPreferenceCounts.Count;
            var totalPreferences = userPreferenceCounts.Sum(x => x.Count);

            var result = new Dictionary<string, double>
            {
                ["AveragePreferencesPerUser"] = totalUsers > 0 ? (double)totalPreferences / totalUsers : 0,
                ["TotalUsers"] = totalUsers,
                ["TotalPreferences"] = totalPreferences
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average preferences per user");
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetComprehensiveAnalyticsAsync()
    {
        try
        {
            var analytics = new Dictionary<string, object>();

            // Basic stats
            var basicStats = await GetPreferenceStatsAsync();
            analytics["BasicStats"] = basicStats;

            // Channel adoption rates
            var channelRates = await GetChannelAdoptionRatesAsync();
            analytics["ChannelAdoptionRates"] = channelRates;

            // Notification type opt-in rates
            var typeRates = await GetNotificationTypeOptInRatesAsync();
            analytics["NotificationTypeOptInRates"] = typeRates;

            // Channel counts
            var channelCounts = await GetPreferenceCountsByChannelAsync();
            analytics["ChannelCounts"] = channelCounts;

            // Type counts
            var typeCounts = await GetPreferenceCountsByNotificationTypeAsync();
            analytics["TypeCounts"] = typeCounts;

            // Average preferences per user
            var averageStats = await GetAveragePreferencesPerUserAsync();
            analytics["AverageStats"] = averageStats;

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comprehensive analytics");
            throw;
        }
    }

    public async Task<List<Guid>> GetMostActiveUsersAsync(int topCount = 10)
    {
        try
        {
            var activeUsers = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty && p.IsEnabled)
                .GroupBy(p => p.UserId)
                .OrderByDescending(g => g.Count())
                .Take(topCount)
                .Select(g => g.Key)
                .ToListAsync();

            return activeUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most active users");
            throw;
        }
    }

    public async Task<List<Guid>> GetLeastActiveUsersAsync(int topCount = 10)
    {
        try
        {
            var inactiveUsers = await _context.UserNotificationPreferences
                .Where(p => p.UserId != Guid.Empty)
                .GroupBy(p => p.UserId)
                .OrderBy(g => g.Count(x => x.IsEnabled))
                .Take(topCount)
                .Select(g => g.Key)
                .ToListAsync();

            return inactiveUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting least active users");
            throw;
        }
    }
}
