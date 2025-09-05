using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Data.Entities;

[Table("NotificationTemplates")]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Type))]
[Index(nameof(IsActive))]
public class NotificationTemplate : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

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

    [Column(TypeName = "jsonb")]
    public string Variables { get; set; } = "{}"; // JSON array of variable names

    [Required]
    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string? Category { get; set; }

    [MaxLength(20)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
