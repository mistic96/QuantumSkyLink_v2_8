var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add controllers for API Gateway functionality
builder.Services.AddControllers();

// Configure CORS for public API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("PublicApiPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("WebGateway:Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-API-Key", "X-Client-Version")
              .AllowCredentials();
    });
});

// Configure HTTP clients for service communication
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
    client.BaseAddress = new Uri("https+http://userservice");
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

var app = builder.Build();

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Apply CORS policy for public API access
app.UseCors("PublicApiPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Map controllers for gateway routing
app.MapControllers();

// Public API health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Service = "WebAPIGateway",
    Timestamp = DateTime.UtcNow,
    Version = "v1.0",
    Environment = app.Environment.EnvironmentName
}));

// API version endpoint
app.MapGet("/api/version", () => Results.Ok(new {
    Version = "1.0.0",
    ApiVersion = "v1",
    Service = "QuantumSkyLink Web API Gateway",
    SupportedVersions = new[] { "v1" }
}));

app.Run();
