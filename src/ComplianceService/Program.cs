using ComplianceService.Data;
using ComplianceService.Services;
using ComplianceService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<ComplianceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-complianceservice")));

// Service clients will be registered via Aspire service discovery
// These would be configured as HTTP clients pointing to other services
builder.Services.AddHttpClient<IUserServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://userservice");
});

builder.Services.AddHttpClient<IAccountServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://accountservice");
});

builder.Services.AddHttpClient<ISecurityServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://securityservice");
});

builder.Services.AddHttpClient<INotificationServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://notificationservice");
});

// Register ComplyCube API client using Refit
builder.Services.AddRefitClient<IComplyCubeService>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("ComplyCubeApiUrl") ?? "https://api.complycube.com/v1");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["ComplyCube:ApiKey"]}");
    });

// Register business services only - focus on domain logic
builder.Services.AddScoped<IComplianceService, ComplianceService.Services.ComplianceService>();


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
