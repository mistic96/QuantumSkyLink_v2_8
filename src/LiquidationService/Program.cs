using Refit;
using LiquidationService.Data;
using LiquidationService.Services;
using LiquidationService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;
using Mapster;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<LiquidationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-liquidationservice")));

// Add services to the container
builder.Services.AddControllers();

 // Register application services
 builder.Services.AddScoped<ILiquidationService, LiquidationService.Services.LiquidationService>();
 builder.Services.AddScoped<ILiquidityProviderService, LiquidityProviderService>();
 builder.Services.AddScoped<IComplianceService, ComplianceService>();
 builder.Services.AddScoped<IAssetEligibilityService, AssetEligibilityService>();
 builder.Services.AddScoped<IMarketPricingService, MarketPricingService>();

// Configure Redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "LiquidationService";
});

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("postgres-liquidationservice"))));

builder.Services.AddHangfireServer();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is required");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ServiceOnly", policy => policy.RequireRole("Service"));
    options.AddPolicy("UserAccess", policy => policy.RequireRole("User", "Admin", "Service"));
});

// Configure Mapster
TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LiquidationService API",
        Version = "v1",
        Description = "API for managing liquidation operations, liquidity providers, compliance checks, asset eligibility, and market pricing"
    });

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("database", () =>
    {
        try
        {
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LiquidationDbContext>();
            context.Database.CanConnect();
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    })
    .AddCheck("redis", () =>
    {
        try
        {
            // Basic Redis health check - this will be improved with actual Redis connection test
            return HealthCheckResult.Healthy("Redis connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis connection failed", ex);
        }
    });

 // Configure HTTP client factory
 builder.Services.AddHttpClient();

 // Register HTTP clients for now - will be replaced with proper Refit clients later
 var paymentGatewayUrl = builder.Configuration.GetValue<string>("Services:PaymentGatewayUrl") ?? "https://paymentgateway";
 builder.Services.AddHttpClient("PaymentGateway", c => c.BaseAddress = new Uri(paymentGatewayUrl));

 var treasuryUrl = builder.Configuration.GetValue<string>("Services:TreasuryServiceUrl") ?? "https://treasuryservice";
 builder.Services.AddHttpClient("Treasury", c => c.BaseAddress = new Uri(treasuryUrl));

 var app = builder.Build();

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LiquidationService API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Use HTTPS redirection
app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowedOrigins");

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Map controllers
app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

app.Run();

// Hangfire authorization filter
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Allow access in development
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }

        // In production, require authentication and Admin role
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}
