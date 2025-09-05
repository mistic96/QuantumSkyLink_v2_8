using TokenService.Data;
using TokenService.Services;
using TokenService.Services.Interfaces;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Refit;
 
using QuantumSkyLink.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<TokenDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-tokenservice")));

 // Register business services only - focus on domain logic
 builder.Services.AddScoped<ITokenService, TokenServiceImplementation>();
 builder.Services.AddScoped<IQuantumLedgerSignatureService, QuantumLedgerSignatureService>();
 builder.Services.AddScoped<IAssetVerificationService, AssetVerificationService>();
 builder.Services.AddScoped<IAiComplianceService, AiComplianceService>();

 // Token lifecycle persistence (uses shared LedgerContext for cross-service token data)
 // NOTE: TokenPersistenceService implementation is not present in this branch; registration removed to allow compilation.
 
 // Register LedgerContext so TokenPersistenceService can use the shared ledger database
 builder.Services.AddDbContext<QuantumLedger.Data.LedgerContext>(options =>
     options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-ledger")));

 // Service clients for inter-service communication (keep only clients that exist)
 builder.Services.AddRefitClient<IQuantumLedgerClient>()
     .ConfigureHttpClient(client =>
     {
         client.BaseAddress = new Uri("https+http://quantumledger-hub");
     });

 // Keep user & notification clients which are defined
 builder.Services.AddRefitClient<IUserServiceClient>()
     .ConfigureHttpClient(client =>
     {
         client.BaseAddress = new Uri("https+http://userservice");
     });

 builder.Services.AddRefitClient<INotificationServiceClient>()
     .ConfigureHttpClient(client =>
     {
         client.BaseAddress = new Uri("https+http://notificationservice");
     });

 // Register LedgerContext so TokenPersistenceService can use the shared ledger database
 builder.Services.AddDbContext<QuantumLedger.Data.LedgerContext>(options =>
     options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-ledger")));

 // Add controllers
 builder.Services.AddControllers();

 // Note: Template compilation, custom signers, S3 storage wrappers and orchestration services were commented out
 // because the implementation files are not present in this branch. Reintroduce them when implementations exist.

var app = builder.Build();

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
