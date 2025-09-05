using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MarketplaceService.Data;
using Mapster;
using MarketplaceService.Services;
using MarketplaceService.Services.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Refit;
using MarketplaceService.Services.Clients;
using MarketplaceService.Services.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-marketplaceservice")));

// Add Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Add Hangfire for background job processing
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("postgres-marketplaceservice")));

builder.Services.AddHangfireServer();

// Add Mapster for object mapping
builder.Services.AddMapster();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

builder.Services.AddAuthorization();

// Register core marketplace services
builder.Services.AddScoped<IMarketplaceService, MarketplaceService.Services.MarketplaceService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IEscrowService, EscrowService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IMarketAnalyticsService, MarketAnalyticsService>();
builder.Services.AddScoped<IMarketplaceIntegrationService, MarketplaceIntegrationService>();

// Checkout service - orchestrates primary market purchase flow (simple green-path)
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

// Register advanced pricing services
builder.Services.AddScoped<IAdvancedPricingService, AdvancedPricingService>();
builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddHttpClient<MarketDataService>();

// Register background job services
builder.Services.AddScoped<MarketplaceService.Services.BackgroundJobs.PricingBackgroundJobs>();

// Add Refit clients for external service integration
builder.Services.AddRefitClient<MarketplaceService.Services.Clients.ITokenServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://tokenservice"));

builder.Services.AddRefitClient<MarketplaceService.Services.Clients.IPaymentGatewayServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://paymentgatewayservice"));

builder.Services.AddRefitClient<MarketplaceService.Services.Clients.IFeeServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://feeservice"));

builder.Services.AddRefitClient<MarketplaceService.Services.Clients.IUserServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://userservice"));

builder.Services.AddRefitClient<MarketplaceService.Services.Clients.INotificationServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://notificationservice"));

builder.Services.AddRefitClient<MarketplaceService.Services.Clients.IComplianceServiceClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://complianceservice"));

 // Add controllers and API documentation
 builder.Services.AddControllers();
 builder.Services.AddOpenApi();

 // Configure CORS to allow CDN origin (CloudFront) when provided, otherwise default to permissive for dev.
 // Use configuration key "CloudFront:CdnDomain" to supply the domain (e.g., https://d1234.cloudfront.net).
 builder.Services.AddCors(options =>
 {
     options.AddPolicy("MarketplaceCorsPolicy", policy =>
     {
         var cdn = builder.Configuration["CloudFront:CdnDomain"];
         if (!string.IsNullOrWhiteSpace(cdn))
         {
             policy.WithOrigins(cdn)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
         }
         else
         {
             // Development fallback - permissive, will be locked down after CDN is available
             policy.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
         }
     });
 });
 

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("postgres-marketplaceservice") ?? "")
    .AddRedis(builder.Configuration.GetConnectionString("cache") ?? "localhost:6379");

var app = builder.Build();

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();

// Apply CORS policy (locks to CDN origin when configured)
app.UseCors("MarketplaceCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
    context.Database.EnsureCreated();
}

// Schedule pricing background jobs
app.Services.SchedulePricingJobs();

app.Run();
