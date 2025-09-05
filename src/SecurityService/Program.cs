using SecurityService.Data;
using SecurityService.Services;
using SecurityService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<SecurityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-securityservice")));

// Register business services only - focus on domain logic
builder.Services.AddScoped<ISecurityService, SecurityService.Services.SecurityService>();

// Service clients for inter-service communication
builder.Services.AddHttpClient<IAccountServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://accountservice");
});

builder.Services.AddHttpClient<IUserServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://userservice");
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
