using MobileAPIGateway.Models.Infrastructure;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service interface for mobile infrastructure operations
/// </summary>
public interface IInfrastructureService
{
    /// <summary>
    /// Get system health status for mobile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System health status</returns>
    Task<MobileHealthStatusResponse> GetMobileHealthStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app configuration
    /// </summary>
    /// <param name="appVersion">Mobile app version</param>
    /// <param name="platform">Mobile platform (iOS/Android)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile app configuration</returns>
    Task<MobileConfigResponse> GetMobileConfigAsync(
        string appVersion,
        string platform,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feature flags for mobile app
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="appVersion">Mobile app version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Feature flags configuration</returns>
    Task<MobileFeatureFlagsResponse> GetMobileFeatureFlagsAsync(
        Guid userId,
        string appVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system status for mobile dashboard
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System status information</returns>
    Task<MobileSystemStatusResponse> GetMobileSystemStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get maintenance notifications for mobile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of maintenance notifications</returns>
    Task<IEnumerable<MaintenanceNotificationResponse>> GetMaintenanceNotificationsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app limits and quotas
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile app limits</returns>
    Task<MobileLimitsResponse> GetMobileLimitsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Report mobile app metrics
    /// </summary>
    /// <param name="request">Mobile metrics request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    Task<bool> ReportMobileMetricsAsync(
        MobileMetricsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app performance data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="timeRange">Time range for performance data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile performance data</returns>
    Task<MobilePerformanceResponse> GetMobilePerformanceAsync(
        Guid userId,
        string timeRange = "24h",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile connectivity status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connectivity status</returns>
    Task<MobileConnectivityResponse> GetMobileConnectivityAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit mobile error report
    /// </summary>
    /// <param name="request">Error report request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Error report submission result</returns>
    Task<ErrorReportResponse> SubmitMobileErrorReportAsync(
        MobileErrorReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app update information
    /// </summary>
    /// <param name="currentVersion">Current app version</param>
    /// <param name="platform">Mobile platform</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>App update information</returns>
    Task<MobileUpdateResponse> GetMobileUpdateInfoAsync(
        string currentVersion,
        string platform,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile security settings
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile security settings</returns>
    Task<MobileSecuritySettingsResponse> GetMobileSecuritySettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update mobile device registration
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Device registration request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Device registration result</returns>
    Task<DeviceRegistrationResponse> UpdateMobileDeviceAsync(
        Guid userId,
        MobileDeviceRegistrationRequest request,
        CancellationToken cancellationToken = default);
}
