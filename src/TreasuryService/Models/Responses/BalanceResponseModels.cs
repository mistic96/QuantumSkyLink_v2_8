namespace TreasuryService.Models.Responses;

public class BalanceValidationResponse
{
    public Guid AccountId { get; set; }
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal ExpectedBalance { get; set; }
    public decimal ActualBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public DateTime ValidationDate { get; set; }
    public string ValidationMethod { get; set; } = string.Empty;
    public IEnumerable<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
    public IEnumerable<ValidationRule> RulesApplied { get; set; } = new List<ValidationRule>();
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ValidationIssue
{
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string? Reference { get; set; }
    public DateTime DetectedAt { get; set; }
}

public class ValidationRule
{
    public string RuleName { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? FailureReason { get; set; }
    public decimal? Threshold { get; set; }
}

public class BalanceAlertResponse
{
    public IEnumerable<BalanceAlert> Alerts { get; set; } = new List<BalanceAlert>();
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int CriticalCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public DateTime? LastAlertTime { get; set; }
}

public class BalanceAlert
{
    public Guid AlertId { get; set; }
    public Guid AccountId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal Threshold { get; set; }
    public decimal Variance { get; set; }
    public DateTime TriggeredAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedBy { get; set; }
    public string? AcknowledgmentNotes { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BalanceSnapshotResponse
{
    public IEnumerable<BalanceSnapshot> Snapshots { get; set; } = new List<BalanceSnapshot>();
    public int TotalCount { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public BalanceSnapshotSummary Summary { get; set; } = new();
}

public class BalanceSnapshot
{
    public Guid SnapshotId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal PendingCredits { get; set; }
    public decimal PendingDebits { get; set; }
    public decimal DayChange { get; set; }
    public decimal DayChangePercentage { get; set; }
    public bool IsReconciled { get; set; }
    public string SnapshotType { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class BalanceSnapshotSummary
{
    public decimal AverageBalance { get; set; }
    public decimal HighestBalance { get; set; }
    public decimal LowestBalance { get; set; }
    public decimal TotalChange { get; set; }
    public decimal TotalChangePercentage { get; set; }
    public decimal Volatility { get; set; }
    public int SnapshotCount { get; set; }
    public DateTime HighestBalanceDate { get; set; }
    public DateTime LowestBalanceDate { get; set; }
}

public class BalanceComparisonResponse
{
    public Guid AccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ComparisonType { get; set; } = string.Empty;
    public IEnumerable<BalanceComparison> Comparisons { get; set; } = new List<BalanceComparison>();
    public ComparisonSummary Summary { get; set; } = new();
    public IEnumerable<ComparisonTrend> Trends { get; set; } = new List<ComparisonTrend>();
}

public class BalanceComparison
{
    public DateTime Date { get; set; }
    public decimal CurrentPeriodBalance { get; set; }
    public decimal PriorPeriodBalance { get; set; }
    public decimal Difference { get; set; }
    public decimal DifferencePercentage { get; set; }
    public string Trend { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ComparisonSummary
{
    public decimal AverageDifference { get; set; }
    public decimal AverageDifferencePercentage { get; set; }
    public decimal MaximumDifference { get; set; }
    public decimal MinimumDifference { get; set; }
    public string OverallTrend { get; set; } = string.Empty;
    public int PeriodsCompared { get; set; }
    public int PositivePeriods { get; set; }
    public int NegativePeriods { get; set; }
}

public class ComparisonTrend
{
    public string TrendName { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = string.Empty;
    public decimal TrendStrength { get; set; }
    public decimal Slope { get; set; }
    public decimal RSquared { get; set; }
    public string Significance { get; set; } = string.Empty;
}

public class CurrencyBalanceResponse
{
    public IEnumerable<CurrencyBalance> Balances { get; set; } = new List<CurrencyBalance>();
    public int CurrencyCount { get; set; }
    public decimal TotalValueInBaseCurrency { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public DateTime AsOfDate { get; set; }
    public IEnumerable<CurrencyExposure> Exposures { get; set; } = new List<CurrencyExposure>();
    public CurrencyRiskMetrics RiskMetrics { get; set; } = new();
}

public class CurrencyBalance
{
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal ValueInBaseCurrency { get; set; }
    public decimal Percentage { get; set; }
    public decimal DayChange { get; set; }
    public decimal DayChangePercentage { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class CurrencyRiskMetrics
{
    public decimal CurrencyConcentrationRisk { get; set; }
    public decimal ExchangeRateVolatility { get; set; }
    public decimal ValueAtRisk { get; set; }
    public decimal CorrelationRisk { get; set; }
    public IEnumerable<CurrencyRiskFactor> RiskFactors { get; set; } = new List<CurrencyRiskFactor>();
}

public class CurrencyRiskFactor
{
    public string Currency { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public decimal Volatility { get; set; }
    public decimal Correlation { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
}

public class BalanceAnalyticsResponse
{
    public Guid AccountId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public BalanceStatistics Statistics { get; set; } = new();
    public IEnumerable<BalanceTrend> Trends { get; set; } = new List<BalanceTrend>();
    public IEnumerable<BalancePattern> Patterns { get; set; } = new List<BalancePattern>();
    public BalanceForecasting Forecasting { get; set; } = new();
    public IEnumerable<BalanceAnomaly> Anomalies { get; set; } = new List<BalanceAnomaly>();
}

public class BalanceStatistics
{
    public decimal CurrentBalance { get; set; }
    public decimal AverageBalance { get; set; }
    public decimal MedianBalance { get; set; }
    public decimal StandardDeviation { get; set; }
    public decimal Variance { get; set; }
    public decimal Skewness { get; set; }
    public decimal Kurtosis { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal MaximumBalance { get; set; }
    public DateTime MinimumBalanceDate { get; set; }
    public DateTime MaximumBalanceDate { get; set; }
    public decimal Range { get; set; }
    public decimal CoefficientOfVariation { get; set; }
}

public class BalanceTrend
{
    public string TrendType { get; set; } = string.Empty;
    public string TrendDirection { get; set; } = string.Empty;
    public decimal TrendStrength { get; set; }
    public decimal Slope { get; set; }
    public decimal RSquared { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal StartValue { get; set; }
    public decimal EndValue { get; set; }
    public decimal TotalChange { get; set; }
    public decimal ChangePercentage { get; set; }
}

public class BalancePattern
{
    public string PatternType { get; set; } = string.Empty;
    public string PatternName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal Amplitude { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
    public int OccurrenceCount { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class BalanceForecasting
{
    public IEnumerable<BalanceForecast> Forecasts { get; set; } = new List<BalanceForecast>();
    public string ForecastMethod { get; set; } = string.Empty;
    public decimal Accuracy { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public DateTime ForecastDate { get; set; }
    public int ForecastHorizonDays { get; set; }
}

public class BalanceForecast
{
    public DateTime Date { get; set; }
    public decimal ForecastBalance { get; set; }
    public decimal ConfidenceIntervalLower { get; set; }
    public decimal ConfidenceIntervalUpper { get; set; }
    public decimal Probability { get; set; }
    public string Scenario { get; set; } = string.Empty;
}

public class BalanceAnomaly
{
    public Guid AnomalyId { get; set; }
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
    public decimal ExpectedBalance { get; set; }
    public decimal Deviation { get; set; }
    public decimal DeviationPercentage { get; set; }
    public string AnomalyType { get; set; } = string.Empty;
    public decimal Severity { get; set; }
    public decimal Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PossibleCause { get; set; }
    public bool IsInvestigated { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BalanceTrendResponse
{
    public IEnumerable<BalanceTrend> Trends { get; set; } = new List<BalanceTrend>();
    public TrendSummary Summary { get; set; } = new();
    public IEnumerable<TrendIndicator> Indicators { get; set; } = new List<TrendIndicator>();
    public TrendAnalysis Analysis { get; set; } = new();
}

public class TrendSummary
{
    public string OverallTrend { get; set; } = string.Empty;
    public decimal OverallStrength { get; set; }
    public decimal TotalChange { get; set; }
    public decimal TotalChangePercentage { get; set; }
    public int TrendCount { get; set; }
    public int PositiveTrends { get; set; }
    public int NegativeTrends { get; set; }
    public int NeutralTrends { get; set; }
}

public class TrendAnalysis
{
    public decimal Momentum { get; set; }
    public decimal Acceleration { get; set; }
    public string TrendQuality { get; set; } = string.Empty;
    public decimal Consistency { get; set; }
    public IEnumerable<TrendBreakpoint> Breakpoints { get; set; } = new List<TrendBreakpoint>();
    public IEnumerable<TrendCycle> Cycles { get; set; } = new List<TrendCycle>();
}

public class TrendBreakpoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string BreakpointType { get; set; } = string.Empty;
    public decimal Significance { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TrendCycle
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DurationDays { get; set; }
    public decimal Amplitude { get; set; }
    public string CycleType { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}

public class BalanceProjectionResponse
{
    public Guid AccountId { get; set; }
    public DateTime ProjectionDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public IEnumerable<BalanceProjection> Projections { get; set; } = new List<BalanceProjection>();
    public IEnumerable<ProjectionScenario> Scenarios { get; set; } = new List<ProjectionScenario>();
    public ProjectionMetrics Metrics { get; set; } = new();
    public ProjectionAssumptions Assumptions { get; set; } = new();
}

public class BalanceProjection
{
    public DateTime Date { get; set; }
    public decimal ProjectedBalance { get; set; }
    public decimal ProjectedInflows { get; set; }
    public decimal ProjectedOutflows { get; set; }
    public decimal NetFlow { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public decimal VarianceRange { get; set; }
    public string ProjectionMethod { get; set; } = string.Empty;
}

public class ProjectionScenario
{
    public string ScenarioName { get; set; } = string.Empty;
    public decimal Probability { get; set; }
    public IEnumerable<BalanceProjection> Projections { get; set; } = new List<BalanceProjection>();
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ProjectionMetrics
{
    public decimal Accuracy { get; set; }
    public decimal MeanAbsoluteError { get; set; }
    public decimal MeanSquaredError { get; set; }
    public decimal RootMeanSquaredError { get; set; }
    public decimal MeanAbsolutePercentageError { get; set; }
    public decimal RSquared { get; set; }
    public string ModelQuality { get; set; } = string.Empty;
}

public class ProjectionAssumptions
{
    public decimal InflowGrowthRate { get; set; }
    public decimal OutflowGrowthRate { get; set; }
    public decimal SeasonalityFactor { get; set; }
    public decimal VolatilityAdjustment { get; set; }
    public IEnumerable<string> ExternalFactors { get; set; } = new List<string>();
    public Dictionary<string, object> ModelParameters { get; set; } = new();
}

public class BalanceSyncResponse
{
    public Guid SyncId { get; set; }
    public Guid AccountId { get; set; }
    public DateTime SyncDate { get; set; }
    public string SyncType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public IEnumerable<SyncResult> Results { get; set; } = new List<SyncResult>();
    public SyncSummary Summary { get; set; } = new();
    public IEnumerable<SyncError> Errors { get; set; } = new List<SyncError>();
    public TimeSpan Duration { get; set; }
}

public class SyncResult
{
    public string DataType { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsDeleted { get; set; }
    public int RecordsSkipped { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SyncSummary
{
    public int TotalRecordsProcessed { get; set; }
    public int TotalRecordsUpdated { get; set; }
    public int TotalRecordsInserted { get; set; }
    public int TotalRecordsDeleted { get; set; }
    public int TotalRecordsSkipped { get; set; }
    public int TotalErrors { get; set; }
    public decimal SuccessRate { get; set; }
    public bool OverallSuccess { get; set; }
}

public class SyncError
{
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string? FieldName { get; set; }
    public string? FieldValue { get; set; }
    public DateTime ErrorTime { get; set; }
    public string Severity { get; set; } = string.Empty;
}
