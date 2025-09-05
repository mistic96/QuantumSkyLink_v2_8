using Refit;
using PaymentGatewayService.Clients;
using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Data;
using Mapster;
using PaymentGatewayService.Services;
using PaymentGatewayService.Services.Interfaces;
using PaymentGatewayService.Services.Integrations;
using PaymentGatewayService.Utils;
using Hangfire;
using Hangfire.PostgreSql;
using PaymentGatewayService.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks, JWT, Redis, etc.
builder.AddServiceDefaults();

// Bind Square configuration
builder.Services.Configure<SquareConfiguration>(builder.Configuration.GetSection("Square"));
// Square SDK services
builder.Services.AddScoped<PaymentGatewayService.Services.Integrations.ISquareService, PaymentGatewayService.Services.Integrations.SquareService>();
builder.Services.AddScoped<PaymentGatewayService.Services.SquareWebhookService>();

// Add Redis distributed cache (required for cache services)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache");
});

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-paymentgatewayservice")));

// Add Hangfire for background job processing
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("postgres-paymentgatewayservice")));

builder.Services.AddHangfireServer();

// Add Mapster for object mapping
builder.Services.AddMapster();

 // Register main payment services
 builder.Services.AddScoped<IPaymentService, PaymentService>();
 builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService.Services.PaymentGatewayService>();
 builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
 
 // Payment router and wallet stub for unified payment methods (task_1754874834371)
 builder.Services.AddScoped<PaymentGatewayService.Services.Interfaces.IPaymentRouter, PaymentGatewayService.Services.PaymentRouter>();
 // Temporary: register concrete WalletBalanceService until interface IWalletBalanceService is available.
 // This avoids a missing-interface build error while we finish the Guid/enum migration.
 builder.Services.AddScoped<PaymentGatewayService.Services.WalletBalanceService>();
 builder.Services.AddScoped<PaymentGatewayService.Services.Interfaces.IWalletBalanceService, PaymentGatewayService.Services.WalletBalanceService>();

// Register specialized business logic services
builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
builder.Services.AddScoped<IPaymentValidationService, PaymentValidationService>();
builder.Services.AddScoped<IGatewayIntegrationService, GatewayIntegrationService>();
builder.Services.AddScoped<IPaymentCacheService, PaymentCacheService>();
builder.Services.AddScoped<IRefundService, RefundService>();


// Add QuantumLedger.Hub client
builder.Services.AddHttpClient<IQuantumLedgerHubClient, QuantumLedgerHubClient>(client =>
{
    client.BaseAddress = new Uri("https+http://quantumledgerhub");
});

 // Add FeeService client
 builder.Services.AddHttpClient<IFeeServiceApi, FeeServiceApi>(client =>
 {
     client.BaseAddress = new Uri("https+http://feeservice");
 });
 
 // TreasuryService Refit client (used by PaymentRouter for crypto flows)
 // Service URL can be configured via configuration key "Services:TreasuryServiceUrl"
 var treasuryUrl = builder.Configuration.GetValue<string>("Services:TreasuryServiceUrl") ?? "https://treasuryservice";
 builder.Services.AddHttpClient(nameof(ITreasuryServiceClient), client => client.BaseAddress = new Uri(treasuryUrl))
     .AddTypedClient(client => RestService.For<ITreasuryServiceClient>(client));

// Add HTTP clients for payment gateways
builder.Services.AddHttpClient("Square", client =>
{
    client.BaseAddress = new Uri("https://connect.squareup.com/");
    client.DefaultRequestHeaders.Add("Square-Version", "2023-10-18");
});

builder.Services.AddHttpClient("Stripe", client =>
{
    client.BaseAddress = new Uri("https://api.stripe.com/");
});

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri("https+http://notificationservice");
});

// Add PIX Brazil HTTP client
builder.Services.AddHttpClient<IPIXBrazilService, PIXBrazilService>(client =>
{
    var pixConfig = builder.Configuration.GetSection("PIXBrazil");
    var baseUrl = pixConfig["BaseUrl"] ?? "https://api.liquido.com/v1/payments/";
    var apiKey = pixConfig["ApiKey"] ?? string.Empty;
    var accessToken = pixConfig["AccessToken"] ?? string.Empty;
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register PIX Brazil services
builder.Services.AddScoped<IPIXBrazilService, PIXBrazilService>();
builder.Services.AddScoped<ICPFValidator, CPFValidator>();

// Add Dots.dev HTTP client
builder.Services.AddHttpClient<IDotsDevService, DotsDevService>(client =>
{
    var dotsDevConfig = builder.Configuration.GetSection("DotsDev");
    var apiKey = dotsDevConfig["ApiKey"] ?? string.Empty;
    var apiSecret = dotsDevConfig["ApiSecret"] ?? string.Empty;
    var baseUrl = dotsDevConfig["BaseUrl"] ?? "https://api.dots.dev/v2/";
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}"))
    );
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Dots.dev services
builder.Services.AddScoped<IDotsDevService, DotsDevService>();

// Add MoonPay HTTP client
builder.Services.AddHttpClient<IMoonPayService, MoonPayService>(client =>
{
    var moonPayConfig = builder.Configuration.GetSection("MoonPay");
    var baseUrl = moonPayConfig["BaseUrl"] ?? "https://api.moonpay.com/v3/";
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "PaymentGatewayService/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register MoonPay services
builder.Services.AddScoped<IMoonPayService, MoonPayService>();

// Add Coinbase HTTP client
builder.Services.AddHttpClient<ICoinbaseService, CoinbaseService>(client =>
{
    var coinbaseConfig = builder.Configuration.GetSection("Coinbase");
    var baseUrl = coinbaseConfig["BaseUrl"] ?? "https://api.coinbase.com/api/v3/brokerage/";
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "PaymentGatewayService/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Coinbase services
builder.Services.AddScoped<ICoinbaseService, CoinbaseService>();
builder.Services.AddScoped<ICoinbaseSignatureService, CoinbaseSignatureService>();
builder.Services.AddScoped<IWebSocketService, WebSocketService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception)
    {
        // Aspire handles logging
        throw;
    }
}

app.Run();
