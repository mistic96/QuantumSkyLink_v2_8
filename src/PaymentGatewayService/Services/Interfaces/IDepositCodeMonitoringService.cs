namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Interface for monitoring and recording deposit code metrics
/// </summary>
public interface IDepositCodeMonitoringService
{
    /// <summary>
    /// Records deposit code validation metrics
    /// </summary>
    /// <param name="isValid">Whether the validation was successful</param>
    /// <param name="rejectionReason">The reason for rejection if validation failed</param>
    void RecordDepositCodeValidation(bool isValid, string? rejectionReason = null);

    /// <summary>
    /// Records deposit code generation metrics
    /// </summary>
    /// <param name="isSuccessful">Whether the generation was successful</param>
    /// <param name="userId">The user ID for whom the code was generated</param>
    void RecordDepositCodeGeneration(bool isSuccessful, string? userId = null);

    /// <summary>
    /// Records deposit code usage metrics
    /// </summary>
    /// <param name="code">The deposit code that was used</param>
    /// <param name="userId">The user ID who used the code</param>
    /// <param name="amount">The amount associated with the usage</param>
    /// <param name="currency">The currency of the transaction</param>
    void RecordDepositCodeUsage(string code, string userId, decimal amount, string currency);

    /// <summary>
    /// Records deposit code expiration events
    /// </summary>
    /// <param name="code">The deposit code that expired</param>
    /// <param name="wasUsed">Whether the code was used before expiration</param>
    void RecordDepositCodeExpiration(string code, bool wasUsed);

    /// <summary>
    /// Gets the current monitoring statistics
    /// </summary>
    /// <returns>The monitoring statistics</returns>
    Task<DepositCodeMonitoringStats> GetMonitoringStatsAsync();

    /// <summary>
    /// Records compliance events for audit purposes
    /// </summary>
    /// <param name="eventType">The type of compliance event</param>
    /// <param name="details">Details about the event</param>
    void RecordComplianceEvent(string eventType, string details);

    /// <summary>
    /// Records security events for monitoring
    /// </summary>
    /// <param name="eventType">The type of security event</param>
    /// <param name="details">Details about the event</param>
    void RecordSecurityEvent(string eventType, string details);

    /// <summary>
    /// Records performance metrics
    /// </summary>
    /// <param name="metricName">The name of the metric</param>
    /// <param name="value">The metric value</param>
    void RecordPerformanceMetric(string metricName, double value);

    /// <summary>
    /// Checks and triggers alerts based on monitoring thresholds
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task CheckAndTriggerAlertsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deposit code metrics
    /// </summary>
    Task<DepositCodeMetrics> GetDepositCodeMetricsAsync();

    /// <summary>
    /// Gets security metrics
    /// </summary>
    Task<SecurityMetrics> GetSecurityMetricsAsync();

    /// <summary>
    /// Gets performance metrics
    /// </summary>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();
}

/// <summary>
/// Deposit code metrics
/// </summary>
public class DepositCodeMetrics
{
    public int TotalCodes { get; set; }
    public int ActiveCodes { get; set; }
    public int UsedCodes { get; set; }
    public int ExpiredCodes { get; set; }
    public decimal TotalVolume { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Security metrics
/// </summary>
public class SecurityMetrics
{
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public int SuspiciousActivities { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance metrics
/// </summary>
public class PerformanceMetrics
{
    public double AverageResponseTime { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double Uptime { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Statistics for deposit code monitoring
/// </summary>
public class DepositCodeMonitoringStats
{
    public long TotalCodesGenerated { get; set; }
    public long TotalCodesUsed { get; set; }
    public long TotalCodesExpired { get; set; }
    public long ValidationSuccesses { get; set; }
    public long ValidationFailures { get; set; }
    public Dictionary<string, long> FailureReasons { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
