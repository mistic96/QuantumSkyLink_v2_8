# QuantumSkyLink v2: Aspire + Neon PostgreSQL Implementation Guide

## Overview

This document provides comprehensive implementation patterns for building microservices using .NET Aspire infrastructure-first approach with Neon PostgreSQL SaaS. These patterns are proven in production across 24 microservices in the QuantumSkyLink v2 financial platform.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Aspire Infrastructure Setup](#aspire-infrastructure-setup)
3. [Service Implementation Patterns](#service-implementation-patterns)
4. [Entity Framework Patterns](#entity-framework-patterns)
5. [Neon PostgreSQL Integration](#neon-postgresql-integration)
6. [Service Discovery & Communication](#service-discovery--communication)
7. [Migration & Deployment](#migration--deployment)
8. [Best Practices & Conventions](#best-practices--conventions)
9. [Troubleshooting Guide](#troubleshooting-guide)

---

## Architecture Overview

### Database-per-Service Architecture

QuantumSkyLink v2 implements a **Database-per-Service** pattern where each microservice has its own dedicated PostgreSQL database hosted on Neon SaaS. This provides:

- **Complete Data Isolation**: Each service owns its data completely
- **Independent Scaling**: Services can scale databases independently
- **Technology Flexibility**: Each service can optimize its database schema
- **Fault Isolation**: Database issues in one service don't affect others

### Service Categories

```
📊 Business Services (15): UserService, AccountService, PaymentGatewayService, etc.
🏗️ Infrastructure (3): AppHost, ServiceDefaults, Shared
🔐 Critical Services (2): SignatureService, OrchestrationService
🌐 API Gateways (3): WebAPIGateway, AdminAPIGateway, MobileAPIGateway
🧪 Supporting (3): RefitClient, QuantumLedger.Hub, UserService.Tests
```

### Aspire Infrastructure-First Approach

- **Service Discovery**: Automatic service-to-service communication
- **Observability**: Built-in OpenTelemetry, metrics, and health checks
- **Configuration Management**: Centralized connection strings and settings
- **Resilience**: Automatic retry policies and circuit breakers

---

## Aspire Infrastructure Setup

### 1. AppHost Configuration (AppHost.cs)

The AppHost serves as the orchestration center for all services and infrastructure:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure Services
var redis = builder.AddRedis("cache");
var rabbitmq = builder.AddRabbitMQ("messaging");

// Neon PostgreSQL - Database-per-Service Architecture
// Each service gets dedicated database following quantumskylink_{service} naming
var postgresUserService = builder.AddConnectionString("postgres-userservice");
var postgresAccountService = builder.AddConnectionString("postgres-accountservice");
var postgresPaymentGatewayService = builder.AddConnectionString("postgres-paymentgatewayservice");
// ... continue for all 24 services

// External Services
var kestra = builder.AddConnectionString("kestra-external", "http://localhost:8080");
var multichain = builder.AddConnectionString("multichain-external", "http://localhost:7446");

// Service Registration with Dependencies
var userService = builder.AddProject<Projects.UserService>("userservice")
    .WithReference(postgresUserService)
    .WithReference(redis);

var accountService = builder.AddProject<Projects.AccountService>("accountservice")
    .WithReference(postgresAccountService)
    .WithReference(redis)
    .WithReference(userService); // Service dependency

// API Gateways with External Endpoints
var webApiGateway = builder.AddProject<Projects.WebAPIGateway>("webapigateway")
    .WithReference(userService)
    .WithReference(accountService)
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

### 2. Connection String Configuration (appsettings.json)

All Neon PostgreSQL connection strings are centralized in the AppHost:

```json
{
  "ConnectionStrings": {
    "postgres-userservice": "Host=ep-aged-paper-a49vo4sv.us-east-1.aws.neon.tech;Database=quantumskylink_userservice;Username=neondb_owner;Password=npg_bDNRrMgSE28p;SSL Mode=Require;Trust Server Certificate=true",
    "postgres-accountservice": "Host=ep-aged-paper-a49vo4sv.us-east-1.aws.neon.tech;Database=quantumskylink_accountservice;Username=neondb_owner;Password=npg_bDNRrMgSE28p;SSL Mode=Require;Trust Server Certificate=true",
    "postgres-paymentgatewayservice": "Host=ep-aged-paper-a49vo4sv.us-east-1.aws.neon.tech;Database=quantumskylink_paymentgateway;Username=neondb_owner;Password=npg_bDNRrMgSE28p;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 3. ServiceDefaults Configuration (Extensions.cs)

ServiceDefaults provides standardized infrastructure setup for all services:

```csharp
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();

        // Configure HTTP clients with resilience and service discovery
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        return builder;
    }
}
```

---

## Service Implementation Patterns

### 1. Minimal Program.cs Pattern

Focus on business logic registration, let Aspire handle infrastructure:

```csharp
using UserService.Data;
using UserService.Services;
using UserService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks
builder.AddServiceDefaults();

// Add Entity Framework with PostgreSQL (Aspire resolves connection string)
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres")));

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

builder.Services.AddControllers();

var app = builder.Build();

// Map Aspire default endpoints (health checks, metrics, etc.)
app.MapDefaultEndpoints();

// Configure minimal HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. What NOT to Include in Program.cs

Avoid manual infrastructure setup that Aspire handles automatically:

```csharp
// ❌ DON'T DO THIS - Aspire handles these automatically
builder.Services.AddHealthChecks();
builder.Services.AddOpenTelemetry();
builder.Services.AddServiceDiscovery();

// ❌ DON'T hardcode connection strings
builder.Services.AddDbContext<DbContext>(options =>
    options.UseNpgsql("Host=localhost;Database=mydb;..."));

// ❌ DON'T configure Redis manually
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// ❌ DON'T configure JWT manually (unless service-specific requirements)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
```

---

## Entity Framework Patterns

### 1. Heavy Annotations Approach

**Philosophy**: "Let EF do the heavy lifting; annotate classes and let EF do the rest"

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data.Entities;

[Table("Users")]
[Index(nameof(LogtoUserId), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string LogtoUserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public virtual UserProfile? Profile { get; set; }
    public virtual UserWallet? Wallet { get; set; }
    public virtual UserSecuritySettings? SecuritySettings { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
```

### 2. Minimal Fluent API Usage

Use Fluent API only when Data Annotations cannot achieve the desired result:

```csharp
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserWallet> UserWallets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Minimal Fluent API - Only for one-to-one relationships that annotations can't handle
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure TimeSpan for PostgreSQL (annotations can't handle this)
        modelBuilder.Entity<UserSecuritySettings>()
            .Property(e => e.LockoutDuration)
            .HasConversion(
                v => v.Ticks,
                v => new TimeSpan(v));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatic timestamp updates
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Added)
                    user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Neon PostgreSQL Integration

### 1. Database Naming Convention

Each service gets its own database following the pattern:

```
quantumskylink_{servicename}
```

Examples:
- `quantumskylink_userservice`
- `quantumskylink_accountservice`
- `quantumskylink_paymentgateway`
- `quantumskylink_treasury`

### 2. Connection String Pattern

All Neon connections require SSL and follow this pattern:

```
Host=ep-aged-paper-a49vo4sv.us-east-1.aws.neon.tech;
Database=quantumskylink_{service};
Username=neondb_owner;
Password={your_password};
SSL Mode=Require;
Trust Server Certificate=true
```

### 3. SSL/TLS Configuration

Neon requires SSL connections. Key parameters:
- `SSL Mode=Require`: Forces SSL connection
- `Trust Server Certificate=true`: Accepts Neon's certificate

### 4. Database Initialization

Services handle their own database initialization:

```csharp
// In Program.cs after app.Build()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("UserService database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize UserService database");
        throw;
    }
}
```

---

## Service Discovery & Communication

### 1. Inter-Service Communication Pattern

Use service names instead of hardcoded URLs:

```csharp
// ✅ CORRECT - Use service name
builder.Services.AddHttpClient<IInfrastructureServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://infrastructureservice");
});

// ❌ WRONG - Don't hardcode localhost URLs
builder.Services.AddHttpClient<IInfrastructureServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});
```

### 2. Service Reference Pattern in AppHost

Services reference each other through the AppHost:

```csharp
var userService = builder.AddProject<Projects.UserService>("userservice")
    .WithReference(postgresUserService)
    .WithReference(redis);

var accountService = builder.AddProject<Projects.AccountService>("accountservice")
    .WithReference(postgresAccountService)
    .WithReference(redis)
    .WithReference(userService); // Service dependency

var orchestrationService = builder.AddProject<Projects.OrchestrationService>("orchestrationservice")
    .WithReference(postgresOrchestrationService)
    .WithReference(redis)
    .WithReference(signatureService)
    .WithReference(paymentGatewayService)
    .WithReference(userService)
    .WithReference(accountService);
```

### 3. HTTP Client Configuration

Aspire automatically configures HTTP clients with resilience:

```csharp
// In ServiceDefaults
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Turn on resilience by default
    http.AddStandardResilienceHandler();
    
    // Turn on service discovery by default
    http.AddServiceDiscovery();
});
```

---

## Migration & Deployment

### 1. Database Migration Strategy

Each service manages its own migrations:

```bash
# Navigate to service directory
cd src/UserService

# Add migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### 2. Environment-Specific Configuration

Use different appsettings files for environments:

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "postgres": "Host=localhost;Database=quantumskylink_userservice_dev;..."
  }
}

// appsettings.Production.json
{
  "ConnectionStrings": {
    "postgres": "Host=ep-aged-paper-a49vo4sv.us-east-1.aws.neon.tech;Database=quantumskylink_userservice;..."
  }
}
```

### 3. Health Check Integration

Aspire automatically adds health checks for registered dependencies:

```csharp
// Automatically added by Aspire when you reference PostgreSQL
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnectionString);
```

---

## Best Practices & Conventions

### 1. Service Naming Conventions

- **Service Names**: Use lowercase with no spaces (e.g., `userservice`, `paymentgatewayservice`)
- **Database Names**: `quantumskylink_{servicename}`
- **Connection String Names**: `postgres-{servicename}`

### 2. Dependency Injection Patterns

Register services with appropriate lifetimes:

```csharp
// Business services - Scoped (per request)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Caching services - Scoped
builder.Services.AddScoped<IPaymentCacheService, PaymentCacheService>();

// HTTP clients - Singleton (managed by HttpClientFactory)
builder.Services.AddHttpClient<IExternalApiClient>();
```

### 3. Error Handling Pattern

Follow PaymentGatewayService pattern - direct response returns, exception-based error handling:

```csharp
[HttpPost]
public async Task<ActionResult<PaymentResponse>> ProcessPayment(PaymentRequest request)
{
    try
    {
        var result = await _paymentService.ProcessPaymentAsync(request);
        return Ok(result);
    }
    catch (ValidationException ex)
    {
        return BadRequest(ex.Message);
    }
    catch (PaymentException ex)
    {
        return StatusCode(402, ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error processing payment");
        return StatusCode(500, "Internal server error");
    }
}
```

### 4. Configuration Management

Keep appsettings.json focused on business-specific configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "BusinessSettings": {
    "MaxTransactionAmount": 10000,
    "DefaultCurrency": "USD",
    "EnableAdvancedFeatures": true
  }
}
```

### 5. Async/Await Patterns

Use CancellationToken support throughout:

```csharp
public async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
{
    return await _context.Users
        .Include(u => u.Profile)
        .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
}
```

---

## Troubleshooting Guide

### 1. Common Connection Issues

**Problem**: SSL connection failures to Neon
```
Solution: Ensure connection string includes:
- SSL Mode=Require
- Trust Server Certificate=true
```

**Problem**: Service discovery not working
```
Solution: Verify service names match between AppHost and HTTP client configuration
```

### 2. Database Issues

**Problem**: Migration failures
```bash
# Check connection string
dotnet ef dbcontext info

# Verify database exists
dotnet ef database update --verbose
```

**Problem**: Entity Framework configuration errors
```
Solution: Prefer Data Annotations over Fluent API
- Use [Required], [MaxLength], [Index] attributes
- Only use Fluent API for complex relationships
```

### 3. Service Communication Issues

**Problem**: HTTP client timeouts
```csharp
// Aspire automatically adds resilience, but you can customize:
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.CircuitBreaker.FailureRatio = 0.5;
    });
});
```

### 4. Performance Issues

**Problem**: Slow database queries
```csharp
// Add indexes using Data Annotations
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(CreatedAt))]
public class User { ... }
```

**Problem**: Memory leaks in HTTP clients
```
Solution: Use IHttpClientFactory (automatically configured by Aspire)
- Don't create HttpClient instances manually
- Use AddHttpClient<T> for typed clients
```

### 5. Observability Issues

**Problem**: Missing telemetry data
```
Solution: Ensure AddServiceDefaults() is called in Program.cs
- This automatically configures OpenTelemetry
- Health checks are automatically added
- Metrics collection is enabled
```

---

## Quick Reference

### New Service Checklist

1. **Create Service Project**
   ```bash
   dotnet new webapi -n NewService
   cd NewService
   dotnet add reference ../../QuantunSkyLink_v2.ServiceDefaults
   ```

2. **Add to AppHost.cs**
   ```csharp
   var postgresNewService = builder.AddConnectionString("postgres-newservice");
   var newService = builder.AddProject<Projects.NewService>("newservice")
       .WithReference(postgresNewService)
       .WithReference(redis);
   ```

3. **Add Connection String to appsettings.json**
   ```json
   "postgres-newservice": "Host=ep-aged-paper-a49vo4sv.us-east-1.aws.neon.tech;Database=quantumskylink_newservice;Username=neondb_owner;Password=npg_bDNRrMgSE28p;SSL Mode=Require;Trust Server Certificate=true"
   ```

4. **Configure Program.cs**
   ```csharp
   builder.AddServiceDefaults();
   builder.Services.AddDbContext<NewServiceDbContext>(options =>
       options.UseNpgsql(builder.Configuration.GetConnectionString("postgres")));
   ```

5. **Create DbContext with Heavy Annotations**
   ```csharp
   [Table("Entities")]
   [Index(nameof(Name), IsUnique = true)]
   public class Entity { ... }
   ```

### Connection String Template

```
Host=ep-aged-paper-a49vo4sv.us-east-1.aws.neon.tech;
Database=quantumskylink_{servicename};
Username=neondb_owner;
Password=npg_bDNRrMgSE28p;
SSL Mode=Require;
Trust Server Certificate=true
```

### Service Communication Template

```csharp
builder.Services.AddHttpClient<ITargetServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://targetservice");
});
```

---

## Conclusion

This implementation guide captures the proven patterns from QuantumSkyLink v2's 24-service production platform. The key principles are:

1. **Infrastructure-First**: Let Aspire handle infrastructure concerns
2. **Database-per-Service**: Complete data isolation with Neon PostgreSQL
3. **Heavy Annotations**: Minimize Fluent API, maximize Data Annotations
4. **Service Discovery**: Use service names, not hardcoded URLs
5. **Minimal Program.cs**: Focus on business logic registration

Following these patterns ensures consistency, maintainability, and scalability across your microservices architecture.
