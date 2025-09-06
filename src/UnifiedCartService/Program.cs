using UnifiedCartService.Repository;
using UnifiedCartService.Services;
using SurrealDb.Net;
using LiquidStorageCloud.DataManagement.Core.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SurrealDB connection
builder.Services.AddSingleton<ISurrealDbClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = configuration["SURREALDB_URL"] ?? "http://surrealdb:8000";
    var ns = configuration["SURREALDB_NS"] ?? "quantumskylink";
    var db = configuration["SURREALDB_DB"] ?? "carts";
    var user = configuration["SURREALDB_USER"] ?? "root";
    var pass = configuration["SURREALDB_PASS"] ?? "surrealpass";
    
    var client = new SurrealDbClient(endpoint);
    client.SignIn(new { user, pass }).GetAwaiter().GetResult();
    client.Use(ns, db).GetAwaiter().GetResult();
    
    return client;
});

// Register repositories
builder.Services.AddScoped<IUnifiedCartRepository, UnifiedCartRepository>();

// Register services
builder.Services.AddScoped<IUnifiedCartService, UnifiedCartService.Services.UnifiedCartService>();
builder.Services.AddScoped<ICheckoutOrchestrator, CheckoutOrchestrator>();

// Add HTTP clients for service communication using Aspire service discovery
builder.Services.AddHttpClient("MarketplaceService", client =>
{
    client.BaseAddress = new Uri("https+http://marketplaceservice");
});

builder.Services.AddHttpClient("PaymentGatewayService", client =>
{
    client.BaseAddress = new Uri("https+http://paymentgatewayservice");
});

builder.Services.AddHttpClient("OrderService", client =>
{
    client.BaseAddress = new Uri("https+http://orderservice");
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Map Aspire default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Initialize SurrealDB schema on startup
using (var scope = app.Services.CreateScope())
{
    var surrealClient = scope.ServiceProvider.GetRequiredService<ISurrealDbClient>();
    
    // Define tables and indexes
    await surrealClient.RawQuery(@"
        DEFINE TABLE unified_carts SCHEMALESS;
        DEFINE FIELD user_id ON unified_carts TYPE string;
        DEFINE FIELD status ON unified_carts TYPE string;
        DEFINE FIELD expires_at ON unified_carts TYPE datetime;
        DEFINE FIELD created_at ON unified_carts TYPE datetime;
        DEFINE FIELD updated_at ON unified_carts TYPE datetime;
        DEFINE INDEX user_active_carts ON unified_carts COLUMNS user_id, status;
        DEFINE INDEX cart_expiry ON unified_carts COLUMNS expires_at;
        
        DEFINE TABLE cart_items SCHEMALESS;
        DEFINE FIELD cart_id ON cart_items TYPE string;
        DEFINE FIELD listing_id ON cart_items TYPE string;
        DEFINE FIELD market_type ON cart_items TYPE string;
        DEFINE FIELD quantity ON cart_items TYPE number;
        DEFINE FIELD price_per_unit ON cart_items TYPE number;
        DEFINE INDEX cart_items_by_cart ON cart_items COLUMNS cart_id;
    ");
}

app.Run();