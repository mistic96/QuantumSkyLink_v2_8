using Refit;

namespace TreasuryService.Services.Interfaces;

[Headers("Accept: application/json", "X-API-Version: 1.0")]
public interface INotificationServiceClient
{
    /// <summary>
    /// Send a notification to a user
    /// </summary>
    [Post("/api/notifications/send")]
    Task<NotificationResponse> SendNotificationAsync([Body] NotificationRequest request);

    /// <summary>
    /// Send a bulk notification to multiple users
    /// </summary>
    [Post("/api/notifications/send-bulk")]
    Task<BulkNotificationResponse> SendBulkNotificationAsync([Body] BulkNotificationRequest request);

    /// <summary>
    /// Get notification status
    /// </summary>
    [Get("/api/notifications/{notificationId}")]
    Task<NotificationStatus> GetNotificationStatusAsync(Guid notificationId);
}

public class NotificationRequest
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical
    public List<string> Channels { get; set; } = new(); // Email, SMS, Push, InApp
}

public class BulkNotificationRequest
{
    public List<Guid> UserIds { get; set; } = new();
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public string Priority { get; set; } = "Normal";
    public List<string> Channels { get; set; } = new();
}

public class NotificationResponse
{
    public Guid NotificationId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public List<string> ChannelsUsed { get; set; } = new();
}

public class BulkNotificationResponse
{
    public Guid BatchId { get; set; }
    public bool Success { get; set; }
    public int TotalRecipients { get; set; }
    public int SuccessfulSends { get; set; }
    public int FailedSends { get; set; }
    public List<NotificationResponse> Results { get; set; } = new();
}

public class NotificationStatus
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Sent, Delivered, Failed, Read
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public List<string> ChannelsUsed { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
