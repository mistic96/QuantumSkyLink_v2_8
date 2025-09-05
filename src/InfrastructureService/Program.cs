using InfrastructureService.Data;
using InfrastructureService.Services;
using InfrastructureService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<InfrastructureDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-infrastructureservice")));

// Register business services only - focus on domain logic
builder.Services.AddScoped<IInfrastructureService, InfrastructureService.Services.InfrastructureService>();
builder.Services.AddScoped<IBlockchainService, BlockchainService>();
builder.Services.AddScoped<IServiceRegistrationService, ServiceRegistrationService>();
builder.Services.AddScoped<IBlockchainAddressService, BlockchainAddressService>();
builder.Services.AddScoped<ISignatureValidationService, SignatureValidationService>();
builder.Services.AddScoped<IVolumeTestingService, VolumeTestingService>();
builder.Services.AddScoped<IMultiNetworkService, MultiNetworkService>();

// Service clients for inter-service communication
builder.Services.AddHttpClient<IUserServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://userservice");
});

builder.Services.AddHttpClient<INotificationServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://notificationservice");
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
