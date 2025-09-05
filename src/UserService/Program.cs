using UserService.Data;
using UserService.Services;
using UserService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Refit;
using UserService.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add HTTP context accessor for security audit service
builder.Services.AddHttpContextAccessor();

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-userservice")));

// Register business services only - focus on domain logic
builder.Services.AddScoped<IUserService, UserService.Services.UserService>();
builder.Services.AddScoped<ILogtoUserService, LogtoUserService>();
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();
builder.Services.AddScoped<IWalletManagementService, WalletManagementService>();

// Service clients for inter-service communication
builder.Services.AddHttpClient<IInfrastructureServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://infrastructureservice");
});

 // Add Logto API client using Refit
builder.Services.AddTransient<LogtoUserService>(); // ensure DI for user service

// Register authentication and the Logto handler as a scheme so framework provides UrlEncoder, ISystemClock, etc.
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, LogtoAuthHandler>("Logto", options => { });

// Ensure authorization services are registered for UseAuthorization()
builder.Services.AddAuthorization();

builder.Services.AddRefitClient<ILogtoApiClient>()
    .ConfigureHttpClient(client =>
    {
        var logtoBaseUrl = builder.Configuration["Logto:BaseUrl"] ?? "https://your-logto-domain.logto.app";
        client.BaseAddress = new Uri(logtoBaseUrl);
        
        // Add Management API token for authentication (optional - handler will fetch token if empty)
        var managementApiToken = builder.Configuration["Logto:ManagementApiToken"];
        if (!string.IsNullOrEmpty(managementApiToken))
        {
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managementApiToken);
        }
    });

// Register internal ComplianceService client (Refit) for KYC status checks
builder.Services.AddRefitClient<UserService.Clients.IComplianceInternalClient>()
    .ConfigureHttpClient(client =>
    {
        // Use Aspire service discovery name for base address
        client.BaseAddress = new Uri("https+http://complianceservice");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

// AccountService HTTP client for post-registration account creation
builder.Services.AddHttpClient("AccountService", client =>
{
    client.BaseAddress = new Uri("https+http://accountservice");
    client.Timeout = TimeSpan.FromSeconds(10);
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
