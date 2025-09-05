using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class UsageUpdateResponse
{
    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal PreviousUsage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal UpdateAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal NewUsage { get; set; }

    [StringLength(50)]
    public string Operation { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(100)]
    public string? TransactionReference { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(50)]
    public string? Period { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RemainingLimit { get; set; }

    public decimal UsagePercentage { get; set; }

    public bool IsNearLimit { get; set; } = false;

    public bool IsOverLimit { get; set; } = false;

    [StringLength(500)]
    public string? ValidationMessage { get; set; }

    public bool Success { get; set; } = true;
}
