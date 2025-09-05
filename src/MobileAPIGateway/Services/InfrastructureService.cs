using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Infrastructure;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service implementation for mobile infrastructure operations
/// </summary>
public class InfrastructureService : IInfrastructureService
{
    private readonly IInfrastructureServiceClient _infrastructureClient;
    private readonly ILogger<InfrastructureService> _logger;

    public InfrastructureService(
        IInfrastructureServiceClient infrastructureClient,
        ILogger<InfrastructureService> logger)
    {
        _infrastructureClient = infrastructureClient;
        _logger = logger;
    }

    /// <summary>
    /// Get system health status for mobile
    /// </summary>
    public async Task<MobileHealthStatusResponse> GetMobileHealthStatusAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile health status");
            
            var response = await _infrastructureClient.GetMobileHealthStatusAsync(cancellationToken);
            
            _logger.LogInformation("Successfully retrieved mobile health status: {Status}", response.Status);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile health status");
            throw;
        }
    }

    /// <summary>
    /// Get mobile app configuration
    /// </summary>
    public async Task<MobileConfigResponse> GetMobileConfigAsync(
        string appVersion,
        string platform,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile config for version {AppVersion}, platform {Platform}", 
                appVersion, platform);
            
            var response = await _infrastructureClient.GetMobileConfigAsync(appVersion, platform, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved mobile config for version {AppVersion}, platform {Platform}", 
                appVersion, platform);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile config for version {AppVersion}, platform {Platform}", 
                appVersion, platform);
            throw;
        }
    }

    /// <summary>
    /// Get feature flags for mobile app
    /// </summary>
    public async Task<MobileFeatureFlagsResponse> GetMobileFeatureFlagsAsync(
        Guid userId,
        string appVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile feature flags for user {UserId}, version {AppVersion}", 
                userId, appVersion);
            
            var response = await _infrastructureClient.GetMobileFeatureFlagsAsync(userId, appVersion, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved {Count} feature flags for user {UserId}", 
                response.Features.Count, userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile feature flags for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get system status for mobile dashboard
    /// </summary>
    public async Task<MobileSystemStatusResponse> GetMobileSystemStatusAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile system status");
            
            var response = await _infrastructureClient.GetMobileSystemStatusAsync(cancellationToken);
            
            _logger.LogInformation("Successfully retrieved mobile system status: {Status}", response.Status);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile system status");
            throw;
        }
    }

    /// <summary>
    /// Get maintenance notifications for mobile
    /// </summary>
    public async Task<IEnumerable<MaintenanceNotificationResponse>> GetMaintenanceNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting maintenance notifications");
            
            var response = await _infrastructureClient.GetMaintenanceNotificationsAsync(cancellationToken);
            
            _logger.LogInformation("Successfully retrieved {Count} maintenance notifications", response.Count());
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting maintenance notifications");
            throw;
        }
    }

    /// <summary>
    /// Get mobile app limits and quotas
    /// </summary>
    public async Task<MobileLimitsResponse> GetMobileLimitsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile limits for user {UserId}", userId);
            
            var response = await _infrastructureClient.GetMobileLimitsAsync(userId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved mobile limits for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile limits for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Report mobile app metrics
    /// </summary>
    public async Task<bool> ReportMobileMetricsAsync(
        MobileMetricsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reporting mobile metrics for user {UserId}, platform {Platform}", 
                request.UserId, request.Platform);
            
            var response = await _infrastructureClient.ReportMobileMetricsAsync(request, cancellationToken);
            
            _logger.LogInformation("Successfully reported mobile metrics for user {UserId}", request.UserId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting mobile metrics for user {UserId}", request.UserId);
            throw;
        }
    }

    /// <summary>
    /// Get mobile app performance data
    /// </summary>
    public async Task<MobilePerformanceResponse> GetMobilePerformanceAsync(
        Guid userId,
        string timeRange = "24h",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile performance for user {UserId}, range {TimeRange}", 
                userId, timeRange);
            
            var response = await _infrastructureClient.GetMobilePerformanceAsync(userId, timeRange, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved mobile performance for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile performance for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get mobile connectivity status
    /// </summary>
    public async Task<MobileConnectivityResponse> GetMobileConnectivityAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile connectivity status");
            
            var response = await _infrastructureClient.GetMobileConnectivityAsync(cancellationToken);
            
            _logger.LogInformation("Successfully retrieved mobile connectivity status: {Status}", response.Status);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile connectivity status");
            throw;
        }
    }

    /// <summary>
    /// Submit mobile error report
    /// </summary>
    public async Task<ErrorReportResponse> SubmitMobileErrorReportAsync(
        MobileErrorReportRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting mobile error report for user {UserId}, error type {ErrorType}", 
                request.UserId, request.ErrorType);
            
            var response = await _infrastructureClient.SubmitMobileErrorReportAsync(request, cancellationToken);
            
            _logger.LogInformation("Successfully submitted mobile error report for user {UserId}, report ID {ReportId}", 
                request.UserId, response.ReportId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting mobile error report for user {UserId}", request.UserId);
            throw;
        }
    }

    /// <summary>
    /// Get mobile app update information
    /// </summary>
    public async Task<MobileUpdateResponse> GetMobileUpdateInfoAsync(
        string currentVersion,
        string platform,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile update info for version {CurrentVersion}, platform {Platform}", 
                currentVersion, platform);
            
            var response = await _infrastructureClient.GetMobileUpdateInfoAsync(currentVersion, platform, cancellationToken);
            
            _logger.LogInformation("Mobile update check completed for version {CurrentVersion}, update available: {UpdateAvailable}", 
                currentVersion, response.UpdateAvailable);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile update info for version {CurrentVersion}, platform {Platform}", 
                currentVersion, platform);
            throw;
        }
    }

    /// <summary>
    /// Get mobile security settings
    /// </summary>
    public async Task<MobileSecuritySettingsResponse> GetMobileSecuritySettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting mobile security settings for user {UserId}", userId);
            
            var response = await _infrastructureClient.GetMobileSecuritySettingsAsync(userId, cancellationToken);
            
            _logger.LogInformation("Successfully retrieved mobile security settings for user {UserId}", userId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mobile security settings for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Update mobile device registration
    /// </summary>
    public async Task<DeviceRegistrationResponse> UpdateMobileDeviceAsync(
        Guid userId,
        MobileDeviceRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating mobile device for user {UserId}, device {DeviceId}", 
                userId, request.DeviceId);
            
            var response = await _infrastructureClient.UpdateMobileDeviceAsync(userId, request, cancellationToken);
            
            _logger.LogInformation("Successfully updated mobile device for user {UserId}, device {DeviceId}", 
                userId, response.DeviceId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mobile device for user {UserId}", userId);
            throw;
        }
    }
}
