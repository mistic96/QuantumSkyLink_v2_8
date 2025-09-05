namespace TreasuryService.Models.Responses;

public class FinancialReportResponse
{
    public Guid ReportId { get; set; }
    public Guid AccountId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Format { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public IEnumerable<ReportSection> Sections { get; set; } = new List<ReportSection>();
    public Dictionary<string, object> Summary { get; set; } = new();
}

public class ReportSection
{
    public string SectionName { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public IEnumerable<ChartData> Charts { get; set; } = new List<ChartData>();
}

public class ChartData
{
    public string ChartType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IEnumerable<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
}

public class DataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime? Date { get; set; }
    public string? Category { get; set; }
}

public class FinancialStatementResponse
{
    public Guid AccountId { get; set; }
    public string StatementType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public IEnumerable<StatementLine> Lines { get; set; } = new List<StatementLine>();
    public IEnumerable<StatementNote> Notes { get; set; } = new List<StatementNote>();
    public Dictionary<string, decimal> Totals { get; set; } = new();
}

public class StatementLine
{
    public string LineItem { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CurrentPeriod { get; set; }
    public decimal? PriorPeriod { get; set; }
    public decimal? Change { get; set; }
    public decimal? ChangePercentage { get; set; }
    public int SortOrder { get; set; }
}

public class StatementNote
{
    public int NoteNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string[]? References { get; set; }
}

public class ProfitLossResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal RealizedGainLoss { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal InterestIncome { get; set; }
    public decimal DividendIncome { get; set; }
    public decimal FeeExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public IEnumerable<ProfitLossItem> Items { get; set; } = new List<ProfitLossItem>();
    public IEnumerable<ProfitLossBreakdown> Breakdown { get; set; } = new List<ProfitLossBreakdown>();
}

public class ProfitLossItem
{
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Reference { get; set; }
}

public class ProfitLossBreakdown
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int Count { get; set; }
}

public class LiquidityAnalyticsResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public decimal TotalLiquidity { get; set; }
    public decimal ImmediateLiquidity { get; set; }
    public decimal ShortTermLiquidity { get; set; }
    public decimal LongTermLiquidity { get; set; }
    public decimal LiquidityRatio { get; set; }
    public decimal CashRatio { get; set; }
    public decimal QuickRatio { get; set; }
    public IEnumerable<LiquidityTier> LiquidityTiers { get; set; } = new List<LiquidityTier>();
    public IEnumerable<LiquidityStressTest> StressTests { get; set; } = new List<LiquidityStressTest>();
}

public class LiquidityTier
{
    public string TierName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public int DaysToLiquidate { get; set; }
    public decimal LiquidationCost { get; set; }
}

public class LiquidityStressTest
{
    public string ScenarioName { get; set; } = string.Empty;
    public decimal RequiredLiquidity { get; set; }
    public decimal AvailableLiquidity { get; set; }
    public decimal Shortfall { get; set; }
    public bool PassesTest { get; set; }
}

public class LiquidityForecastResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ScenarioType { get; set; } = string.Empty;
    public decimal ConfidenceLevel { get; set; }
    public IEnumerable<LiquidityProjection> Projections { get; set; } = new List<LiquidityProjection>();
    public IEnumerable<LiquidityAlert> Alerts { get; set; } = new List<LiquidityAlert>();
}

public class LiquidityProjection
{
    public DateTime Date { get; set; }
    public decimal ProjectedBalance { get; set; }
    public decimal ProjectedInflows { get; set; }
    public decimal ProjectedOutflows { get; set; }
    public decimal NetFlow { get; set; }
    public decimal LiquidityRatio { get; set; }
}

