namespace PaymentGatewayService.Services;

/// <summary>
/// Deposit code metrics for monitoring
/// </summary>
public class DepositCodeMetrics
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalValidations { get; set; }
    public int SuccessfulValidations { get; set; }
    public int FailedValidations { get; set; }
    public double SuccessRate { get; set; }
    public int TotalGenerations { get; set; }
    public int TotalUsage { get; set; }
    public int ActiveCodes { get; set; }
    public int ExpiredCodes { get; set; }
    public Dictionary<string, int> RejectionReasons { get; set; } = new();
}

/// <summary>
/// Security metrics for monitoring
/// </summary>
public class SecurityMetrics
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalSecurityEvents { get; set; }
    public int SuspiciousActivityEvents { get; set; }
    public int DuplicateAttempts { get; set; }
    public int UnauthorizedAccess { get; set; }
    public int HighRiskEvents { get; set; }
    public double SecurityIncidentFrequency { get; set; }
}

/// <summary>
/// Performance metrics for monitoring
/// </summary>
public class PerformanceMetrics
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public TimeSpan AverageValidationTime { get; set; }
    public TimeSpan AverageGenerationTime { get; set; }
    public TimeSpan AverageRejectionProcessingTime { get; set; }
    public TimeSpan P95ValidationTime { get; set; }
    public TimeSpan P99ValidationTime { get; set; }
    public double ServiceUptime { get; set; }
    public double ErrorRate { get; set; }
}