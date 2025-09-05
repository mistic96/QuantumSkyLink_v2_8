using Refit;
using MobileAPIGateway.Models.Infrastructure;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Client interface for InfrastructureService integration
/// </summary>
public interface IInfrastructureServiceClient
{
    /// <summary>
    /// Get system health status for mobile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System health status</returns>
    [Get("/api/infrastructure/health/mobile")]
    Task<MobileHealthStatusResponse> GetMobileHealthStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app configuration
    /// </summary>
    /// <param name="appVersion">Mobile app version</param>
    /// <param name="platform">Mobile platform (iOS/Android)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile app configuration</returns>
    [Get("/api/infrastructure/config/mobile")]
    Task<MobileConfigResponse> GetMobileConfigAsync(
        [Query] string appVersion,
        [Query] string platform,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feature flags for mobile app
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="appVersion">Mobile app version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Feature flags configuration</returns>
    [Get("/api/infrastructure/features/mobile/{userId}")]
    Task<MobileFeatureFlagsResponse> GetMobileFeatureFlagsAsync(
        Guid userId,
        [Query] string appVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system status for mobile dashboard
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System status information</returns>
    [Get("/api/infrastructure/status/mobile")]
    Task<MobileSystemStatusResponse> GetMobileSystemStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get maintenance notifications for mobile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of maintenance notifications</returns>
    [Get("/api/infrastructure/maintenance/mobile")]
    Task<IEnumerable<MaintenanceNotificationResponse>> GetMaintenanceNotificationsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app limits and quotas
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile app limits</returns>
    [Get("/api/infrastructure/limits/mobile/{userId}")]
    Task<MobileLimitsResponse> GetMobileLimitsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Report mobile app metrics
    /// </summary>
    /// <param name="request">Mobile metrics request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [Post("/api/infrastructure/metrics/mobile")]
    Task<bool> ReportMobileMetricsAsync(
        [Body] MobileMetricsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app performance data
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="timeRange">Time range for performance data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile performance data</returns>
    [Get("/api/infrastructure/performance/mobile/{userId}")]
    Task<MobilePerformanceResponse> GetMobilePerformanceAsync(
        Guid userId,
        [Query] string timeRange = "24h",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile connectivity status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connectivity status</returns>
    [Get("/api/infrastructure/connectivity/mobile")]
    Task<MobileConnectivityResponse> GetMobileConnectivityAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit mobile error report
    /// </summary>
    /// <param name="request">Error report request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Error report submission result</returns>
    [Post("/api/infrastructure/errors/mobile")]
    Task<ErrorReportResponse> SubmitMobileErrorReportAsync(
        [Body] MobileErrorReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile app update information
    /// </summary>
    /// <param name="currentVersion">Current app version</param>
    /// <param name="platform">Mobile platform</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>App update information</returns>
    [Get("/api/infrastructure/updates/mobile")]
    Task<MobileUpdateResponse> GetMobileUpdateInfoAsync(
        [Query] string currentVersion,
        [Query] string platform,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get mobile security settings
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Mobile security settings</returns>
    [Get("/api/infrastructure/security/mobile/{userId}")]
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
    [Put("/api/infrastructure/devices/mobile/{userId}")]
    Task<DeviceRegistrationResponse> UpdateMobileDeviceAsync(
        Guid userId,
        [Body] MobileDeviceRegistrationRequest request,
        CancellationToken cancellationToken = default);
}
