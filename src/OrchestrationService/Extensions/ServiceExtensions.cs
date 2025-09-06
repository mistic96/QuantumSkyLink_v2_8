using OrchestrationService.Clients;
using OrchestrationService.Services;
using OrchestrationService.Endpoints;
using Refit;
using Polly;
using Polly.Extensions.Http;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OrchestrationService.Extensions;

/// <summary>
/// Extension methods for service registration and configuration
/// Following the same pattern as SignatureService
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Add OrchestrationService API documentation
    /// </summary>
    public static IServiceCollection AddOrchestrationServiceApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "OrchestrationService API",
                Version = "v1",
                Description = "Workflow orchestration service with Kestra integration and zero-trust security",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "QuantumSkyLink v2",
                    Email = "support@quantumskylink.com"
                }
            });

            options.EnableAnnotations();
            // XML documentation is optional - commented out to prevent startup errors
            // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "OrchestrationService.xml"), true);
        });

        return services;
    }

    /// <summary>
    /// Configure CORS for OrchestrationService
    /// </summary>
    public static IServiceCollection AddOrchestrationServiceCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Add performance monitoring and metrics
    /// </summary>
    public static IServiceCollection AddOrchestrationServiceMetrics(this IServiceCollection services)
    {
        services.AddSingleton<IMetricsLogger, MetricsLogger>();
        return services;
    }

    /// <summary>
    /// Register all service clients with Refit
    /// </summary>
    public static IServiceCollection AddOrchestrationServiceClients(this IServiceCollection services)
    {
        // SignatureService client with fail-fast timeout
        services.AddRefitClient<ISignatureServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://signatureservice");
                client.Timeout = TimeSpan.FromSeconds(1); // Fail-fast for signatures
            })
            .AddPolicyHandler(GetRetryPolicy("SignatureService", retryCount: 0)); // No retries for signatures

        // Internal SignatureService multisig endpoints (internal)
        services.AddRefitClient<IInternalMultisigClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://signatureservice");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(GetRetryPolicy("SignatureServiceInternal", retryCount: 1));

        // PaymentGatewayService client
        services.AddRefitClient<IPaymentGatewayServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://paymentgatewayservice");
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddPolicyHandler(GetRetryPolicy("PaymentGatewayService"));

        // QuantumLedger.Hub client
        services.AddRefitClient<IQuantumLedgerHubClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://quantumledgerhub");
                client.Timeout = TimeSpan.FromSeconds(3);
            })
            .AddPolicyHandler(GetRetryPolicy("QuantumLedgerHub"));

        // UserService client
        services.AddRefitClient<IUserServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://userservice");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddPolicyHandler(GetRetryPolicy("UserService"));

        // AccountService client
        services.AddRefitClient<IAccountServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://accountservice");
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddPolicyHandler(GetRetryPolicy("AccountService"));

        // TreasuryService client
        services.AddRefitClient<ITreasuryServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://treasuryservice");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddPolicyHandler(GetRetryPolicy("TreasuryService"));

        // NotificationService client
        services.AddRefitClient<INotificationServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://notificationservice");
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddPolicyHandler(GetRetryPolicy("NotificationService"));

        // IdentityVerificationService client
        services.AddRefitClient<IIdentityVerificationServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://identityverificationservice");
                client.Timeout = TimeSpan.FromSeconds(30); // KYC can take longer
            })
            .AddPolicyHandler(GetRetryPolicy("IdentityVerificationService"));

        // MarketplaceService client
        services.AddRefitClient<IMarketplaceServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https+http://marketplaceservice");
                client.Timeout = TimeSpan.FromSeconds(10); // Marketplace operations
            })
            .AddPolicyHandler(GetRetryPolicy("MarketplaceService"));

        // Kestra Workflow Engine client (External)
        services.AddRefitClient<IKestraClient>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                
                // Use Aspire connection string for external Kestra
                var kestraUrl = configuration.GetConnectionString("kestra-external") ?? "http://localhost:8080";
                client.BaseAddress = new Uri(kestraUrl);
                client.Timeout = TimeSpan.FromSeconds(30); // Workflows can take time
                
                // Add basic authentication if credentials are provided
                var username = Environment.GetEnvironmentVariable("KESTRA_USERNAME");
                var password = Environment.GetEnvironmentVariable("KESTRA_PASSWORD");
                
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                }
            })
            .AddPolicyHandler(GetRetryPolicy("Kestra", retryCount: 1)); // Minimal retries for workflow engine

        return services;
    }

    /// <summary>
    /// Register OrchestrationService core services
    /// </summary>
    public static IServiceCollection AddOrchestrationServices(this IServiceCollection services)
    {
        // Core workflow services
        services.AddScoped<WorkflowManager>();
        services.AddScoped<WorkflowStatusService>();
        services.AddScoped<WorkflowEventPublisher>();

        // Memory cache for workflow execution context
        services.AddMemoryCache();

        return services;
    }

    /// <summary>
    /// Map OrchestrationService endpoints
    /// </summary>
    public static WebApplication MapOrchestrationServiceEndpoints(this WebApplication app)
    {
        app.MapOrchestrationEndpoints();
        return app;
    }

    /// <summary>
    /// Get retry policy for service clients
    /// </summary>
    private static Polly.IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(string serviceName, int retryCount = 2)
    {
        return Polly.Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    logger?.LogWarning("Retry {RetryCount} for {ServiceName} after {Delay}ms",
                        retryCount, serviceName, timespan.TotalMilliseconds);
                });
    }
}

/// <summary>
/// Metrics logger interface for performance monitoring
/// </summary>
public interface IMetricsLogger
{
    void RecordWorkflowExecution(string workflowType, TimeSpan duration, string status);
    void RecordServiceCall(string serviceName, TimeSpan duration, bool success);
}

/// <summary>
/// Implementation of metrics logger
/// </summary>
public class MetricsLogger : IMetricsLogger
{
    private readonly ILogger<MetricsLogger> _logger;

    public MetricsLogger(ILogger<MetricsLogger> logger)
    {
        _logger = logger;
    }

    public void RecordWorkflowExecution(string workflowType, TimeSpan duration, string status)
    {
        _logger.LogInformation("METRIC: Workflow execution - Type: {WorkflowType}, Duration: {Duration}ms, Status: {Status}",
            workflowType, duration.TotalMilliseconds, status);
    }

    public void RecordServiceCall(string serviceName, TimeSpan duration, bool success)
    {
        _logger.LogInformation("METRIC: Service call - Service: {ServiceName}, Duration: {Duration}ms, Success: {Success}",
            serviceName, duration.TotalMilliseconds, success);
    }
}

/// <summary>
/// Extension methods for Polly context
/// </summary>
public static class PollyContextExtensions
{
    public static ILogger? GetLogger(this Polly.Context context)
    {
        if (context.TryGetValue("logger", out var logger) && logger is ILogger log)
        {
            return log;
        }
        return null;
    }
}
