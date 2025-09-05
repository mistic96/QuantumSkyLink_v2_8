using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using Mapster;
using NotificationService.Services;
using NotificationService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults - handles service discovery, observability, health checks, JWT, Redis, etc.
builder.AddServiceDefaults();

// Add Redis distributed cache (required for cache services)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("cache");
});

// Add SendGrid client for email notifications
builder.Services.AddSingleton<SendGrid.ISendGridClient>(provider =>
{
    var apiKey = builder.Configuration["SendGrid:ApiKey"] ?? string.Empty;
    return new SendGrid.SendGridClient(apiKey);
});

// Add Entity Framework with PostgreSQL (Aspire will resolve connection string)
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgres-notificationservice")));

// Add Mapster for object mapping
builder.Services.AddMapster();

// Add SignalR for real-time notifications
builder.Services.AddSignalR();

// Register application services
// Register all notification services
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();
builder.Services.AddScoped<INotificationDeliveryService, NotificationDeliveryService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotificationQueueService, NotificationQueueService>();
builder.Services.AddScoped<IUserNotificationPreferenceService, UserNotificationPreferenceService>();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();

// Register specialized user preference services
builder.Services.AddScoped<UserNotificationPreferenceCacheService>();
builder.Services.AddScoped<UserNotificationPreferenceAnalyticsService>();
builder.Services.AddScoped<UserNotificationPreferenceBulkService>();
builder.Services.AddScoped<UserNotificationPreferenceValidationService>();

// Add HTTP clients for external services
builder.Services.AddHttpClient("SendGrid", client =>
{
    client.BaseAddress = new Uri("https://api.sendgrid.com/");
});

builder.Services.AddHttpClient("Twilio", client =>
{
    client.BaseAddress = new Uri("https://api.twilio.com/");
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

// Map SignalR hubs
app.MapHub<NotificationHub>("/notificationHub");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Aspire handles logging
        throw;
    }
}

app.Run();
