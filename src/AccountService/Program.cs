using AccountService.Data;
using AccountService.Services;
using AccountService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-accountservice")));

// Register business services only - focus on domain logic
builder.Services.AddScoped<IAccountService, AccountService.Services.AccountService>();
builder.Services.AddScoped<IAccountTransactionService, AccountTransactionService>();
builder.Services.AddScoped<IAccountVerificationService, AccountVerificationService>();
builder.Services.AddScoped<IAccountLimitService, AccountLimitService>();

// Service clients for inter-service communication
builder.Services.AddHttpClient<IUserServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://userservice");
});

builder.Services.AddHttpClient<IInfrastructureServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://infrastructureservice");
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
