using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class LimitUsageStatistic
{
    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal UsedAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RemainingAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal UsagePercentage { get; set; }

    public int TransactionCount { get; set; }

    public DateTime LastUsed { get; set; }

    [StringLength(50)]
    public string Period { get; set; } = string.Empty; // Daily, Weekly, Monthly, Yearly

    public bool IsExceeded { get; set; } = false;

    public bool IsNearLimit { get; set; } = false;

    [Column(TypeName = "decimal(18,8)")]
    public decimal WarningThreshold { get; set; } = 0.8m; // 80% default

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
