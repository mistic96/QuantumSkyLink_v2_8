using Microsoft.EntityFrameworkCore;
using Serilog;
using SignatureService.Extensions;
using SignatureService.Services;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Blockchain.Services.MultiChain;
using QuantumLedger.Cryptography.PQC.Providers;
using QuantumLedger.Cryptography.Models;
using LiquidStorageCloud.DataManagement.Core.Extensions;
using LiquidStorageCloud.DataManagement.Core.Repository;
using QuantumLedger.Cryptography.Storage;
using SurrealDb.Net;
using QuantumLedger.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (same pattern as QuantumLedger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/signature-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Aspire service defaults (same as QuantumSkyLink v2 and QuantumLedger)
builder.AddServiceDefaults();

// Add database contexts for audit logging and nonce tracking
builder.Services.AddDbContext<SignatureAuditContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-signatureservice")));

// Add LedgerContext for multisig persistence
builder.Services.AddDbContext<LedgerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-ledger")));

// Add Redis distributed cache for high-performance nonce tracking and caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache");
    options.InstanceName = "SignatureService";
});

// Add SurrealDB for hybrid key storage and CQRS operations
builder.Services.AddDataManagementCore(builder.Configuration, options =>
{
    options.EnableEventPublishing = true;
});

// Register KeyEntity as a transient for SurrealRepository generic type resolution
// Note: Entity models need to be registered for generic repository patterns
builder.Services.AddTransient<KeyEntity>();

// Register S3 key storage service for hybrid storage
builder.Services.AddScoped<KeyStorageS3Service>();
builder.Services.AddScoped<ObjectStorageS3Service>();
builder.Services.AddScoped<SignatureService.Services.MultisigStorageService>();
builder.Services.AddScoped<SignatureService.Services.MultisigPersistenceService>();

// Register hybrid key storage service
builder.Services.AddScoped<HybridKeyStorage>();

// Add SignatureService API documentation and Minimal APIs support
builder.Services.AddSignatureServiceApiDocumentation();

// Configure CORS for cross-system communication
builder.Services.AddSignatureServiceCors();

// Add performance monitoring and metrics
builder.Services.AddSignatureServiceMetrics();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Register QuantumLedger's cryptographic providers (reuse existing infrastructure)
Log.Logger.Information("Initializing QuantumLedger cryptographic infrastructure for SignatureService");

// Register post-quantum signature providers from QuantumLedger
builder.Services.AddSingleton<ISignatureProvider>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DilithiumProvider>>();
    return new DilithiumProvider(3, logger); // NIST Level 3 for high security
});

builder.Services.AddSingleton<ISignatureProvider>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FalconProvider>>();
    return new FalconProvider(9, logger); // Height 9 (recommended for performance/security balance)
});

// Register traditional signature providers (placeholder for EC256)
// Note: In a full implementation, you would add EC256Provider from QuantumLedger
// builder.Services.AddSingleton<ISignatureProvider, EC256Provider>();

// Register QuantumLedger's key management system
// Note: This would require implementing IKeyManager or using QuantumLedger's implementation
// For now, we'll create a placeholder that can be replaced with actual QL key manager
builder.Services.AddScoped<IKeyManager, PlaceholderKeyManager>();

// Register QuantumLedger's multi-cloud key vault providers (reuse existing infrastructure)
Log.Logger.Information("Initializing Multi-Cloud Key Vault System for SignatureService");

// Note: These would be imported from QuantumLedger.Cryptography.Providers
// builder.Services.AddSingleton<GoogleCloudKmsProvider>();
// builder.Services.AddSingleton<AzureKeyVaultProvider>();
// builder.Services.AddSingleton<AwsKmsProvider>();
// builder.Services.AddSingleton<ICloudKeyVaultFactory, CloudKeyVaultFactory>();

 // Register SignatureService-specific services
builder.Services.AddScoped<SignatureValidationService>();
builder.Services.AddScoped<NonceTrackingService>();

// Register MultiChain multisig test generator (internal, no persistence)
builder.Services.AddHttpClient<QuantumLedger.Blockchain.Services.MultiChainMultisigService>();
builder.Services.AddScoped<QuantumLedger.Blockchain.Services.MultiChainMultisigService>();

// Register audit logging service for security compliance
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditLoggingService, AuditLoggingService>();

// Configure performance optimization services
builder.Services.AddMemoryCache();

// Configure caching services for high performance
builder.Services.Configure<Microsoft.Extensions.Caching.Memory.MemoryCacheOptions>(options =>
{
    options.SizeLimit = 1000; // Limit cache size
});

// Configure connection pooling for optimal performance
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });
});

// Configure AWS services for cryptographic key storage
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
builder.Services.AddAWSService<Amazon.KeyManagementService.IAmazonKeyManagementService>();

