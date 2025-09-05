namespace MobileAPIGateway.Models.Infrastructure;

/// <summary>
/// Mobile health status response
/// </summary>
public class MobileHealthStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public IEnumerable<ServiceHealthStatus> Services { get; set; } = new List<ServiceHealthStatus>();
    public string OverallHealth { get; set; } = string.Empty;
    public IEnumerable<string> Issues { get; set; } = new List<string>();
}

/// <summary>
/// Service health status
/// </summary>
public class ServiceHealthStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Mobile configuration response
/// </summary>
public class MobileConfigResponse
{
    public string AppVersion { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public IEnumerable<string> EnabledFeatures { get; set; } = new List<string>();
    public Dictionary<string, string> ApiEndpoints { get; set; } = new();
    public MobileThemeConfig Theme { get; set; } = new();
    public MobileSecurityConfig Security { get; set; } = new();
}

/// <summary>
/// Mobile theme configuration
/// </summary>
public class MobileThemeConfig
{
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
}

/// <summary>
/// Mobile security configuration
/// </summary>
public class MobileSecurityConfig
{
    public bool BiometricEnabled { get; set; }
    public bool PinRequired { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public bool TwoFactorRequired { get; set; }
    public IEnumerable<string> AllowedNetworks { get; set; } = new List<string>();
}

/// <summary>
/// Mobile feature flags response
/// </summary>
public class MobileFeatureFlagsResponse
{
    public Guid UserId { get; set; }
    public string AppVersion { get; set; } = string.Empty;
    public Dictionary<string, bool> Features { get; set; } = new();
    public Dictionary<string, object> FeatureConfigs { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Mobile system status response
/// </summary>
public class MobileSystemStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<SystemComponent> Components { get; set; } = new List<SystemComponent>();
    public bool MaintenanceMode { get; set; }
    public DateTime? NextMaintenanceWindow { get; set; }
}

/// <summary>
/// System component status
/// </summary>
public class SystemComponent
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
}

/// <summary>
/// Maintenance notification response
/// </summary>
public class MaintenanceNotificationResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Severity { get; set; } = string.Empty;
    public IEnumerable<string> AffectedServices { get; set; } = new List<string>();
    public bool IsActive { get; set; }
}

/// <summary>
/// Mobile limits response
/// </summary>
public class MobileLimitsResponse
{
    public Guid UserId { get; set; }
    public Dictionary<string, decimal> TransactionLimits { get; set; } = new();
    public Dictionary<string, int> RateLimits { get; set; } = new();
    public Dictionary<string, long> DataLimits { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Mobile metrics request
/// </summary>
public class MobileMetricsRequest
{
    public Guid UserId { get; set; }
    public string AppVersion { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Mobile performance response
/// </summary>
public class MobilePerformanceResponse
{
    public Guid UserId { get; set; }
    public string TimeRange { get; set; } = string.Empty;
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public IEnumerable<PerformanceDataPoint> DataPoints { get; set; } = new List<PerformanceDataPoint>();
    public string OverallRating { get; set; } = string.Empty;
}

/// <summary>
/// Performance data point
/// </summary>
public class PerformanceDataPoint
{
    public DateTime Timestamp { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// Mobile connectivity response
/// </summary>
public class MobileConnectivityResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public IEnumerable<ConnectivityTest> Tests { get; set; } = new List<ConnectivityTest>();
    public string RecommendedAction { get; set; } = string.Empty;
}

/// <summary>
/// Connectivity test result
/// </summary>
public class ConnectivityTest
{
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Mobile error report request
/// </summary>
public class MobileErrorReportRequest
{
    public Guid UserId { get; set; }
    public string AppVersion { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Error report response
/// </summary>
public class ErrorReportResponse
{
    public Guid ReportId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? TicketNumber { get; set; }
}

/// <summary>
/// Mobile update response
/// </summary>
public class MobileUpdateResponse
{
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public bool UpdateAvailable { get; set; }
    public bool UpdateRequired { get; set; }
    public string UpdateType { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
}

/// <summary>
/// Mobile security settings response
/// </summary>
public class MobileSecuritySettingsResponse
{
    public Guid UserId { get; set; }
    public bool BiometricEnabled { get; set; }
    public bool PinEnabled { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public IEnumerable<string> TrustedDevices { get; set; } = new List<string>();
    public DateTime LastSecurityUpdate { get; set; }
}

/// <summary>
/// Mobile device registration request
/// </summary>
public class MobileDeviceRegistrationRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string PushToken { get; set; } = string.Empty;
    public Dictionary<string, string> DeviceInfo { get; set; } = new();
}

/// <summary>
/// Device registration response
/// </summary>
public class DeviceRegistrationResponse
{
    public Guid DeviceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public bool IsTrusted { get; set; }
}
