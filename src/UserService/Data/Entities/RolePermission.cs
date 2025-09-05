using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data.Entities;

[Table("RolePermissions")]
[Index(nameof(RoleId), nameof(PermissionId), IsUnique = true)]
public class RolePermission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("Role")]
    public Guid RoleId { get; set; }

    [Required]
    [ForeignKey("Permission")]
    public Guid PermissionId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [MaxLength(255)]
    public string? AssignedBy { get; set; }

    [MaxLength(500)]
    public string? AssignmentReason { get; set; }

    // Navigation Properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
