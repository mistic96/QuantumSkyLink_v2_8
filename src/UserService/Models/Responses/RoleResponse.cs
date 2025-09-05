using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class RoleResponse
{
    public Guid Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public int UserCount { get; set; }

    public int PermissionCount { get; set; }

    public IEnumerable<PermissionResponse> Permissions { get; set; } = new List<PermissionResponse>();
}
