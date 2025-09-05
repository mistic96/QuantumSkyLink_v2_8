using Microsoft.EntityFrameworkCore;
using Mapster;
using TreasuryService.Data;
using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;
using TreasuryService.Services.Interfaces;

namespace TreasuryService.Services;

public class TreasuryAnalyticsService : ITreasuryAnalyticsService
{
    private readonly TreasuryDbContext _context;
    private readonly ILogger<TreasuryAnalyticsService> _logger;

    public TreasuryAnalyticsService(
        TreasuryDbContext context,
        ILogger<TreasuryAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Treasury Analytics

    public async Task<TreasuryAnalyticsResponse> GetTreasuryAnalyticsAsync(GetTreasuryAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var accounts = await _context.TreasuryAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync(cancellationToken);

        var transactions = await _context.TreasuryTransactions
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var analytics = new TreasuryAnalyticsResponse
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalAccounts = accounts.Count,
            ActiveAccounts = accounts.Count(a => a.Status == "Active"),
            TotalBalance = accounts.Sum(a => a.Balance),
            TotalAvailableBalance = accounts.Sum(a => a.AvailableBalance),
            TotalReservedBalance = accounts.Sum(a => a.ReservedBalance),
            TotalTransactions = transactions.Count,
            TotalTransactionVolume = transactions.Sum(t => t.Amount),
            AverageTransactionAmount = transactions.Any() ? transactions.Average(t => t.Amount) : 0,
            LargestTransaction = transactions.Any() ? transactions.Max(t => t.Amount) : 0,
            TransactionsByType = transactions.GroupBy(t => t.TransactionType)
                .ToDictionary(g => g.Key, g => g.Count()),
            TransactionsByStatus = transactions.GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return analytics;
    }

    public async Task<TreasuryCashFlowResponse> GetCashFlowAnalysisAsync(GetCashFlowAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var transactions = await _context.TreasuryTransactions
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate && t.Status == "Completed")
            .ToListAsync(cancellationToken);

        var inflows = transactions
            .Where(t => t.TransactionType == "Deposit" || t.TransactionType == "Transfer_In")
            .Sum(t => t.Amount);

        var outflows = transactions
            .Where(t => t.TransactionType == "Withdrawal" || t.TransactionType == "Transfer_Out")
            .Sum(t => t.Amount);

        var netCashFlow = inflows - outflows;

        return new TreasuryCashFlowResponse
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalInflows = inflows,
            TotalOutflows = outflows,
            NetCashFlow = netCashFlow,
            InflowsByType = transactions
                .Where(t => t.TransactionType == "Deposit" || t.TransactionType == "Transfer_In")
                .GroupBy(t => t.TransactionType)
                .Select(g => new CashFlowByType { TransactionType = g.Key, Amount = g.Sum(t => t.Amount) }),
            OutflowsByType = transactions
                .Where(t => t.TransactionType == "Withdrawal" || t.TransactionType == "Transfer_Out")
                .GroupBy(t => t.TransactionType)
                .Select(g => new CashFlowByType { TransactionType = g.Key, Amount = g.Sum(t => t.Amount) })
        };
    }

    public async Task<TreasuryPerformanceResponse> GetPerformanceAnalyticsAsync(GetPerformanceAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var accounts = await _context.TreasuryAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync(cancellationToken);

        var transactions = await _context.TreasuryTransactions
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var totalDays = (endDate - startDate).Days;
        var dailyTransactionVolume = totalDays > 0 ? transactions.Sum(t => t.Amount) / totalDays : 0;

        return new TreasuryPerformanceResponse
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            AverageAccountBalance = accounts.Any() ? accounts.Average(a => a.Balance) : 0,
            BalanceUtilizationRate = CalculateBalanceUtilizationRate(accounts),
            TransactionSuccessRate = CalculateTransactionSuccessRate(transactions),
            AverageDailyVolume = dailyTransactionVolume,
            AccountGrowthRate = CalculateAccountGrowthRate(startDate, endDate),
            LiquidityRatio = CalculateLiquidityRatio(accounts)
        };
    }

    #endregion

    #region Portfolio Analytics

    public async Task<PortfolioAnalyticsResponse> GetPortfolioAnalyticsAsync(GetPortfolioAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var accounts = await _context.TreasuryAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync(cancellationToken);

        var totalBalance = accounts.Sum(a => a.Balance);

        var portfolioAnalytics = new PortfolioAnalyticsResponse
        {
            TotalPortfolioValue = totalBalance,
            AccountCount = accounts.Count,
            CurrencyDistribution = accounts
                .GroupBy(a => a.Currency)
                .Select(g => new CurrencyExposure { Currency = g.Key, Value = g.Sum(a => a.Balance) }),
            AccountTypeDistribution = accounts
                .GroupBy(a => a.AccountType)
                .Select(g => new AssetAllocation { AssetType = g.Key, Value = g.Sum(a => a.Balance) }),
            LargestAccount = accounts.OrderByDescending(a => a.Balance).FirstOrDefault()?.Balance ?? 0,
            SmallestAccount = accounts.OrderBy(a => a.Balance).FirstOrDefault()?.Balance ?? 0,
            AverageAccountSize = accounts.Any() ? accounts.Average(a => a.Balance) : 0
        };

        return portfolioAnalytics;
    }

    public async Task<IEnumerable<AssetAllocationResponse>> GetAssetAllocationAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _context.TreasuryAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync(cancellationToken);

        var totalBalance = accounts.Sum(a => a.Balance);

        return accounts
            .GroupBy(a => a.Currency)
            .Select(g => new AssetAllocationResponse
            {
                AssetType = g.Key,
                TotalValue = g.Sum(a => a.Balance),
                Percentage = totalBalance > 0 ? (g.Sum(a => a.Balance) / totalBalance) * 100 : 0,
                AccountCount = g.Count()
            })
            .OrderByDescending(a => a.TotalValue);
    }

    public async Task<RiskAnalyticsResponse> GetRiskAnalyticsAsync(GetRiskAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        var accounts = await _context.TreasuryAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync(cancellationToken);

        var riskMetrics = new RiskAnalyticsResponse
        {
            ConcentrationRisk = CalculateConcentrationRisk(accounts),
            LiquidityRisk = CalculateLiquidityRisk(accounts),
            CurrencyRisk = CalculateCurrencyRisk(accounts),
            OperationalRisk = CalculateOperationalRisk(accounts),
            OverallRiskScore = 0 // Will be calculated based on other metrics
        };

        // Calculate overall risk score as weighted average
        riskMetrics.OverallRiskScore = (riskMetrics.ConcentrationRisk * 0.3m) +
                                      (riskMetrics.LiquidityRisk * 0.3m) +
                                      (riskMetrics.CurrencyRisk * 0.2m) +
                                      (riskMetrics.OperationalRisk * 0.2m);

        return riskMetrics;
    }

    #endregion

    #region Not Yet Implemented - Placeholder methods

    public Task<FinancialReportResponse> GenerateFinancialReportAsync(GenerateFinancialReportRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<FinancialStatementResponse>> GetFinancialStatementsAsync(GetFinancialStatementsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<ProfitLossResponse> GetProfitLossAnalysisAsync(GetProfitLossRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<LiquidityAnalyticsResponse> GetLiquidityAnalyticsAsync(GetLiquidityAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<LiquidityForecastResponse>> GetLiquidityForecastAsync(GetLiquidityForecastRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<LiquidityRatioResponse> GetLiquidityRatiosAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<TreasuryMetricsResponse> GetTreasuryMetricsAsync(GetTreasuryMetricsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<KpiResponse>> GetKeyPerformanceIndicatorsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<BenchmarkComparisonResponse> GetBenchmarkComparisonAsync(GetBenchmarkComparisonRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<TrendAnalysisResponse>> GetTrendAnalysisAsync(GetTrendAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<SeasonalAnalysisResponse> GetSeasonalAnalysisAsync(GetSeasonalAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<VolatilityAnalysisResponse> GetVolatilityAnalysisAsync(GetVolatilityAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<ForecastResponse>> GetTreasuryForecastAsync(GetTreasuryForecastRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<CashFlowForecastResponse> GetCashFlowForecastAsync(GetCashFlowForecastRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<ScenarioAnalysisResponse> GetScenarioAnalysisAsync(GetScenarioAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<ComplianceAnalyticsResponse> GetComplianceAnalyticsAsync(GetComplianceAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<ComplianceViolationResponse>> GetComplianceViolationsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<RegulatoryReportResponse> GenerateRegulatoryReportAsync(GenerateRegulatoryReportRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<CustomAnalyticsResponse> RunCustomAnalyticsAsync(RunCustomAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<AnalyticsTemplateResponse>> GetAnalyticsTemplatesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<AnalyticsTemplateResponse> CreateAnalyticsTemplateAsync(CreateAnalyticsTemplateRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<byte[]> ExportAnalyticsDataAsync(ExportAnalyticsDataRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<ReportScheduleResponse> ScheduleReportAsync(ScheduleReportRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<ScheduledReportResponse>> GetScheduledReportsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<RealTimeAnalyticsResponse> GetRealTimeAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<AlertResponse>> GetAnalyticsAlertsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<bool> CreateAnalyticsAlertAsync(CreateAnalyticsAlertRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    #endregion

    #region Helper Methods

    private decimal CalculateBalanceUtilizationRate(List<TreasuryAccount> accounts)
    {
        if (!accounts.Any()) return 0;

        var totalBalance = accounts.Sum(a => a.Balance);
        var totalMaxBalance = accounts.Sum(a => a.MaximumBalance);

        return totalMaxBalance > 0 ? (totalBalance / totalMaxBalance) * 100 : 0;
    }

    private decimal CalculateTransactionSuccessRate(List<TreasuryTransaction> transactions)
    {
        if (!transactions.Any()) return 100;

        var successfulTransactions = transactions.Count(t => t.Status == "Completed");
        return (decimal)successfulTransactions / transactions.Count * 100;
    }

    private decimal CalculateAccountGrowthRate(DateTime startDate, DateTime endDate)
    {
        // Mock implementation - in real system, compare account counts over time
        return 5.0m; // 5% growth rate
    }

    private decimal CalculateLiquidityRatio(List<TreasuryAccount> accounts)
    {
        if (!accounts.Any()) return 0;

        var totalBalance = accounts.Sum(a => a.Balance);
        var totalAvailableBalance = accounts.Sum(a => a.AvailableBalance);

        return totalBalance > 0 ? (totalAvailableBalance / totalBalance) * 100 : 0;
    }

    private decimal CalculateConcentrationRisk(List<TreasuryAccount> accounts)
    {
        if (!accounts.Any()) return 0;

        var totalBalance = accounts.Sum(a => a.Balance);
        var largestAccountBalance = accounts.Max(a => a.Balance);

        return totalBalance > 0 ? (largestAccountBalance / totalBalance) * 100 : 0;
    }

    private decimal CalculateLiquidityRisk(List<TreasuryAccount> accounts)
    {
        if (!accounts.Any()) return 0;

        var totalBalance = accounts.Sum(a => a.Balance);
        var totalReservedBalance = accounts.Sum(a => a.ReservedBalance);

        return totalBalance > 0 ? (totalReservedBalance / totalBalance) * 100 : 0;
    }

    private decimal CalculateCurrencyRisk(List<TreasuryAccount> accounts)
    {
        if (!accounts.Any()) return 0;

        var currencyGroups = accounts.GroupBy(a => a.Currency).Count();
        
        // Higher currency diversity = higher risk (simplified model)
        return Math.Min(currencyGroups * 10, 100); // Cap at 100%
    }

    private decimal CalculateOperationalRisk(List<TreasuryAccount> accounts)
    {
        if (!accounts.Any()) return 0;

        // Simple operational risk based on account complexity
        var complexAccounts = accounts.Count(a => a.RequiresApproval);
        var totalAccounts = accounts.Count;

        return totalAccounts > 0 ? (decimal)complexAccounts / totalAccounts * 100 : 0;
    }

    #endregion
}