var app = builder.Build();

// Map Aspire default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SignatureService API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Add custom middleware for performance headers
app.Use(async (context, next) =>
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    await next();
    
    stopwatch.Stop();
    context.Response.Headers.Add("X-Processing-Time", stopwatch.ElapsedMilliseconds.ToString());
});

// Map SignatureService endpoints using Minimal APIs
app.MapSignatureServiceEndpoints();

// Add health check endpoint
app.MapHealthChecks("/health");

// Add metrics endpoint for monitoring
app.MapGet("/metrics", async (IMetricsLogger metricsLogger) =>
{
    var metrics = new Dictionary<string, object>
    {
        ["service"] = "SignatureService",
        ["version"] = "1.0.0",
        ["timestamp"] = DateTime.UtcNow,
        ["uptime_ms"] = Environment.TickCount64,
        ["performance_targets"] = new Dictionary<string, string>
        {
            ["signature_validation"] = "≤1000ms",
            ["nonce_validation"] = "≤200ms",
            ["dual_signature_validation"] = "≤1000ms"
        }
    };
    
    return Results.Ok(metrics);
})
.WithName("GetMetrics")
.WithSummary("Get service metrics")
.WithTags("Monitoring");

Log.Logger.Information("SignatureService starting up - Independent zero-trust signature validation");
Log.Logger.Information("Leveraging QuantumLedger cryptographic infrastructure for world-class performance");

app.Run();

// Placeholder implementations for services that would be imported from QuantumLedger

/// <summary>
/// Placeholder key manager - would be replaced with QuantumLedger's IKeyManager implementation
/// </summary>
public class PlaceholderKeyManager : IKeyManager
{
    private readonly ILogger<PlaceholderKeyManager> _logger;

    public PlaceholderKeyManager(ILogger<PlaceholderKeyManager> logger)
    {
        _logger = logger;
    }

    public Task<string> GenerateKeyPairAsync(string address, string algorithm, KeyCategory category, int? version = null)
    {
        _logger.LogWarning("Using placeholder key manager - replace with QuantumLedger implementation");
        return Task.FromResult($"placeholder-key-{Guid.NewGuid()}");
    }

    public Task<byte[]> GetPublicKeyAsync(string keyId)
    {
        _logger.LogWarning("Using placeholder key manager - replace with QuantumLedger implementation");
        // Return a placeholder public key for testing
        return Task.FromResult(new byte[32]); // Placeholder 32-byte key
    }

    public Task<byte[]> GetPrivateKeyAsync(string keyId)
    {
        _logger.LogWarning("Using placeholder key manager - replace with QuantumLedger implementation");
        return Task.FromResult(new byte[64]); // Placeholder 64-byte key
    }

    public Task<string> GetAlgorithmAsync(string keyId)
    {
        return Task.FromResult("Dilithium");
    }

    public Task<int?> GetVersionAsync(string keyId)
    {
        return Task.FromResult<int?>(1);
    }

    public Task<int?> GetLatestVersionAsync(string address, KeyCategory category)
    {
        return Task.FromResult<int?>(1);
    }

    public Task<string> RotateKeysAsync(string address, KeyCategory category)
    {
        return Task.FromResult($"rotated-key-{Guid.NewGuid()}");
    }

    public Task<string?> GetLatestKeyPairAsync(string address, KeyCategory category)
    {
        return Task.FromResult<string?>($"latest-key-{address}");
    }

    public Task<IEnumerable<KeyEntity>> GetAddressKeysAsync(string address)
    {
        return Task.FromResult(Enumerable.Empty<KeyEntity>());
    }

    public Task<KeyEntity?> GetCurrentKeyAsync(string address, KeyCategory category)
    {
        return Task.FromResult<KeyEntity?>(null);
    }
}

/// <summary>
/// Placeholder audit logging service - would be replaced with QuantumLedger implementation
/// </summary>
public interface IAuditLoggingService
{
    Task LogSignatureValidationAsync(string accountId, string operation, bool success, string details);
}

public class AuditLoggingService : IAuditLoggingService
{
    private readonly ILogger<AuditLoggingService> _logger;

    public AuditLoggingService(ILogger<AuditLoggingService> logger)
    {
        _logger = logger;
    }

    public Task LogSignatureValidationAsync(string accountId, string operation, bool success, string details)
    {
        _logger.LogInformation("AUDIT: Account={AccountId}, Operation={Operation}, Success={Success}, Details={Details}",
            accountId, operation, success, details);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Placeholder signature audit context - would be replaced with actual EF context
/// </summary>
public class SignatureAuditContext : DbContext
{
    public SignatureAuditContext(DbContextOptions<SignatureAuditContext> options) : base(options)
    {
    }

    // Placeholder - would contain actual audit log entities
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure audit log entities
    }
}
