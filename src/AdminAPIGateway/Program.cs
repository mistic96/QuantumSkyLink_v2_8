using AdminAPIGateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add controllers for API Gateway functionality
builder.Services.AddControllers();

// Register PaymentGatewayClient
builder.Services.AddHttpClient<IPaymentGatewayClient, PaymentGatewayClient>(client =>
{
    client.BaseAddress = new Uri("https+http://paymentgatewayservice");
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
app.UseAuthentication();
app.UseAuthorization();

// Map controllers for gateway routing
app.MapControllers();

// Admin-specific health check endpoint
app.MapGet("/admin/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Service = "AdminAPIGateway",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
}));

app.Run();
