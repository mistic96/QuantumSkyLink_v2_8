using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data.Entities;

[Table("UserRoles")]
[Index(nameof(UserId), nameof(RoleId), IsUnique = true)]
public class UserRole
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    [Required]
    [ForeignKey("Role")]
    public Guid RoleId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(255)]
    public string? AssignedBy { get; set; }

    [MaxLength(500)]
    public string? AssignmentReason { get; set; }

    public DateTime? RemovedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
