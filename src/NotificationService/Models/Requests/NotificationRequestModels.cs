using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.Requests;

public class SendNotificationRequest
{
    [Required]
    public Guid UserId { get; set; }

    public Guid? TemplateId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Email, SMS, Push, InApp

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? HtmlBody { get; set; }

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

    [MaxLength(100)]
    public string? Recipient { get; set; } // Override recipient

    public Dictionary<string, object>? Variables { get; set; } // Template variables

    public DateTime? ScheduledAt { get; set; }

    [MaxLength(50)]
    public string? Channel { get; set; } // Specific channel within type

    public Dictionary<string, object>? Metadata { get; set; }
}

public class SendBulkNotificationRequest
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();

    public Guid? TemplateId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? HtmlBody { get; set; }

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal";

    public Dictionary<string, object>? Variables { get; set; }

    public DateTime? ScheduledAt { get; set; }

    [MaxLength(50)]
    public string? Channel { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}

public class GetNotificationsRequest
{
    public Guid? UserId { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "desc";
}

public class UpdateNotificationStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}

public class MarkNotificationReadRequest
{
    [Required]
    public Guid NotificationId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}

public class CreateNotificationTemplateRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? HtmlBody { get; set; }

    public List<string>? Variables { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal";

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateNotificationTemplateRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? HtmlBody { get; set; }

    public List<string>? Variables { get; set; }

    public bool? IsActive { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class GetNotificationTemplatesRequest
{
    public string? Type { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "Name";
    public string? SortDirection { get; set; } = "asc";
}

public class UpdateUserNotificationPreferencesRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public List<UserNotificationPreferenceRequest> Preferences { get; set; } = new();
}

public class UserNotificationPreferenceRequest
{
    [Required]
    [MaxLength(50)]
    public string NotificationType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Channel { get; set; } = string.Empty;

    [Required]
    public bool IsEnabled { get; set; }

    [MaxLength(20)]
    public string? Frequency { get; set; }

    public TimeOnly? PreferredTime { get; set; }

    [MaxLength(10)]
    public string? TimeZone { get; set; }

    public Dictionary<string, object>? Settings { get; set; }
}

public class GetUserNotificationPreferencesRequest
{
    [Required]
    public Guid UserId { get; set; }

    public string? NotificationType { get; set; }
    public string? Channel { get; set; }
}

public class GetQueuedNotificationsRequest
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? NotificationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "ScheduledAt";
    public string? SortDirection { get; set; } = "asc";
}
