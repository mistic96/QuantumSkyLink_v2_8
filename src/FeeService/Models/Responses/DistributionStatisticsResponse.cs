namespace FeeService.Models.Responses;

public class DistributionStatisticsResponse
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? FeeType { get; set; }
    public decimal TotalDistributed { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int TotalDistributions { get; set; }
    public int CompletedDistributions { get; set; }
    public int PendingDistributions { get; set; }
    public int FailedDistributions { get; set; }
    public decimal AverageDistributionAmount { get; set; }
    public IEnumerable<RecipientStatistics> RecipientBreakdown { get; set; } = new List<RecipientStatistics>();
    public IEnumerable<FeeTypeStatistics> FeeTypeBreakdown { get; set; } = new List<FeeTypeStatistics>();
    public DateTime GeneratedAt { get; set; }
}

public class RecipientStatistics
{
    public string RecipientType { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int DistributionCount { get; set; }
    public decimal Percentage { get; set; }
}

public class FeeTypeStatistics
{
    public string FeeType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int DistributionCount { get; set; }
    public decimal Percentage { get; set; }
}
