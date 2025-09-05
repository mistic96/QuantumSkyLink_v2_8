using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace QuantumLedger.Hub.Middleware;

/// <summary>
/// Rate limiting middleware for API protection
/// Provides configurable rate limits for different endpoint types
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitingOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var clientId = GetClientIdentifier(context);

        // Check if this endpoint should be rate limited
        var rateLimitRule = GetRateLimitRule(endpoint);
        if (rateLimitRule == null)
        {
            await _next(context);
            return;
        }

        // Check rate limit
        var isAllowed = await CheckRateLimitAsync(clientId, endpoint, rateLimitRule);
        if (!isAllowed)
        {
            await HandleRateLimitExceeded(context, rateLimitRule);
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get client IP address
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        
        // Check for forwarded headers (for load balancers/proxies)
        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                clientIp = forwardedFor.Split(',')[0].Trim();
            }
        }
        else if (context.Request.Headers.ContainsKey("X-Real-IP"))
        {
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                clientIp = realIp;
            }
        }

        // For authenticated requests, could also use user ID
        // var userId = context.User?.Identity?.Name;

        return clientIp ?? "unknown";
    }

    private RateLimitRule? GetRateLimitRule(string endpoint)
    {
        foreach (var rule in _options.Rules)
        {
            if (endpoint.Contains(rule.PathPattern, StringComparison.OrdinalIgnoreCase))
            {
                return rule;
            }
        }

        return null;
    }

    private async Task<bool> CheckRateLimitAsync(string clientId, string endpoint, RateLimitRule rule)
    {
        var cacheKey = $"ratelimit:{clientId}:{rule.PathPattern}";
        var now = DateTime.UtcNow;

        // Get current request count from cache
        var requestData = _cache.Get<RateLimitData>(cacheKey);
        
        if (requestData == null)
        {
            // First request in the window
            requestData = new RateLimitData
            {
                RequestCount = 1,
                WindowStart = now,
                LastRequest = now
            };
            
            var expiration = TimeSpan.FromSeconds(rule.WindowSizeSeconds);
            _cache.Set(cacheKey, requestData, expiration);
            
            _logger.LogDebug("Rate limit initialized for {ClientId} on {Endpoint}: 1/{Limit}", 
                clientId, endpoint, rule.RequestLimit);
            
            return true;
        }

        // Check if we're still in the same time window
        var windowEnd = requestData.WindowStart.AddSeconds(rule.WindowSizeSeconds);
        if (now > windowEnd)
        {
            // New time window - reset counter
            requestData.RequestCount = 1;
            requestData.WindowStart = now;
            requestData.LastRequest = now;
            
            var expiration = TimeSpan.FromSeconds(rule.WindowSizeSeconds);
            _cache.Set(cacheKey, requestData, expiration);
            
            _logger.LogDebug("Rate limit window reset for {ClientId} on {Endpoint}: 1/{Limit}", 
                clientId, endpoint, rule.RequestLimit);
            
            return true;
        }

        // Check if limit is exceeded
        if (requestData.RequestCount >= rule.RequestLimit)
        {
            _logger.LogWarning("Rate limit exceeded for {ClientId} on {Endpoint}: {Count}/{Limit}", 
                clientId, endpoint, requestData.RequestCount, rule.RequestLimit);
            
            return false;
        }

        // Increment counter
        requestData.RequestCount++;
        requestData.LastRequest = now;
        
        var remainingTime = windowEnd - now;
        _cache.Set(cacheKey, requestData, remainingTime);
        
        _logger.LogDebug("Rate limit updated for {ClientId} on {Endpoint}: {Count}/{Limit}", 
            clientId, endpoint, requestData.RequestCount, rule.RequestLimit);
        
        return true;
    }

    private async Task HandleRateLimitExceeded(HttpContext context, RateLimitRule rule)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Rate limit exceeded",
            message = $"Too many requests. Limit: {rule.RequestLimit} requests per {rule.WindowSizeSeconds} seconds",
            retryAfter = rule.WindowSizeSeconds
        };

        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", rule.RequestLimit.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", "0");
        context.Response.Headers.Add("X-RateLimit-Reset", DateTimeOffset.UtcNow.AddSeconds(rule.WindowSizeSeconds).ToUnixTimeSeconds().ToString());
        context.Response.Headers.Add("Retry-After", rule.WindowSizeSeconds.ToString());

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Configuration options for rate limiting
/// </summary>
public class RateLimitingOptions
{
    public List<RateLimitRule> Rules { get; set; } = new();
    public List<string> WhitelistedIPs { get; set; } = new();
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Rate limit rule for specific endpoints
/// </summary>
public class RateLimitRule
{
    public string PathPattern { get; set; } = string.Empty;
    public int RequestLimit { get; set; }
    public int WindowSizeSeconds { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Internal data structure for tracking rate limit state
/// </summary>
internal class RateLimitData
{
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime LastRequest { get; set; }
}

/// <summary>
/// Extension methods for rate limiting middleware
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }

    /// <summary>
    /// Adds rate limiting services to the DI container
    /// </summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, Action<RateLimitingOptions> configureOptions)
    {
        var options = new RateLimitingOptions();
        configureOptions(options);
        
        services.AddSingleton(options);
        services.AddMemoryCache(); // Required for rate limiting storage
        
        return services;
    }

    /// <summary>
    /// Adds default rate limiting rules for substitution key endpoints
    /// </summary>
    public static IServiceCollection AddSubstitutionKeyRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiting(options =>
        {
            // Key generation - most restrictive
            options.Rules.Add(new RateLimitRule
            {
                PathPattern = "/api/substitution-keys/generate",
                RequestLimit = 10, // 10 requests per minute
                WindowSizeSeconds = 60,
                Description = "Substitution key generation rate limit"
            });

            // Key validation and verification - moderate
            options.Rules.Add(new RateLimitRule
            {
                PathPattern = "/api/substitution-keys/validate",
                RequestLimit = 100, // 100 requests per minute
                WindowSizeSeconds = 60,
                Description = "Substitution key validation rate limit"
            });

            options.Rules.Add(new RateLimitRule
            {
                PathPattern = "/api/substitution-keys/verify-signature",
                RequestLimit = 1000, // 1000 requests per minute
                WindowSizeSeconds = 60,
                Description = "Signature verification rate limit"
            });

            // Key management operations - moderate
            options.Rules.Add(new RateLimitRule
            {
                PathPattern = "/api/substitution-keys/",
                RequestLimit = 200, // 200 requests per minute for all other operations
                WindowSizeSeconds = 60,
                Description = "General substitution key operations rate limit"
            });

            // Enable logging
            options.EnableLogging = true;
        });
    }
}
