using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class LimitValidationResponse
{
    public bool IsValid { get; set; }

    public bool IsWithinLimit { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RequestedAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal CurrentUsage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RemainingLimit { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal ExceededBy { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [StringLength(50)]
    public string Period { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? ValidationMessage { get; set; }

    [StringLength(500)]
    public string? RecommendedAction { get; set; }

    public List<string> Warnings { get; set; } = new();

    public List<string> Errors { get; set; } = new();

    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    public bool RequiresApproval { get; set; }

    public Guid? ApproverId { get; set; }

    public List<string> Violations { get; set; } = new();

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
