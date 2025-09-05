using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class UpdateRoleRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    [Range(0, 100)]
    public int? Priority { get; set; }

    public List<Guid>? PermissionIds { get; set; }

    public Guid? UpdatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
