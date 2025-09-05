using GovernanceService.Data;
using GovernanceService.Services;
using GovernanceService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<GovernanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-governanceservice")));

// Register business services only - focus on domain logic
builder.Services.AddScoped<IGovernanceService, GovernanceService.Services.GovernanceService>();
builder.Services.AddScoped<IGovernanceVotingService, GovernanceVotingService>();
builder.Services.AddScoped<IGovernanceValidationService, GovernanceValidationService>();
builder.Services.AddScoped<IGovernanceExecutionService, GovernanceExecutionService>();
builder.Services.AddScoped<IGovernanceAnalyticsService, GovernanceAnalyticsService>();
builder.Services.AddScoped<IGovernanceBackgroundService, GovernanceBackgroundService>();


builder.Services.AddMemoryCache();
// Service clients for inter-service communication
builder.Services.AddHttpClient<ITokenServiceClient>(client =>
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
