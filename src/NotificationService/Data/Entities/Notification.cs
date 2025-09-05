using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Data.Entities;

[Table("Notifications")]
[Index(nameof(UserId))]
[Index(nameof(Status))]
[Index(nameof(Type))]
[Index(nameof(Priority))]
[Index(nameof(ScheduledAt))]
[Index(nameof(CreatedAt))]
public class Notification : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("NotificationTemplate")]
    public Guid? TemplateId { get; set; }
    public NotificationTemplate? Template { get; set; }

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Email, SMS, Push, InApp

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "text")]
    public string Body { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string? HtmlBody { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Cancelled

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

    [MaxLength(100)]
    public string? Recipient { get; set; } // Email address, phone number, device token

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; } // Additional data for the notification

    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; } = 0;

    [MaxLength(100)]
    public string? ExternalId { get; set; } // ID from external service (SendGrid, Twilio, etc.)

    [MaxLength(50)]
    public string? Channel { get; set; } // Specific channel within type

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<NotificationDeliveryAttempt> DeliveryAttempts { get; set; } = new List<NotificationDeliveryAttempt>();
}
