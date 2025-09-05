using NotificationService.Models.Responses;

namespace NotificationService.Services.Interfaces;

public interface INotificationHubService
{
    // Real-time Notification Delivery
    Task SendNotificationToUserAsync(Guid userId, NotificationResponse notification);
    Task SendNotificationToUsersAsync(List<Guid> userIds, NotificationResponse notification);
    Task SendNotificationToAllAsync(NotificationResponse notification);
    Task SendNotificationToGroupAsync(string groupName, NotificationResponse notification);

    // User Connection Management
    Task AddUserToGroupAsync(Guid userId, string groupName);
    Task RemoveUserFromGroupAsync(Guid userId, string groupName);
    Task AddConnectionToUserAsync(string connectionId, Guid userId);
    Task RemoveConnectionFromUserAsync(string connectionId);
    Task<List<string>> GetUserConnectionsAsync(Guid userId);
    Task<bool> IsUserOnlineAsync(Guid userId);

    // Group Management
    Task CreateGroupAsync(string groupName, List<Guid> userIds);
    Task DeleteGroupAsync(string groupName);
    Task<List<Guid>> GetGroupMembersAsync(string groupName);
    Task<List<string>> GetUserGroupsAsync(Guid userId);

    // Notification Status Updates
    Task NotifyNotificationStatusChangeAsync(Guid userId, Guid notificationId, string newStatus);
    Task NotifyNotificationReadAsync(Guid userId, Guid notificationId);
    Task NotifyNotificationDeliveredAsync(Guid userId, Guid notificationId);

    // Typing and Presence Indicators
    Task NotifyUserTypingAsync(Guid userId, string context);
    Task NotifyUserStoppedTypingAsync(Guid userId, string context);
    Task UpdateUserPresenceAsync(Guid userId, string status); // Online, Away, Busy, Offline
    Task<string> GetUserPresenceAsync(Guid userId);

    // Broadcast Operations
    Task BroadcastSystemNotificationAsync(string message, string? type = null);
    Task BroadcastMaintenanceNotificationAsync(string message, DateTime? scheduledTime = null);
    Task BroadcastEmergencyNotificationAsync(string message);

    // Connection Analytics
    Task<int> GetActiveConnectionCountAsync();
    Task<int> GetOnlineUserCountAsync();
    Task<Dictionary<string, int>> GetConnectionStatsAsync();
    Task<List<Guid>> GetActiveUsersAsync();

    // Message History and Persistence
    Task SaveMessageAsync(Guid userId, string message, string? type = null);
    Task<List<object>> GetMessageHistoryAsync(Guid userId, int count = 50);
    Task MarkMessagesAsReadAsync(Guid userId, List<Guid> messageIds);

    // Custom Events
    Task SendCustomEventAsync(Guid userId, string eventName, object data);
    Task SendCustomEventToGroupAsync(string groupName, string eventName, object data);
    Task SendCustomEventToAllAsync(string eventName, object data);

    // Connection Health
    Task<bool> PingConnectionAsync(string connectionId);
    Task<Dictionary<string, object>> GetConnectionHealthAsync();
    Task CleanupStaleConnectionsAsync(TimeSpan? timeout = null);
}
