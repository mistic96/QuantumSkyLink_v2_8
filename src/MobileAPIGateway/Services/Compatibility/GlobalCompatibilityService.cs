//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.Compatibility.Global;
//using MobileAPIGateway.Models.Global;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the global compatibility service
///// </summary>
//public class GlobalCompatibilityService : IGlobalCompatibilityService
//{
//    private readonly IGlobalService _globalService;
//    private readonly ILogger<GlobalCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="GlobalCompatibilityService"/> class
//    /// </summary>
//    /// <param name="globalService">The global service</param>
//    /// <param name="logger">The logger</param>
//    public GlobalCompatibilityService(IGlobalService globalService, ILogger<GlobalCompatibilityService> logger)
//    {
//        _globalService = globalService ?? throw new ArgumentNullException(nameof(globalService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    ///// <inheritdoc />
//    //public async Task<SystemStatusCompatibilityResponse> GetSystemStatusAsync(SystemStatusCompatibilityRequest request)
//    //{
//    //    try
//    //    {
//    //        _logger.LogInformation("Getting system status for client version: {ClientVersion}", request.ClientVersion);
            
//    //        // Call the new service
//    //        var systemStatus = await _globalService.GetSystemStatusAsync();
            
//    //        // Map from new response model to compatibility response
//    //        return new SystemStatusCompatibilityResponse
//    //        {
//    //            IsSuccessful = true,
//    //            Message = "System status retrieved successfully",
//    //            IsSystemOnline = systemStatus.IsSystemOnline,
//    //            IsMaintenanceInProgress = systemStatus.IsMaintenanceInProgress,
//    //            MaintenanceMessage = systemStatus.MaintenanceMessage,
//    //            MaintenanceEndTime = systemStatus.MaintenanceEndTime,
//    //            IsUpdateRequired = systemStatus.IsUpdateRequired,
//    //            MinimumRequiredVersion = systemStatus.MinimumRequiredVersion,
//    //            LatestVersion = systemStatus.LatestVersion,
//    //            UpdateUrl = systemStatus.UpdateUrl,
//    //            ServerTime = DateTime.UtcNow
//    //        };
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogError(ex, "Error getting system status for client version: {ClientVersion}", request.ClientVersion);
//    //        return new SystemStatusCompatibilityResponse
//    //        {
//    //            IsSuccessful = false,
//    //            Message = $"Error getting system status: {ex.Message}",
//    //            IsSystemOnline = false,
//    //            ServerTime = DateTime.UtcNow
//    //        };
//    //    }
//    //}
    
//    /// <inheritdoc />
//    //public async Task<AppConfigCompatibilityResponse> GetAppConfigAsync(AppConfigCompatibilityRequest request)
//    //{
//    //    try
//    //    {
//    //        _logger.LogInformation("Getting app configuration for key: {ConfigKey}", request.ConfigKey);
            
//    //        // Call the new service
//    //        var appConfig = await _globalService.GetAppConfigAsync(request.ConfigKey);
            
//    //        // Map from new response model to compatibility response
//    //        return new AppConfigCompatibilityResponse
//    //        {
//    //            IsSuccessful = true,
//    //            Message = "App configuration retrieved successfully",
//    //            ConfigKey = appConfig.Key,
//    //            ConfigValue = appConfig.Value,
//    //            ConfigDescription = appConfig.Description,
//    //            ConfigGroup = appConfig.Group,
//    //            ConfigType = appConfig.Type,
//    //            IsEnabled = appConfig.IsEnabled,
//    //            ServerTime = DateTime.UtcNow
//    //        };
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogError(ex, "Error getting app configuration for key: {ConfigKey}", request.ConfigKey);
//    //        return new AppConfigCompatibilityResponse
//    //        {
//    //            IsSuccessful = false,
//    //            Message = $"Error getting app configuration: {ex.Message}",
//    //            ServerTime = DateTime.UtcNow
//    //        };
//    //    }
//    //}
    
//    /// <inheritdoc />
//    //public async Task<LimitCompatibilityResponse> GetLimitAsync(LimitCompatibilityRequest request)
//    //{
//    //    try
//    //    {
//    //        _logger.LogInformation("Getting limit for type: {LimitType}", request.LimitType);
            
//    //        // Call the new service
//    //        var limit = await _globalService..GetLimitAsync(request.LimitType, request.UserId);
            
//    //        // Map from new response model to compatibility response
//    //        return new LimitCompatibilityResponse
//    //        {
//    //            IsSuccessful = true,
//    //            Message = "Limit retrieved successfully",
//    //            LimitType = limit.LimitType,
//    //            LimitValue = limit.LimitValue,
//    //            LimitCurrency = limit.LimitCurrency,
//    //            LimitPeriod = limit.LimitPeriod,
//    //            LimitResetDate = limit.LimitResetDate,
//    //            CurrentUsage = limit.CurrentUsage,
//    //            RemainingAmount = limit.RemainingAmount,
//    //            IsLimitExceeded = limit.IsLimitExceeded,
//    //            ServerTime = DateTime.UtcNow
//    //        };
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogError(ex, "Error getting limit for type: {LimitType}", request.LimitType);
//    //        return new LimitCompatibilityResponse
//    //        {
//    //            IsSuccessful = false,
//    //            Message = $"Error getting limit: {ex.Message}",
//    //            ServerTime = DateTime.UtcNow
//    //        };
//    //    }
//    //}
//}
