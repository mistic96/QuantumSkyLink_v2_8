using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class AccountLimitResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [StringLength(10)]
    public string Currency { get; set; } = "USD";

    [StringLength(50)]
    public string Period { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool RequiresApproval { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? WarningThreshold { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal CurrentUsage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RemainingLimit { get; set; }

    public decimal UsagePercentage { get; set; }

    public bool IsNearLimit { get; set; }

    public bool IsExceeded { get; set; }

    public DateTime? LastUsageUpdate { get; set; }

    public DateTime? LastResetAt { get; set; }
}
