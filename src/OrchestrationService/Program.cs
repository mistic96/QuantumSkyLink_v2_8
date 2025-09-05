using Microsoft.EntityFrameworkCore;
using Serilog;
using OrchestrationService.Extensions;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.Extensions.NETCore.Setup;
using OrchestrationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (same pattern as SignatureService and QuantumLedger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/orchestration-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Aspire service defaults (same as all QuantumSkyLink v2 services)
builder.AddServiceDefaults();

// Add Redis distributed cache for workflow execution context and status tracking
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache");
    options.InstanceName = "OrchestrationService";
});

// Add OrchestrationService API documentation and Minimal APIs support
builder.Services.AddOrchestrationServiceApiDocumentation();

// Configure CORS for cross-system communication
builder.Services.AddOrchestrationServiceCors();

// Add performance monitoring and metrics
builder.Services.AddOrchestrationServiceMetrics();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Register AWS services for SNS/SQS event integration
Log.Logger.Information("Initializing AWS services for event integration");

builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
builder.Services.AddAWSService<IAmazonSQS>();

// Register all service clients with Refit and proper timeouts
Log.Logger.Information("Registering service clients for workflow orchestration");
builder.Services.AddOrchestrationServiceClients();

// Register OrchestrationService core services
builder.Services.AddOrchestrationServices();

// Configure performance optimization services
builder.Services.AddMemoryCache();

// Configure connection pooling for optimal performance
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
});

var app = builder.Build();

// Map Aspire default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrchestrationService API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Add custom middleware for performance headers and workflow tracking
app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    // Add workflow execution ID to response headers if available
    if (context.Request.Headers.ContainsKey("X-Workflow-Execution-Id"))
    {
        context.Response.Headers.Add("X-Workflow-Execution-Id", 
            context.Request.Headers["X-Workflow-Execution-Id"].ToString());
    }
    
    await next();
    
    stopwatch.Stop();
    context.Response.Headers.Add("X-Processing-Time", stopwatch.ElapsedMilliseconds.ToString());
});

// Map OrchestrationService endpoints using Minimal APIs
app.MapOrchestrationServiceEndpoints();

// Add health check endpoint
app.MapHealthChecks("/health");

// Add metrics endpoint for monitoring
app.MapGet("/metrics", async (IMetricsLogger metricsLogger) =>
{
    var metrics = new Dictionary<string, object>
    {
        ["service"] = "OrchestrationService",
        ["version"] = "1.0.0",
        ["timestamp"] = DateTime.UtcNow,
        ["uptime_ms"] = Environment.TickCount64,
        ["performance_targets"] = new Dictionary<string, string>
        {
            ["payment_workflow"] = "≤5000ms",
            ["user_onboarding"] = "≤10000ms",
            ["treasury_operations"] = "≤15000ms",
            ["status_queries"] = "≤100ms"
        },
        ["workflow_types"] = new[]
        {
            "payment-processing-zero-trust",
            "user-onboarding-optimized", 
            "treasury-operations-secure"
        },
        ["integrated_services"] = new[]
        {
            "SignatureService",
            "PaymentGatewayService",
            "QuantumLedger.Hub",
            "UserService",
            "AccountService",
            "TreasuryService",
            "NotificationService",
            "IdentityVerificationService"
        },
        ["event_integration"] = new Dictionary<string, object>
        {
            ["sns_topics"] = new[]
            {
                "workflow-events",
                "workflow-status",
                "workflow-errors",
                "admin-alerts"
            },
            ["event_triggers"] = new[]
            {
                "payment_requested",
                "user_registration",
                "treasury_operation_requested"
            }
        }
    };
    
    return Results.Ok(metrics);
})
.WithName("GetMetrics")
.WithSummary("Get service metrics and performance targets")
.WithTags("Monitoring");

// Add workflow status endpoint for real-time monitoring
app.MapGet("/status", async () =>
{
    var status = new
    {
        Service = "OrchestrationService",
        Status = "Running",
        Timestamp = DateTime.UtcNow,
        Features = new[]
        {
            "Workflow Orchestration",
            "Zero-Trust Integration", 
            "Event-Driven Triggers",
            "Real-Time Status Tracking",
            "SNS/SQS Publishing",
            "Multi-Service Coordination"
        },
        Architecture = new
        {
            Pattern = "Event-Driven Workflow Orchestration",
            Security = "Zero-Trust with SignatureService Integration",
            Performance = "Fail-Fast with ≤5 Second Payment Workflows",
            Integration = "Aspire Service Discovery"
        }
    };
    
    return Results.Ok(status);
})
.WithName("GetStatus")
.WithSummary("Get service status and architecture information")
.WithTags("Monitoring");

Log.Logger.Information("OrchestrationService starting up - Workflow orchestration with zero-trust security");
Log.Logger.Information("Integrated services: SignatureService, PaymentGatewayService, QuantumLedger.Hub, and 5 additional services");
Log.Logger.Information("Performance targets: ≤5s payment workflows, ≤10s user onboarding, ≤15s treasury operations");

app.Run();
