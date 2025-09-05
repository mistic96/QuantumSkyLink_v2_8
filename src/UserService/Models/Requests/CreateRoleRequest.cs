using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class CreateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [StringLength(50)]
    public string? Category { get; set; }

    [Range(0, 100)]
    public int Priority { get; set; } = 0;

    public List<Guid> PermissionIds { get; set; } = new();

    public Guid? CreatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
