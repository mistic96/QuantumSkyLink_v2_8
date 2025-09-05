using AdminAPIGateway.Controllers;

namespace AdminAPIGateway.Services;

/// <summary>
/// Client interface for communicating with PaymentGatewayService
/// </summary>
public interface IPaymentGatewayClient
{
    // Deposit Code Review Management
    Task<PaginatedResponse<DepositCodeReviewItem>> GetPendingReviewCodesAsync(
        int page, int pageSize, string? sortBy, bool sortDescending, string? filterReason);
    
    Task<DepositCodeReviewDetail?> GetDepositCodeReviewDetailAsync(Guid depositCodeId);
    
    Task<DepositCodeApprovalResult> ApproveDepositCodeAsync(Guid depositCodeId, DepositCodeApprovalRequest request);
    
    Task<DepositCodeRejectionResult> RejectDepositCodeAsync(Guid depositCodeId, DepositCodeRejectionRequest request);
    
    Task<BulkActionResult> BulkDepositCodeActionAsync(BulkDepositCodeActionRequest request);
    
    // Statistics and Analytics
    Task<DuplicateStatistics> GetDuplicateStatisticsAsync(DateTime startDate, DateTime endDate);
    
    // Search and Export
    Task<PaginatedResponse<DepositCodeSearchResult>> SearchDepositCodesAsync(DepositCodeSearchRequest request);
    
    Task<List<DepositCodeAuditEntry>> GetDepositCodeAuditTrailAsync(Guid depositCodeId);
    
    Task<byte[]> ExportDepositCodesAsync(DepositCodeExportRequest request);
    
    // Monitoring endpoints (existing)
    Task<DepositCodeMetrics> GetDepositCodeMetricsAsync(DateTime? startDate, DateTime? endDate);
    
    Task<List<SecurityEvent>> GetSecurityEventsAsync(DateTime? startDate, DateTime? endDate);
    
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime? startDate, DateTime? endDate);
    
    Task<DashboardData> GetDashboardDataAsync();
    
    Task<HealthCheckResult> GetHealthStatusAsync();
}

// Additional models for existing monitoring functionality
public class DepositCodeMetrics
{
    public int TotalValidations { get; set; }
    public int SuccessfulValidations { get; set; }
    public int FailedValidations { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<string, int> FailureReasons { get; set; } = new();
    public Dictionary<string, int> ValidationsByHour { get; set; } = new();
}

public class SecurityEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class PerformanceMetrics
{
    public double AverageValidationTime { get; set; }
    public double P95ValidationTime { get; set; }
    public double P99ValidationTime { get; set; }
    public int TotalRequests { get; set; }
    public Dictionary<string, double> AverageTimeByOperation { get; set; } = new();
}

public class DashboardData
{
    public DepositCodeMetrics Metrics { get; set; } = new();
    public List<SecurityEvent> RecentSecurityEvents { get; set; } = new();
    public PerformanceMetrics Performance { get; set; } = new();
    public int PendingReviewCount { get; set; }
    public int ActiveCodesCount { get; set; }
}

public class HealthCheckResult
{
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> Components { get; set; } = new();
    public DateTime Timestamp { get; set; }
}