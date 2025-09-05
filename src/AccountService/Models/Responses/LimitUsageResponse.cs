using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class LimitUsageResponse
{
    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal CurrentUsage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RemainingLimit { get; set; }

    public decimal UsagePercentage { get; set; }

    [StringLength(50)]
    public string Period { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public DateTime LastUpdated { get; set; }

    public int TransactionCount { get; set; }

    public bool IsNearLimit { get; set; }

    public bool IsExceeded { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal UsedAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RemainingAmount { get; set; }
}
