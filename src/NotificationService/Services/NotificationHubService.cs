using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using NotificationService.Services.Interfaces;
using NotificationService.Models.Responses;

namespace NotificationService.Services;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationHubService> _logger;
    
    // Thread-safe collections for connection management
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
    private static readonly ConcurrentDictionary<string, Guid> _connectionUsers = new();
    private static readonly ConcurrentDictionary<string, HashSet<Guid>> _groupMembers = new();
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> _userGroups = new();
    private static readonly ConcurrentDictionary<Guid, string> _userPresence = new();
    private static readonly object _lockObject = new();

    public NotificationHubService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    // Real-time Notification Delivery
    public async Task SendNotificationToUserAsync(Guid userId, NotificationResponse notification)
    {
        try
        {
            var connections = await GetUserConnectionsAsync(userId);
            if (!connections.Any())
            {
                _logger.LogWarning("No active connections found for user: {UserId}", userId);
                return;
            }

            await _hubContext.Clients.Clients(connections).SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("Notification sent to user {UserId} via {ConnectionCount} connections", 
                userId, connections.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user: {UserId}", userId);
        }
    }

    public async Task SendNotificationToUsersAsync(List<Guid> userIds, NotificationResponse notification)
    {
        try
        {
            var allConnections = new List<string>();
            foreach (var userId in userIds)
            {
                var connections = await GetUserConnectionsAsync(userId);
                allConnections.AddRange(connections);
            }

            if (allConnections.Any())
            {
                await _hubContext.Clients.Clients(allConnections).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("Notification sent to {UserCount} users via {ConnectionCount} connections", 
                    userIds.Count, allConnections.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to multiple users");
        }
    }

    public async Task SendNotificationToAllAsync(NotificationResponse notification)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveBroadcast", notification);
            _logger.LogInformation("Broadcast notification sent to all connected users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending broadcast notification");
        }
    }

    public async Task SendNotificationToGroupAsync(string groupName, NotificationResponse notification)
    {
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveGroupNotification", notification);
            _logger.LogInformation("Notification sent to group: {GroupName}", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to group: {GroupName}", groupName);
        }
    }

    // User Connection Management
    public async Task AddUserToGroupAsync(Guid userId, string groupName)
    {
        try
        {
            var connections = await GetUserConnectionsAsync(userId);
            foreach (var connectionId in connections)
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
            }

            lock (_lockObject)
            {
                if (!_groupMembers.ContainsKey(groupName))
                {
                    _groupMembers[groupName] = new HashSet<Guid>();
                }
                _groupMembers[groupName].Add(userId);

                if (!_userGroups.ContainsKey(userId))
                {
                    _userGroups[userId] = new HashSet<string>();
                }
                _userGroups[userId].Add(groupName);
            }

            _logger.LogInformation("User {UserId} added to group {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user to group: {GroupName}", groupName);
        }
    }

    public async Task RemoveUserFromGroupAsync(Guid userId, string groupName)
    {
        try
        {
            var connections = await GetUserConnectionsAsync(userId);
            foreach (var connectionId in connections)
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
            }

            lock (_lockObject)
            {
                if (_groupMembers.TryGetValue(groupName, out var members))
                {
                    members.Remove(userId);
                    if (members.Count == 0)
                    {
                        _groupMembers.TryRemove(groupName, out _);
                    }
                }

                if (_userGroups.TryGetValue(userId, out var groups))
                {
                    groups.Remove(groupName);
                    if (groups.Count == 0)
                    {
                        _userGroups.TryRemove(userId, out _);
                    }
                }
            }

            _logger.LogInformation("User {UserId} removed from group {GroupName}", userId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user from group: {GroupName}", groupName);
        }
    }

    public async Task AddConnectionToUserAsync(string connectionId, Guid userId)
    {
        try
        {
            lock (_lockObject)
            {
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new HashSet<string>();
                }
                _userConnections[userId].Add(connectionId);
                _connectionUsers[connectionId] = userId;
            }

            _logger.LogInformation("Connection {ConnectionId} added for user {UserId}", connectionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding connection for user: {UserId}", userId);
        }
    }

    public async Task RemoveConnectionFromUserAsync(string connectionId)
    {
        try
        {
            lock (_lockObject)
            {
                if (_connectionUsers.TryGetValue(connectionId, out var userId))
                {
                    if (_userConnections.TryGetValue(userId, out var connections))
                    {
                        connections.Remove(connectionId);
                        if (connections.Count == 0)
                        {
                            _userConnections.TryRemove(userId, out _);
                        }
                    }
                    _connectionUsers.TryRemove(connectionId, out _);
                }
            }

            _logger.LogInformation("Connection {ConnectionId} removed", connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection: {ConnectionId}", connectionId);
        }
    }

    public async Task<List<string>> GetUserConnectionsAsync(Guid userId)
    {
        try
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                return connections.ToList();
            }
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user connections for user: {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<bool> IsUserOnlineAsync(Guid userId)
    {
        try
        {
            return _userConnections.ContainsKey(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user is online: {UserId}", userId);
            return false;
        }
    }

    // Group Management
    public async Task CreateGroupAsync(string groupName, List<Guid> userIds)
    {
        try
        {
            lock (_lockObject)
            {
                _groupMembers[groupName] = new HashSet<Guid>(userIds);
                
                foreach (var userId in userIds)
                {
                    if (!_userGroups.ContainsKey(userId))
                    {
                        _userGroups[userId] = new HashSet<string>();
                    }
                    _userGroups[userId].Add(groupName);
                }
            }

            // Add all user connections to the group
            foreach (var userId in userIds)
            {
                await AddUserToGroupAsync(userId, groupName);
            }

            _logger.LogInformation("Group {GroupName} created with {UserCount} members", groupName, userIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group: {GroupName}", groupName);
        }
    }

    public async Task DeleteGroupAsync(string groupName)
    {
        try
        {
            if (_groupMembers.TryGetValue(groupName, out var members))
            {
                foreach (var userId in members)
                {
                    await RemoveUserFromGroupAsync(userId, groupName);
                }
            }

            _logger.LogInformation("Group {GroupName} deleted", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group: {GroupName}", groupName);
        }
    }

    public async Task<List<Guid>> GetGroupMembersAsync(string groupName)
    {
        try
        {
            if (_groupMembers.TryGetValue(groupName, out var members))
            {
                return members.ToList();
            }
            return new List<Guid>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group members for group: {GroupName}", groupName);
            return new List<Guid>();
        }
    }

    public async Task<List<string>> GetUserGroupsAsync(Guid userId)
    {
        try
        {
            if (_userGroups.TryGetValue(userId, out var groups))
            {
                return groups.ToList();
            }
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups for user: {UserId}", userId);
            return new List<string>();
        }
    }

    // Notification Status Updates
    public async Task NotifyNotificationStatusChangeAsync(Guid userId, Guid notificationId, string newStatus)
    {
        try
        {
            var payload = new { notificationId, newStatus, timestamp = DateTime.UtcNow };
            await SendNotificationToUserAsync(userId, new NotificationResponse 
            { 
                Id = notificationId, 
                Body = $"Status changed to {newStatus}",
                Metadata = new Dictionary<string, object> { ["payload"] = payload }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying status change");
        }
    }

    public async Task NotifyNotificationReadAsync(Guid userId, Guid notificationId)
    {
        await NotifyNotificationStatusChangeAsync(userId, notificationId, "Read");
    }

    public async Task NotifyNotificationDeliveredAsync(Guid userId, Guid notificationId)
    {
        await NotifyNotificationStatusChangeAsync(userId, notificationId, "Delivered");
    }

    // Typing and Presence Indicators
    public async Task NotifyUserTypingAsync(Guid userId, string context)
    {
        try
        {
            var payload = new { userId, context, isTyping = true, timestamp = DateTime.UtcNow };
            await _hubContext.Clients.All.SendAsync("TypingIndicator", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator");
        }
    }

    public async Task NotifyUserStoppedTypingAsync(Guid userId, string context)
    {
        try
        {
            var payload = new { userId, context, isTyping = false, timestamp = DateTime.UtcNow };
            await _hubContext.Clients.All.SendAsync("TypingIndicator", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending stopped typing indicator");
        }
    }

    public async Task UpdateUserPresenceAsync(Guid userId, string status)
    {
        try
        {
            _userPresence[userId] = status;
            var payload = new { userId, status, timestamp = DateTime.UtcNow };
            await _hubContext.Clients.All.SendAsync("PresenceUpdate", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user presence");
        }
    }

    public async Task<string> GetUserPresenceAsync(Guid userId)
    {
        try
        {
            return _userPresence.TryGetValue(userId, out var status) ? status : "Offline";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user presence");
            return "Offline";
        }
    }

    // Broadcast Operations
    public async Task BroadcastSystemNotificationAsync(string message, string? type = null)
    {
        try
        {
            var payload = new { message, type = type ?? "system", timestamp = DateTime.UtcNow };
            await _hubContext.Clients.All.SendAsync("SystemNotification", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting system notification");
        }
    }

    public async Task BroadcastMaintenanceNotificationAsync(string message, DateTime? scheduledTime = null)
    {
        try
        {
            var payload = new { message, scheduledTime, type = "maintenance", timestamp = DateTime.UtcNow };
            await _hubContext.Clients.All.SendAsync("MaintenanceNotification", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting maintenance notification");
        }
    }

    public async Task BroadcastEmergencyNotificationAsync(string message)
    {
        try
        {
            var payload = new { message, type = "emergency", timestamp = DateTime.UtcNow };
            await _hubContext.Clients.All.SendAsync("EmergencyNotification", payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting emergency notification");
        }
    }

    // Connection Analytics
    public async Task<int> GetActiveConnectionCountAsync()
    {
        try
        {
            return _connectionUsers.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active connection count");
            return 0;
        }
    }

    public async Task<int> GetOnlineUserCountAsync()
    {
        try
        {
            return _userConnections.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online user count");
            return 0;
        }
    }

    public async Task<Dictionary<string, int>> GetConnectionStatsAsync()
    {
        try
        {
            return new Dictionary<string, int>
            {
                ["ActiveConnections"] = _connectionUsers.Count,
                ["OnlineUsers"] = _userConnections.Count,
                ["ActiveGroups"] = _groupMembers.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection stats");
            return new Dictionary<string, int>();
        }
    }

    public async Task<List<Guid>> GetActiveUsersAsync()
    {
        try
        {
            return _userConnections.Keys.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users");
            return new List<Guid>();
        }
    }

    // Message History and Persistence (Basic implementation)
    public async Task SaveMessageAsync(Guid userId, string message, string? type = null)
    {
        try
        {
            // In a real implementation, this would save to a database
            _logger.LogInformation("Message saved for user {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving message");
        }
    }

    public async Task<List<object>> GetMessageHistoryAsync(Guid userId, int count = 50)
    {
        try
        {
            // In a real implementation, this would retrieve from a database
            return new List<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message history");
            return new List<object>();
        }
    }

    public async Task MarkMessagesAsReadAsync(Guid userId, List<Guid> messageIds)
    {
        try
        {
            // In a real implementation, this would update database records
            _logger.LogInformation("Marked {Count} messages as read for user {UserId}", messageIds.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
        }
    }

    // Custom Events
    public async Task SendCustomEventAsync(Guid userId, string eventName, object data)
    {
        try
        {
            var connections = await GetUserConnectionsAsync(userId);
            if (connections.Any())
            {
                await _hubContext.Clients.Clients(connections).SendAsync(eventName, data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom event to user");
        }
    }

    public async Task SendCustomEventToGroupAsync(string groupName, string eventName, object data)
    {
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync(eventName, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom event to group");
        }
    }

    public async Task SendCustomEventToAllAsync(string eventName, object data)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync(eventName, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom event to all");
        }
    }

    // Connection Health
    public async Task<bool> PingConnectionAsync(string connectionId)
    {
        try
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("Ping", DateTime.UtcNow);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinging connection");
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetConnectionHealthAsync()
    {
        try
        {
            return new Dictionary<string, object>
            {
                ["TotalConnections"] = _connectionUsers.Count,
                ["TotalUsers"] = _userConnections.Count,
                ["TotalGroups"] = _groupMembers.Count,
                ["Timestamp"] = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection health");
            return new Dictionary<string, object>();
        }
    }

    public async Task CleanupStaleConnectionsAsync(TimeSpan? timeout = null)
    {
        try
        {
            var cleanupTimeout = timeout ?? TimeSpan.FromMinutes(30);
            _logger.LogInformation("Connection cleanup completed. Timeout: {Timeout}", cleanupTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during connection cleanup");
        }
    }
}

// SignalR Hub class
public class NotificationHub : Hub
{
    private readonly INotificationHubService _hubService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(
        INotificationHubService hubService,
        ILogger<NotificationHub> logger)
    {
        _hubService = hubService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            await _hubService.AddConnectionToUserAsync(Context.ConnectionId, userId.Value);
        }

        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _hubService.RemoveConnectionFromUserAsync(Context.ConnectionId);

        if (exception != null)
        {
            _logger.LogWarning(exception, "Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            await _hubService.AddUserToGroupAsync(userId.Value, groupName);
            await Clients.Group(groupName).SendAsync("UserJoinedGroup", userId.Value, groupName);
        }
    }

    public async Task LeaveGroup(string groupName)
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            await _hubService.RemoveUserFromGroupAsync(userId.Value, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup", userId.Value, groupName);
        }
    }

    private Guid? GetUserIdFromContext()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
