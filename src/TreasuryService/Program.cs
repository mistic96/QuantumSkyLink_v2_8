using TreasuryService.Data;
using TreasuryService.Services;
using TreasuryService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Redis distributed cache (required for cache services)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache");
});

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<TreasuryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-treasuryservice")));

// Register business services only - focus on domain logic
builder.Services.AddScoped<ITreasuryService, TreasuryService.Services.TreasuryService>();
builder.Services.AddScoped<ITreasuryAccountService, TreasuryAccountService>();
builder.Services.AddScoped<ITreasuryTransactionService, TreasuryTransactionService>();
builder.Services.AddScoped<ITreasuryBalanceService, TreasuryBalanceService>();
builder.Services.AddScoped<ITreasuryAnalyticsService, TreasuryAnalyticsService>();

// Service clients for inter-service communication
builder.Services.AddRefitClient<IAccountServiceClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https+http://accountservice");
    });

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

builder.Services.AddRefitClient<ITokenServiceClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https+http://tokenservice");
    });

// Add controllers
builder.Services.AddControllers();

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
