using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Data.Entities;

[Table("NotificationDeliveryAttempts")]
[Index(nameof(NotificationId))]
[Index(nameof(Status))]
[Index(nameof(AttemptedAt))]
public class NotificationDeliveryAttempt : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("Notification")]
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;

    [Required]
    public int AttemptNumber { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // Success, Failed, Retry

    [Required]
    public DateTime AttemptedAt { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    [MaxLength(100)]
    public string? ExternalId { get; set; } // Response ID from external service

    [Column(TypeName = "jsonb")]
    public string? ResponseData { get; set; } // Full response from external service

    public TimeSpan? ResponseTime { get; set; }

    [MaxLength(100)]
    public string? Provider { get; set; } // SendGrid, Twilio, Firebase, etc.

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
