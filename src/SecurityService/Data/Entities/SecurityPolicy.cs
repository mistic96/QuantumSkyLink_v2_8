using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SecurityService.Data.Entities;

[Table("SecurityPolicies")]
[Index(nameof(UserId), nameof(PolicyType), IsUnique = false)]
[Index(nameof(UserId), nameof(IsActive), IsUnique = false)]
public class SecurityPolicy
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string PolicyType { get; set; } = string.Empty; // MFA, MultiSig, SessionTimeout, etc.

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "jsonb")]
    public string Configuration { get; set; } = "{}";

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public bool IsRequired { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CorrelationId { get; set; } = string.Empty;
}
