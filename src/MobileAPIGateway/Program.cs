using FluentValidation;
using FluentValidation.AspNetCore;
using MobileAPIGateway.Authentication;
using MobileAPIGateway.Extensions;
using MobileAPIGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add controllers with FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add authentication
builder.Services.AddLogtoAuthentication(builder.Configuration);

// Add user context
builder.Services.AddUserContext();

// Add service clients, services, and validators
builder.Services.AddServiceClients(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddValidators();

// Configure HTTP clients for service communication
// These can be enabled/disabled based on StandaloneMode configuration
var standaloneMode = builder.Configuration.GetValue<bool>("MobileGateway:StandaloneMode", false);

if (!standaloneMode)
{
    // Full microservices mode - connect to all backend services
    builder.Services.AddHttpClient("PaymentGatewayService", client =>
    {
        client.BaseAddress = new Uri("https+http://paymentgatewayservice");
    });

    builder.Services.AddHttpClient("NotificationService", client =>
    {
        client.BaseAddress = new Uri("https+http://notificationservice");
    });

    builder.Services.AddHttpClient("LiquidationService", client =>
    {
        client.BaseAddress = new Uri("https+http://liquidationservice");
    });

    builder.Services.AddHttpClient("FeeService", client =>
    {
        client.BaseAddress = new Uri("https+http://feeservice");
    });

    builder.Services.AddHttpClient("ComplianceService", client =>
    {
        client.BaseAddress = new Uri("https+http://complianceservice");
    });

    builder.Services.AddHttpClient("GovernanceService", client =>
    {
        client.BaseAddress = new Uri("https+http://governanceservice");
    });

    builder.Services.AddHttpClient("InfrastructureService", client =>
    {
        client.BaseAddress = new Uri("https+http://infrastructureservice");
    });

    builder.Services.AddHttpClient("TokenService", client =>
    {
        client.BaseAddress = new Uri("https+http://tokenservice");
    });

    builder.Services.AddHttpClient("UserService", client =>
    {
        var userServiceUrl = builder.Configuration["ServiceUrls:UserService"] ?? "https+http://userservice";
        client.BaseAddress = new Uri(userServiceUrl);
    });

    builder.Services.AddHttpClient("AccountService", client =>
    {
        client.BaseAddress = new Uri("https+http://accountservice");
    });

    builder.Services.AddHttpClient("SecurityService", client =>
    {
        client.BaseAddress = new Uri("https+http://securityservice");
    });

    builder.Services.AddHttpClient("TreasuryService", client =>
    {
        client.BaseAddress = new Uri("https+http://treasuryservice");
    });

    builder.Services.AddHttpClient("MarketplaceService", client =>
    {
        client.BaseAddress = new Uri("https+http://marketplaceservice");
    });

    builder.Services.AddHttpClient("AIReviewService", client =>
    {
        client.BaseAddress = new Uri("https+http://aireviewservice");
    });

    builder.Services.AddHttpClient("IdentityVerificationService", client =>
    {
        client.BaseAddress = new Uri("https+http://identityverificationservice");
    });
}
else
{
    // Standalone mode - using local SQLite data instead of backend services
    // This creates a standalone mobile gateway for UAT testing
    // All service calls will be handled by local implementations
}

var app = builder.Build();

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Add error handling middleware
app.UseErrorHandling();

// Add request logging middleware
app.UseRequestLogging();

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Add user context middleware
app.UseUserContext();

// Map controllers
app.MapControllers();

// Mobile-specific health check endpoint
app.MapGet("/mobile/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Service = "MobileAPIGateway",
    Timestamp = DateTime.UtcNow,
    Version = "v1.0",
    Platform = "Mobile",
    Environment = app.Environment.EnvironmentName
}));

// Mobile API version endpoint
app.MapGet("/mobile/version", () => Results.Ok(new {
    Version = "1.0.0",
    ApiVersion = "v1",
    Service = "QuantumSkyLink Mobile API Gateway",
    SupportedVersions = new[] { "v1" },
    MobileFeatures = new[] { "OfflineSync", "PushNotifications", "Biometric", "QRCode" }
}));

app.Run();
