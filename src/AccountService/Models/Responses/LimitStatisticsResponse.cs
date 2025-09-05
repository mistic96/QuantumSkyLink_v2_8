using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class LimitStatisticsResponse
{
    public Guid AccountId { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public int TotalLimits { get; set; }

    public int ActiveLimits { get; set; }

    public int ExceededLimits { get; set; }

    public int NearLimitWarnings { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalLimitAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalUsage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal AverageUsagePercentage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal HighestUsagePercentage { get; set; }

    [StringLength(100)]
    public string? MostUsedLimitType { get; set; }

    [StringLength(100)]
    public string? LeastUsedLimitType { get; set; }

    public int LimitViolations { get; set; }

    public int LimitAdjustments { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalLimitIncreases { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalLimitDecreases { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public List<LimitUsageStatistic> UsageStatistics { get; set; } = new();

    public List<string> TopLimitTypes { get; set; } = new();

    public Dictionary<string, decimal> LimitTypeUsage { get; set; } = new();
}
