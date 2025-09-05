using FeeService.Data;
using FeeService.Services;
using FeeService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<FeeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-feeservice")));

// Register business services only - focus on domain logic
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IFeeCalculationService, FeeCalculationService>();
builder.Services.AddScoped<IFeeCollectionService, FeeCollectionService>();
builder.Services.AddScoped<IFeeDistributionService, FeeDistributionService>();
builder.Services.AddScoped<IExternalRateProviderService, ExternalRateProviderService>();

// Configure external API settings for business logic
builder.Services.Configure<ExternalApiSettings>(
    builder.Configuration.GetSection("ExternalApis"));

// Add Redis distributed cache (Aspire will resolve connection string)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache");
    options.InstanceName = "FeeService";
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
