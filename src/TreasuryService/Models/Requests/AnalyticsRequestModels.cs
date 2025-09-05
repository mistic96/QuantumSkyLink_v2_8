using System.ComponentModel.DataAnnotations;

namespace TreasuryService.Models.Requests;

public class GetPerformanceAnalyticsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public string? Currency { get; set; }
    public string? BenchmarkType { get; set; }
    public bool IncludeProjections { get; set; } = false;
}

public class GetPortfolioAnalyticsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime AsOfDate { get; set; }
    
    public string? Currency { get; set; }
    public string? AnalysisType { get; set; }
    public bool IncludeRiskMetrics { get; set; } = true;
    public bool IncludePerformanceMetrics { get; set; } = true;
}

public class GetRiskAnalyticsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? RiskType { get; set; }
    public decimal? ConfidenceLevel { get; set; }
    public int? TimeHorizonDays { get; set; }
}

public class GenerateFinancialReportRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string ReportType { get; set; } = string.Empty;
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? Format { get; set; } = "PDF";
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeComparisons { get; set; } = false;
}

public class GetFinancialStatementsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    [Required]
    public string StatementType { get; set; } = string.Empty;
    
    public string? Currency { get; set; }
    public bool IncludeNotes { get; set; } = true;
    public bool IncludeComparativePeriod { get; set; } = false;
}

public class GetProfitLossRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? GroupBy { get; set; }
    public bool IncludeUnrealizedGains { get; set; } = true;
}

public class GetLiquidityAnalyticsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime AsOfDate { get; set; }
    
    public string? Currency { get; set; }
    public int? ForecastDays { get; set; }
    public bool IncludeStressTest { get; set; } = false;
}

public class GetLiquidityForecastRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? ScenarioType { get; set; }
    public decimal? ConfidenceLevel { get; set; }
}

public class GetTreasuryMetricsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string[]? MetricTypes { get; set; }
    public string? Frequency { get; set; } = "Daily";
}

public class GetBenchmarkComparisonRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string BenchmarkType { get; set; } = string.Empty;
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string[]? MetricsToCompare { get; set; }
}

public class GetTrendAnalysisRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string TrendType { get; set; } = string.Empty;
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? Frequency { get; set; } = "Daily";
    public int? MovingAveragePeriods { get; set; }
}

public class GetSeasonalAnalysisRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? SeasonalityType { get; set; }
    public int? YearsOfHistory { get; set; }
}

public class GetVolatilityAnalysisRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? VolatilityType { get; set; }
    public int? WindowSize { get; set; }
}

public class GetTreasuryForecastRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? ForecastType { get; set; }
    public string[]? Scenarios { get; set; }
}

public class GetCashFlowForecastRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? Frequency { get; set; } = "Daily";
    public bool IncludeStressScenarios { get; set; } = false;
}

public class GetScenarioAnalysisRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string ScenarioType { get; set; } = string.Empty;
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public Dictionary<string, object>? ScenarioParameters { get; set; }
}

public class GetComplianceAnalyticsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? ComplianceType { get; set; }
    public string[]? RegulatoryFrameworks { get; set; }
    public bool IncludeViolations { get; set; } = true;
}

public class GenerateRegulatoryReportRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string ReportType { get; set; } = string.Empty;
    
    [Required]
    public string RegulatoryFramework { get; set; } = string.Empty;
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    public string? Currency { get; set; }
    public string? Format { get; set; } = "PDF";
    public bool IncludeAttachments { get; set; } = false;
}

public class RunCustomAnalyticsRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string AnalyticsType { get; set; } = string.Empty;
    
    [Required]
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Currency { get; set; }
}

public class CreateAnalyticsTemplateRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public string AnalyticsType { get; set; } = string.Empty;
    
    [Required]
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    public bool IsPublic { get; set; } = false;
    public string[]? Tags { get; set; }
}

public class ExportAnalyticsDataRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string DataType { get; set; } = string.Empty;
    
    [Required]
    public DateTime FromDate { get; set; }
    
    [Required]
    public DateTime ToDate { get; set; }
    
    [Required]
    public string Format { get; set; } = "CSV";
    
    public string? Currency { get; set; }
    public string[]? Columns { get; set; }
    public bool IncludeHeaders { get; set; } = true;
}

public class ScheduleReportRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string ReportType { get; set; } = string.Empty;
    
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string Schedule { get; set; } = string.Empty; // Cron expression
    
    [Required]
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    public string? Format { get; set; } = "PDF";
    public string[]? Recipients { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateAnalyticsAlertRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    public string MetricType { get; set; } = string.Empty;
    
    [Required]
    public string Condition { get; set; } = string.Empty;
    
    [Required]
    public decimal Threshold { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public string[]? Recipients { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Currency { get; set; }
}
