using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Services.Interfaces;

public interface ITreasuryAnalyticsService
{
    // Treasury Analytics
    Task<TreasuryAnalyticsResponse> GetTreasuryAnalyticsAsync(GetTreasuryAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryCashFlowResponse> GetCashFlowAnalysisAsync(GetCashFlowAnalysisRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryPerformanceResponse> GetPerformanceAnalyticsAsync(GetPerformanceAnalyticsRequest request, CancellationToken cancellationToken = default);
    
    // Portfolio Analytics
    Task<PortfolioAnalyticsResponse> GetPortfolioAnalyticsAsync(GetPortfolioAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<AssetAllocationResponse>> GetAssetAllocationAsync(CancellationToken cancellationToken = default);
    Task<RiskAnalyticsResponse> GetRiskAnalyticsAsync(GetRiskAnalyticsRequest request, CancellationToken cancellationToken = default);
    
    // Financial Reporting
    Task<FinancialReportResponse> GenerateFinancialReportAsync(GenerateFinancialReportRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<FinancialStatementResponse>> GetFinancialStatementsAsync(GetFinancialStatementsRequest request, CancellationToken cancellationToken = default);
    Task<ProfitLossResponse> GetProfitLossAnalysisAsync(GetProfitLossRequest request, CancellationToken cancellationToken = default);
    
    // Liquidity Analytics
    Task<LiquidityAnalyticsResponse> GetLiquidityAnalyticsAsync(GetLiquidityAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<LiquidityForecastResponse>> GetLiquidityForecastAsync(GetLiquidityForecastRequest request, CancellationToken cancellationToken = default);
    Task<LiquidityRatioResponse> GetLiquidityRatiosAsync(CancellationToken cancellationToken = default);
    
    // Treasury Metrics
    Task<TreasuryMetricsResponse> GetTreasuryMetricsAsync(GetTreasuryMetricsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<KpiResponse>> GetKeyPerformanceIndicatorsAsync(CancellationToken cancellationToken = default);
    Task<BenchmarkComparisonResponse> GetBenchmarkComparisonAsync(GetBenchmarkComparisonRequest request, CancellationToken cancellationToken = default);
    
    // Trend Analysis
    Task<IEnumerable<TrendAnalysisResponse>> GetTrendAnalysisAsync(GetTrendAnalysisRequest request, CancellationToken cancellationToken = default);
    Task<SeasonalAnalysisResponse> GetSeasonalAnalysisAsync(GetSeasonalAnalysisRequest request, CancellationToken cancellationToken = default);
    Task<VolatilityAnalysisResponse> GetVolatilityAnalysisAsync(GetVolatilityAnalysisRequest request, CancellationToken cancellationToken = default);
    
    // Forecasting
    Task<IEnumerable<ForecastResponse>> GetTreasuryForecastAsync(GetTreasuryForecastRequest request, CancellationToken cancellationToken = default);
    Task<CashFlowForecastResponse> GetCashFlowForecastAsync(GetCashFlowForecastRequest request, CancellationToken cancellationToken = default);
    Task<ScenarioAnalysisResponse> GetScenarioAnalysisAsync(GetScenarioAnalysisRequest request, CancellationToken cancellationToken = default);
    
    // Compliance Analytics
    Task<ComplianceAnalyticsResponse> GetComplianceAnalyticsAsync(GetComplianceAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplianceViolationResponse>> GetComplianceViolationsAsync(CancellationToken cancellationToken = default);
    Task<RegulatoryReportResponse> GenerateRegulatoryReportAsync(GenerateRegulatoryReportRequest request, CancellationToken cancellationToken = default);
    
    // Custom Analytics
    Task<CustomAnalyticsResponse> RunCustomAnalyticsAsync(RunCustomAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<AnalyticsTemplateResponse>> GetAnalyticsTemplatesAsync(CancellationToken cancellationToken = default);
    Task<AnalyticsTemplateResponse> CreateAnalyticsTemplateAsync(CreateAnalyticsTemplateRequest request, CancellationToken cancellationToken = default);
    
    // Data Export
    Task<byte[]> ExportAnalyticsDataAsync(ExportAnalyticsDataRequest request, CancellationToken cancellationToken = default);
    Task<ReportScheduleResponse> ScheduleReportAsync(ScheduleReportRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ScheduledReportResponse>> GetScheduledReportsAsync(CancellationToken cancellationToken = default);
    
    // Real-time Analytics
    Task<RealTimeAnalyticsResponse> GetRealTimeAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AlertResponse>> GetAnalyticsAlertsAsync(CancellationToken cancellationToken = default);
    Task<bool> CreateAnalyticsAlertAsync(CreateAnalyticsAlertRequest request, CancellationToken cancellationToken = default);
}
