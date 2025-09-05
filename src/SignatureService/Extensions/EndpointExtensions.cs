using SignatureService.Endpoints;

namespace SignatureService.Extensions;

/// <summary>
/// Extension methods for registering SignatureService endpoints
/// Follows the same pattern as QuantumLedger's endpoint registration
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all SignatureService endpoints to the application
    /// </summary>
    public static void MapSignatureServiceEndpoints(this IEndpointRouteBuilder app)
    {
        // Map core signature validation endpoints
        app.MapSignatureEndpoints();
        
        // Map multisig test generation endpoints (internal)
        app.MapMultisigEndpoints();

        // Map additional endpoint groups as they are created
        // app.MapKeyManagementEndpoints();
        // app.MapAuditEndpoints();
        // app.MapMonitoringEndpoints();
    }

    /// <summary>
    /// Adds SignatureService API documentation
    /// </summary>
    public static IServiceCollection AddSignatureServiceApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "SignatureService API",
                Version = "v1",
                Description = "Independent zero-trust signature validation service for QuantumSkyLink v2 and QuantumLedger",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "QuantumSkyLink Team",
                    Email = "support@quantumskylink.com"
                }
            });

            // Add security definitions for future authentication
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Include XML comments for better documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Add examples for better API documentation
            // options.EnableAnnotations(); // Removed - not available in this version
        });

        return services;
    }

    /// <summary>
    /// Configures CORS for SignatureService
    /// </summary>
    public static IServiceCollection AddSignatureServiceCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(
                    "https://api.quantumskylink.com",      // QuantumSkyLink v2
                    "https://ledger.quantumskylink.com",   // QuantumLedger
                    "http://localhost:3000",               // Development
                    "http://localhost:5000",               // Development
                    "http://localhost:8080"                // Development
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("X-Validation-Id", "X-Processing-Time");
            });
        });

        return services;
    }

    /// <summary>
    /// Adds performance monitoring and metrics
    /// </summary>
    public static IServiceCollection AddSignatureServiceMetrics(this IServiceCollection services)
    {
        // Add metrics collection for performance monitoring
        services.AddSingleton<IMetricsLogger, MetricsLogger>();
        
        return services;
    }
}

/// <summary>
/// Simple metrics logger interface for performance tracking
/// </summary>
public interface IMetricsLogger
{
    void RecordSignatureValidation(TimeSpan duration, bool success, string algorithm);
    void RecordNonceValidation(TimeSpan duration, bool success);
    void RecordCacheHit(string cacheType, bool hit);
}

/// <summary>
/// Basic metrics logger implementation
/// </summary>
public class MetricsLogger : IMetricsLogger
{
    private readonly ILogger<MetricsLogger> _logger;

    public MetricsLogger(ILogger<MetricsLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void RecordSignatureValidation(TimeSpan duration, bool success, string algorithm)
    {
        _logger.LogInformation("SignatureValidation: Duration={DurationMs}ms, Success={Success}, Algorithm={Algorithm}", 
            duration.TotalMilliseconds, success, algorithm);
    }

    public void RecordNonceValidation(TimeSpan duration, bool success)
    {
        _logger.LogInformation("NonceValidation: Duration={DurationMs}ms, Success={Success}", 
            duration.TotalMilliseconds, success);
    }

    public void RecordCacheHit(string cacheType, bool hit)
    {
        _logger.LogDebug("CacheOperation: Type={CacheType}, Hit={Hit}", cacheType, hit);
    }
}