public class LiquidityAlert
{
    public DateTime Date { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public decimal ProjectedValue { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class LiquidityRatioResponse
{
    public Guid AccountId { get; set; }
    public DateTime AsOfDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal CurrentRatio { get; set; }
    public decimal QuickRatio { get; set; }
    public decimal CashRatio { get; set; }
    public decimal OperatingCashFlowRatio { get; set; }
    public decimal LiquidityIndex { get; set; }
    public IEnumerable<RatioTrend> Trends { get; set; } = new List<RatioTrend>();
    public IEnumerable<RatioBenchmark> Benchmarks { get; set; } = new List<RatioBenchmark>();
}

public class RatioTrend
{
    public DateTime Date { get; set; }
    public string RatioName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Change { get; set; }
}

public class RatioBenchmark
{
    public string RatioName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal BenchmarkValue { get; set; }
    public decimal Deviation { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TreasuryMetricsResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IEnumerable<MetricValue> Metrics { get; set; } = new List<MetricValue>();
    public IEnumerable<MetricTrend> Trends { get; set; } = new List<MetricTrend>();
    public Dictionary<string, object> Summary { get; set; } = new();
}

public class MetricValue
{
    public string MetricName { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? Change { get; set; }
}

public class MetricTrend
{
    public string MetricName { get; set; } = string.Empty;
    public IEnumerable<TrendPoint> DataPoints { get; set; } = new List<TrendPoint>();
    public string TrendDirection { get; set; } = string.Empty;
    public decimal TrendSlope { get; set; }
}

public class KpiResponse
{
    public string KpiName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal TargetValue { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public IEnumerable<KpiHistory> History { get; set; } = new List<KpiHistory>();
}

public class KpiHistory
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal Target { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BenchmarkComparisonResponse
{
    public Guid AccountId { get; set; }
    public string BenchmarkType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public IEnumerable<ComparisonMetric> Comparisons { get; set; } = new List<ComparisonMetric>();
    public decimal OverallScore { get; set; }
    public string PerformanceRating { get; set; } = string.Empty;
}

public class ComparisonMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal PortfolioValue { get; set; }
    public decimal BenchmarkValue { get; set; }
    public decimal Difference { get; set; }
    public decimal DifferencePercentage { get; set; }
    public string Performance { get; set; } = string.Empty;
}

public class TrendAnalysisResponse
{
    public Guid AccountId { get; set; }
    public string TrendType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public decimal TrendStrength { get; set; }
    public decimal Slope { get; set; }
    public decimal RSquared { get; set; }
    public IEnumerable<TrendPoint> DataPoints { get; set; } = new List<TrendPoint>();
    public IEnumerable<TrendIndicator> Indicators { get; set; } = new List<TrendIndicator>();
}

public class TrendIndicator
{
    public string IndicatorName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Signal { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class SeasonalAnalysisResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string SeasonalityType { get; set; } = string.Empty;
    public IEnumerable<SeasonalPattern> Patterns { get; set; } = new List<SeasonalPattern>();
    public IEnumerable<SeasonalForecast> Forecasts { get; set; } = new List<SeasonalForecast>();
}

public class SeasonalPattern
{
    public string Period { get; set; } = string.Empty;
    public decimal AverageValue { get; set; }
    public decimal Volatility { get; set; }
    public decimal SeasonalIndex { get; set; }
    public string Trend { get; set; } = string.Empty;
}

public class SeasonalForecast
{
    public DateTime Date { get; set; }
    public decimal ForecastValue { get; set; }
    public decimal ConfidenceInterval { get; set; }
    public decimal SeasonalAdjustment { get; set; }
}

public class VolatilityAnalysisResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string VolatilityType { get; set; } = string.Empty;
    public decimal CurrentVolatility { get; set; }
    public decimal AverageVolatility { get; set; }
    public decimal VolatilityRank { get; set; }
    public IEnumerable<VolatilityMeasure> Measures { get; set; } = new List<VolatilityMeasure>();
    public IEnumerable<VolatilityRegime> Regimes { get; set; } = new List<VolatilityRegime>();
}

public class VolatilityMeasure
{
    public DateTime Date { get; set; }
    public decimal Volatility { get; set; }
    public decimal RollingVolatility { get; set; }
    public string Regime { get; set; } = string.Empty;
}

public class VolatilityRegime
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string RegimeType { get; set; } = string.Empty;
    public decimal AverageVolatility { get; set; }
    public int DurationDays { get; set; }
}

public class ForecastResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ForecastType { get; set; } = string.Empty;
    public IEnumerable<ForecastPoint> Forecasts { get; set; } = new List<ForecastPoint>();
    public IEnumerable<ForecastScenario> Scenarios { get; set; } = new List<ForecastScenario>();
    public ForecastAccuracy Accuracy { get; set; } = new();
}

public class ForecastPoint
{
    public DateTime Date { get; set; }
    public decimal ForecastValue { get; set; }
    public decimal ConfidenceIntervalLower { get; set; }
    public decimal ConfidenceIntervalUpper { get; set; }
    public decimal? ActualValue { get; set; }
}

public class ForecastScenario
{
    public string ScenarioName { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public IEnumerable<ForecastPoint> Points { get; set; } = new List<ForecastPoint>();
}

public class ForecastAccuracy
{
    public decimal MeanAbsoluteError { get; set; }
    public decimal MeanSquaredError { get; set; }
    public decimal MeanAbsolutePercentageError { get; set; }
    public decimal RSquared { get; set; }
}

public class CashFlowForecastResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public IEnumerable<CashFlowProjection> Projections { get; set; } = new List<CashFlowProjection>();
    public IEnumerable<CashFlowScenario> Scenarios { get; set; } = new List<CashFlowScenario>();
    public CashFlowSummary Summary { get; set; } = new();
}

public class CashFlowProjection
{
    public DateTime Date { get; set; }
    public decimal ProjectedInflows { get; set; }
    public decimal ProjectedOutflows { get; set; }
    public decimal NetCashFlow { get; set; }
    public decimal CumulativeCashFlow { get; set; }
    public decimal ProjectedBalance { get; set; }
}

public class CashFlowScenario
{
    public string ScenarioName { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public IEnumerable<CashFlowProjection> Projections { get; set; } = new List<CashFlowProjection>();
}

public class CashFlowSummary
{
    public decimal TotalProjectedInflows { get; set; }
    public decimal TotalProjectedOutflows { get; set; }
    public decimal NetProjectedCashFlow { get; set; }
    public decimal AverageDailyCashFlow { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal MaximumBalance { get; set; }
    public DateTime MinimumBalanceDate { get; set; }
    public DateTime MaximumBalanceDate { get; set; }
}

public class ComplianceAnalyticsResponse
{
    public Guid AccountId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ComplianceType { get; set; } = string.Empty;
    public decimal ComplianceScore { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty;
    public IEnumerable<ComplianceRule> Rules { get; set; } = new List<ComplianceRule>();
    public IEnumerable<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
}

public class ComplianceRule
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public decimal CurrentValue { get; set; }
}

public class ComplianceViolation
{
    public string ViolationId { get; set; } = string.Empty;
    public string RuleId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ComplianceViolationResponse
{
    public Guid AccountId { get; set; }
    public IEnumerable<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
    public int TotalViolations { get; set; }
    public int CriticalViolations { get; set; }
    public int HighViolations { get; set; }
    public int MediumViolations { get; set; }
    public int LowViolations { get; set; }
}

public class RegulatoryReportResponse
{
    public Guid ReportId { get; set; }
    public Guid AccountId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string RegulatoryFramework { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public IEnumerable<ReportSection> Sections { get; set; } = new List<ReportSection>();
}

public class CustomAnalyticsResponse
{
    public Guid AnalyticsId { get; set; }
    public Guid AccountId { get; set; }
    public string AnalyticsType { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class AnalyticsTemplateResponse
{
    public Guid TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AnalyticsType { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public bool IsPublic { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}

public class ReportScheduleResponse
{
    public Guid ScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string Schedule { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Format { get; set; } = string.Empty;
    public string[] Recipients { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
}

public class ScheduledReportResponse
{
    public Guid ReportId { get; set; }
    public Guid ScheduleId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string[] Recipients { get; set; } = Array.Empty<string>();
}

public class RealTimeAnalyticsResponse
{
    public Guid AccountId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, decimal> Metrics { get; set; } = new();
    public IEnumerable<RealTimeAlert> Alerts { get; set; } = new List<RealTimeAlert>();
    public string Status { get; set; } = string.Empty;
}

public class RealTimeAlert
{
    public string AlertId { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class AlertResponse
{
    public Guid AlertId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string MetricType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public decimal Threshold { get; set; }
    public string Description { get; set; } = string.Empty;
    public string[] Recipients { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggered { get; set; }
}
