using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Data.Entities;

[Table("NotificationQueue")]
[Index(nameof(Status))]
[Index(nameof(Priority))]
[Index(nameof(ScheduledAt))]
[Index(nameof(ProcessingStartedAt))]
[Index(nameof(CreatedAt))]
public class NotificationQueue : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("Notification")]
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Queued"; // Queued, Processing, Completed, Failed, Cancelled

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

    [Required]
    public DateTime ScheduledAt { get; set; }

    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? ProcessingCompletedAt { get; set; }

    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;

    public DateTime? NextRetryAt { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }

    [MaxLength(100)]
    public string? ProcessorId { get; set; } // ID of the worker processing this item

    [Column(TypeName = "jsonb")]
    public string? ProcessingMetadata { get; set; } // Additional processing information

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
