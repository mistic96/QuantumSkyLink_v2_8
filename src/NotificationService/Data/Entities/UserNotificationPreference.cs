using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Data.Entities;

[Table("UserNotificationPreferences")]
[Index(nameof(UserId))]
[Index(nameof(NotificationType))]
[Index(nameof(Channel))]
public class UserNotificationPreference : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string NotificationType { get; set; } = string.Empty; // Security, Transaction, Governance, etc.

    [Required]
    [MaxLength(50)]
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, InApp

    [Required]
    public bool IsEnabled { get; set; } = true;

    [MaxLength(20)]
    public string? Frequency { get; set; } // Immediate, Daily, Weekly, Never

    [Column(TypeName = "time")]
    public TimeOnly? PreferredTime { get; set; } // For batched notifications

    [MaxLength(10)]
    public string? TimeZone { get; set; } // User's timezone preference

    [Column(TypeName = "jsonb")]
    public string? Settings { get; set; } // Additional channel-specific settings

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
