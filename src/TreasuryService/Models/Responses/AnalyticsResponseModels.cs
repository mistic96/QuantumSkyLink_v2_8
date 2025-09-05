namespace TreasuryService.Models.Responses;

public class AccountStatisticsResponse
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal AverageBalance { get; set; }
    public decimal HighestBalance { get; set; }
    public decimal LowestBalance { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal NetCashFlow { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime LastTransactionDate { get; set; }
    
    // Additional properties expected by service
    public int TotalTransactions { get; set; }
    public int TotalDeposits { get; set; }
    public int TotalWithdrawals { get; set; }
    public int TotalTransfers { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal LargestTransaction { get; set; }
    public decimal SmallestTransaction { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    public IEnumerable<MonthlyStatistics> MonthlyBreakdown { get; set; } = new List<MonthlyStatistics>();
}

public class MonthlyStatistics
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal Inflows { get; set; }
    public decimal Outflows { get; set; }
    public decimal NetFlow { get; set; }
    public int TransactionCount { get; set; }
}

public class TreasuryPerformanceResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal TotalReturnPercentage { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal Volatility { get; set; }
    public decimal SharpeRatio { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal BenchmarkReturn { get; set; }
    public decimal Alpha { get; set; }
    public decimal Beta { get; set; }
    public decimal TrackingError { get; set; }
    public decimal InformationRatio { get; set; }
    
    // Additional properties expected by service
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal AverageAccountBalance { get; set; }
    public decimal BalanceUtilizationRate { get; set; }
    public decimal TransactionSuccessRate { get; set; }
    public decimal AverageDailyVolume { get; set; }
    public decimal AccountGrowthRate { get; set; }
    public decimal LiquidityRatio { get; set; }
    
    public IEnumerable<PerformanceMetric> DailyReturns { get; set; } = new List<PerformanceMetric>();
    public IEnumerable<PerformanceMetric> MonthlyReturns { get; set; } = new List<PerformanceMetric>();
}

public class PerformanceMetric
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal Return { get; set; }
    public decimal ReturnPercentage { get; set; }
    public decimal? BenchmarkReturn { get; set; }
}

public class PortfolioAnalyticsResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal UnrealizedGainLossPercentage { get; set; }
    public decimal DayChange { get; set; }
    public decimal DayChangePercentage { get; set; }
    
    // Additional properties expected by service
    public decimal TotalPortfolioValue { get; set; }
    public int AccountCount { get; set; }
    public IEnumerable<CurrencyExposure> CurrencyDistribution { get; set; } = new List<CurrencyExposure>();
    public IEnumerable<AssetAllocation> AccountTypeDistribution { get; set; } = new List<AssetAllocation>();
    public decimal LargestAccount { get; set; }
    public decimal SmallestAccount { get; set; }
    public decimal AverageAccountSize { get; set; }
    
    public IEnumerable<AssetAllocation> AssetAllocations { get; set; } = new List<AssetAllocation>();
    public IEnumerable<CurrencyExposure> CurrencyExposures { get; set; } = new List<CurrencyExposure>();
    public RiskMetrics RiskMetrics { get; set; } = new();
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
}

public class AssetAllocation
{
    public string AssetType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
    public decimal Weight { get; set; }
    public int Count { get; set; }
    public int AccountCount { get; set; }
}

public class CurrencyExposure
{
    public string Currency { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal LocalValue { get; set; }
}

public class RiskMetrics
{
    public decimal ValueAtRisk { get; set; }
    public decimal ConditionalValueAtRisk { get; set; }
    public decimal Volatility { get; set; }
    public decimal Beta { get; set; }
    public decimal CorrelationToMarket { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal DownsideDeviation { get; set; }
}

public class PerformanceMetrics
{
    public decimal TotalReturn { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal SharpeRatio { get; set; }
    public decimal SortinoRatio { get; set; }
    public decimal CalmarRatio { get; set; }
    public decimal InformationRatio { get; set; }
    public decimal TrackingError { get; set; }
}

public class AssetAllocationResponse
{
    public Guid AccountId { get; set; }
    public DateTime AsOfDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    
    // Additional properties expected by service
    public string AssetType { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int AccountCount { get; set; }
    
    public IEnumerable<AssetAllocation> Allocations { get; set; } = new List<AssetAllocation>();
    public IEnumerable<AllocationTarget> Targets { get; set; } = new List<AllocationTarget>();
    public IEnumerable<AllocationDeviation> Deviations { get; set; } = new List<AllocationDeviation>();
}

public class AllocationTarget
{
    public string AssetType { get; set; } = string.Empty;
    public decimal TargetPercentage { get; set; }
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
}

public class AllocationDeviation
{
    public string AssetType { get; set; } = string.Empty;
    public decimal CurrentPercentage { get; set; }
    public decimal TargetPercentage { get; set; }
    public decimal Deviation { get; set; }
    public bool IsWithinRange { get; set; }
}

public class RiskAnalyticsResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal ValueAtRisk { get; set; }
    public decimal ConditionalValueAtRisk { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public int TimeHorizonDays { get; set; }
    public decimal Volatility { get; set; }
    public decimal Skewness { get; set; }
    public decimal Kurtosis { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal DownsideDeviation { get; set; }
    
    // Additional properties expected by service
    public decimal ConcentrationRisk { get; set; }
    public decimal LiquidityRisk { get; set; }
    public decimal CurrencyRisk { get; set; }
    public decimal OperationalRisk { get; set; }
    public decimal OverallRiskScore { get; set; }
    
    public IEnumerable<RiskScenario> StressTestResults { get; set; } = new List<RiskScenario>();
    public IEnumerable<CorrelationMatrix> Correlations { get; set; } = new List<CorrelationMatrix>();
}

public class RiskScenario
{
    public string ScenarioName { get; set; } = string.Empty;
    public decimal PotentialLoss { get; set; }
    public decimal PotentialLossPercentage { get; set; }
    public decimal Probability { get; set; }
}

public class CorrelationMatrix
{
    public string Asset1 { get; set; } = string.Empty;
    public string Asset2 { get; set; } = string.Empty;
    public decimal Correlation { get; set; }
}

public class ScenarioAnalysisResponse
{
    public Guid AccountId { get; set; }
    public string ScenarioType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IEnumerable<ScenarioResult> Results { get; set; } = new List<ScenarioResult>();
    public ScenarioComparison Comparison { get; set; } = new();
}

public class ScenarioResult
{
    public string ScenarioName { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public decimal ExpectedValue { get; set; }
    public decimal StandardDeviation { get; set; }
    public decimal ValueAtRisk { get; set; }
    public decimal MaximumLoss { get; set; }
    public decimal MaximumGain { get; set; }
    public IEnumerable<ScenarioOutcome> Outcomes { get; set; } = new List<ScenarioOutcome>();
}

public class ScenarioOutcome
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal Probability { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ScenarioComparison
{
    public string BaseScenario { get; set; } = string.Empty;
    public IEnumerable<ScenarioResult> AlternativeScenarios { get; set; } = new List<ScenarioResult>();
    public decimal BestCaseValue { get; set; }
    public decimal WorstCaseValue { get; set; }
    public decimal MostLikelyValue { get; set; }
    public decimal ExpectedValue { get; set; }
    public decimal StandardDeviation { get; set; }
}
