namespace NotificationService.Models.Responses;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TemplateId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Recipient { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public string? ExternalId { get; set; }
    public string? Channel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public NotificationTemplateResponse? Template { get; set; }
    public List<NotificationDeliveryAttemptResponse>? DeliveryAttempts { get; set; }
}

public class NotificationTemplateResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public List<string> Variables { get; set; } = new();
    public bool IsActive { get; set; }
    public string? Category { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NotificationDeliveryAttemptResponse
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public int AttemptNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExternalId { get; set; }
    public Dictionary<string, object>? ResponseData { get; set; }
    public TimeSpan? ResponseTime { get; set; }
    public string? Provider { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserNotificationPreferenceResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? Frequency { get; set; }
    public TimeOnly? PreferredTime { get; set; }
    public string? TimeZone { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NotificationQueueResponse
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? ProcessingCompletedAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProcessorId { get; set; }
    public Dictionary<string, object>? ProcessingMetadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public NotificationResponse? Notification { get; set; }
}

public class SendNotificationResponse
{
    public Guid NotificationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public string? ExternalId { get; set; }
}

public class SendBulkNotificationResponse
{
    public int TotalRequested { get; set; }
    public int SuccessfullyQueued { get; set; }
    public int Failed { get; set; }
    public List<SendNotificationResponse> Results { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class NotificationStatsResponse
{
    public int TotalNotifications { get; set; }
    public int PendingNotifications { get; set; }
    public int SentNotifications { get; set; }
    public int FailedNotifications { get; set; }
    public int ReadNotifications { get; set; }
    public Dictionary<string, int> NotificationsByType { get; set; } = new();
    public Dictionary<string, int> NotificationsByStatus { get; set; } = new();
    public Dictionary<string, int> NotificationsByPriority { get; set; } = new();
    public double DeliverySuccessRate { get; set; }
    public double ReadRate { get; set; }
    public TimeSpan AverageDeliveryTime { get; set; }
}

public class NotificationHealthResponse
{
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
    public DateTime CheckedAt { get; set; }
    public List<string> Issues { get; set; } = new();
}

public class TemplateValidationResponse
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> MissingVariables { get; set; } = new();
    public List<string> UnusedVariables { get; set; } = new();
}
