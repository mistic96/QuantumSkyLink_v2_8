using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PaymentGatewayService.Services.Interfaces;

namespace PaymentGatewayService.Services;

/// <summary>
/// Background service that continuously monitors deposit code metrics and triggers alerts
/// Runs on configurable intervals to ensure real-time monitoring
/// </summary>
public class DepositCodeAlertingService : BackgroundService
{
    private readonly ILogger<DepositCodeAlertingService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _checkInterval;

    public DepositCodeAlertingService(
        ILogger<DepositCodeAlertingService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        
        // Default to 5-minute intervals, configurable via appsettings
        var intervalMinutes = _configuration.GetValue<int>("DepositCodeMonitoring:AlertCheckIntervalMinutes", 5);
        _checkInterval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Deposit Code Alerting Service started. Check interval: {Interval}", _checkInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoringService = scope.ServiceProvider.GetRequiredService<IDepositCodeMonitoringService>();

                await monitoringService.CheckAndTriggerAlertsAsync();
                
                _logger.LogDebug("Alert check completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during alert checking cycle");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Service is being stopped
                break;
            }
        }

        _logger.LogInformation("Deposit Code Alerting Service stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deposit Code Alerting Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
