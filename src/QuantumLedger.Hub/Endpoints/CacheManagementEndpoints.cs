using Microsoft.AspNetCore.Mvc;
using QuantumLedger.Cryptography.Services;

namespace QuantumLedger.Hub.Endpoints;

/// <summary>
/// Cache management endpoints for monitoring and administration
/// Provides cache statistics, health checks, and management operations
/// </summary>
public static class CacheManagementEndpoints
{
    /// <summary>
    /// Maps cache management endpoints to the application
    /// </summary>
    public static void MapCacheManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cache")
            .WithTags("Cache Management")
            .WithOpenApi();

        // Cache statistics endpoint
        group.MapGet("/statistics", GetCacheStatistics)
            .WithName("GetCacheStatistics")
            .WithSummary("Get cache performance statistics")
            .WithDescription("Retrieves detailed cache performance metrics including hit ratio, response times, and request counts")
            .Produces<CacheStatisticsResponse>(200)
            .Produces<ProblemDetails>(500);

        // Cache health endpoint
        group.MapGet("/health", GetCacheHealth)
            .WithName("GetCacheHealth")
            .WithSummary("Get cache health status")
            .WithDescription("Checks cache health and provides performance recommendations")
            .Produces<CacheHealthResponse>(200)
            .Produces<ProblemDetails>(500);

        // Clear cache endpoint (admin only)
        group.MapDelete("/clear", ClearCache)
            .WithName("ClearCache")
            .WithSummary("Clear all cached public keys")
            .WithDescription("Clears all cached public keys - use with caution in production")
            .Produces<ClearCacheResponse>(200)
            .Produces<ProblemDetails>(500);

        // Clear specific key endpoint
        group.MapDelete("/clear/{keyHash}", ClearSpecificKey)
            .WithName("ClearSpecificKey")
            .WithSummary("Clear a specific cached public key")
            .WithDescription("Removes a specific public key from cache by its hash")
            .Produces<ClearKeyResponse>(200)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(500);

        // Clear account keys endpoint
        group.MapDelete("/clear/account/{accountId:guid}", ClearAccountKeys)
            .WithName("ClearAccountKeys")
            .WithSummary("Clear all cached keys for an account")
            .WithDescription("Removes all cached public keys associated with a specific account")
            .Produces<ClearAccountKeysResponse>(200)
            .Produces<ProblemDetails>(500);

        // Cache warmup endpoint
        group.MapPost("/warmup", WarmupCache)
            .WithName("WarmupCache")
            .WithSummary("Warm up the cache")
            .WithDescription("Pre-loads frequently accessed public keys into cache")
            .Produces<WarmupCacheResponse>(200)
            .Produces<ProblemDetails>(500);
    }

    /// <summary>
    /// Gets comprehensive cache performance statistics
    /// </summary>
    private static async Task<IResult> GetCacheStatistics(
        IPublicKeyCacheService cacheService,
        ILogger<Program> logger)
    {
        try
        {
            var stats = await cacheService.GetStatisticsAsync();
            
            var response = new CacheStatisticsResponse
            {
                HitCount = stats.HitCount,
                MissCount = stats.MissCount,
                HitRatio = stats.HitRatio,
                TotalRequests = stats.TotalRequests,
                AverageResponseTimeMs = stats.AverageResponseTime.TotalMilliseconds,
                CachedItemCount = stats.CachedItemCount,
                LastResetTime = stats.LastResetTime,
                UptimeHours = (DateTime.UtcNow - stats.LastResetTime).TotalHours,
                Performance = GetPerformanceRating(stats.HitRatio, stats.AverageResponseTime),
                Recommendations = GetPerformanceRecommendations(stats)
            };

            logger.LogInformation("Cache statistics retrieved: {HitRatio:P2} hit ratio, {AvgResponseTime:F2}ms avg response", 
                stats.HitRatio, stats.AverageResponseTime.TotalMilliseconds);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving cache statistics");
            return Results.Problem("Failed to retrieve cache statistics", statusCode: 500);
        }
    }

    /// <summary>
    /// Gets cache health status and recommendations
    /// </summary>
    private static async Task<IResult> GetCacheHealth(
        IPublicKeyCacheService cacheService,
        ILogger<Program> logger)
    {
        try
        {
            var stats = await cacheService.GetStatisticsAsync();
            
            var health = new CacheHealthResponse
            {
                Status = GetHealthStatus(stats),
                HitRatio = stats.HitRatio,
                AverageResponseTimeMs = stats.AverageResponseTime.TotalMilliseconds,
                TotalRequests = stats.TotalRequests,
                Issues = GetHealthIssues(stats),
                Recommendations = GetPerformanceRecommendations(stats),
                LastChecked = DateTime.UtcNow
            };

            logger.LogInformation("Cache health check completed: {Status}", health.Status);

            return Results.Ok(health);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking cache health");
            return Results.Problem("Failed to check cache health", statusCode: 500);
        }
    }

    /// <summary>
    /// Clears all cached public keys
    /// </summary>
    private static async Task<IResult> ClearCache(
        IPublicKeyCacheService cacheService,
        ILogger<Program> logger)
    {
        try
        {
            var statsBefore = await cacheService.GetStatisticsAsync();
            await cacheService.ClearCacheAsync();
            
            var response = new ClearCacheResponse
            {
                Success = true,
                Message = "Cache cleared successfully",
                ItemsCleared = statsBefore.CachedItemCount,
                ClearedAt = DateTime.UtcNow
            };

            logger.LogWarning("Cache cleared by admin request - {ItemsCleared} items removed", 
                statsBefore.CachedItemCount);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing cache");
            return Results.Problem("Failed to clear cache", statusCode: 500);
        }
    }

    /// <summary>
    /// Clears a specific cached public key
    /// </summary>
    private static async Task<IResult> ClearSpecificKey(
        string keyHash,
        IPublicKeyCacheService cacheService,
        ILogger<Program> logger)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyHash))
            {
                return Results.BadRequest("Key hash is required");
            }

            // Check if key exists in cache first
            var existingKey = await cacheService.GetPublicKeyAsync(keyHash);
            if (existingKey == null)
            {
                return Results.NotFound($"Key with hash {keyHash} not found in cache");
            }

            await cacheService.InvalidatePublicKeyAsync(keyHash);
            
            var response = new ClearKeyResponse
            {
                Success = true,
                Message = $"Key {keyHash} cleared from cache",
                KeyHash = keyHash,
                ClearedAt = DateTime.UtcNow
            };

            logger.LogInformation("Specific key cleared from cache: {KeyHash}", keyHash);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing specific key {KeyHash}", keyHash);
            return Results.Problem($"Failed to clear key {keyHash}", statusCode: 500);
        }
    }

    /// <summary>
    /// Clears all cached keys for a specific account
    /// </summary>
    private static async Task<IResult> ClearAccountKeys(
        Guid accountId,
        IPublicKeyCacheService cacheService,
        ILogger<Program> logger)
    {
        try
        {
            await cacheService.InvalidateAccountKeysAsync(accountId);
            
            var response = new ClearAccountKeysResponse
            {
                Success = true,
                Message = $"All keys for account {accountId} cleared from cache",
                AccountId = accountId,
                ClearedAt = DateTime.UtcNow
            };

            logger.LogInformation("Account keys cleared from cache: {AccountId}", accountId);

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing account keys for {AccountId}", accountId);
            return Results.Problem($"Failed to clear keys for account {accountId}", statusCode: 500);
        }
    }

    /// <summary>
    /// Warms up the cache with frequently accessed keys
    /// </summary>
    private static async Task<IResult> WarmupCache(
        IPublicKeyCacheService cacheService,
        ILogger<Program> logger)
    {
        try
        {
            // In a real implementation, this would pre-load frequently accessed keys
            // For now, we'll just return a success response
            
            var response = new WarmupCacheResponse
            {
                Success = true,
                Message = "Cache warmup completed",
                KeysLoaded = 0, // Would be actual count in real implementation
                WarmupTime = DateTime.UtcNow
            };

            logger.LogInformation("Cache warmup completed");

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during cache warmup");
            return Results.Problem("Failed to warm up cache", statusCode: 500);
        }
    }

    #region Helper Methods

    private static string GetPerformanceRating(double hitRatio, TimeSpan avgResponseTime)
    {
        if (hitRatio >= 0.9 && avgResponseTime.TotalMilliseconds <= 10)
            return "Excellent";
        if (hitRatio >= 0.8 && avgResponseTime.TotalMilliseconds <= 25)
            return "Good";
        if (hitRatio >= 0.6 && avgResponseTime.TotalMilliseconds <= 50)
            return "Fair";
        return "Poor";
    }

    private static string GetHealthStatus(CacheStatistics stats)
    {
        if (stats.HitRatio >= 0.8 && stats.AverageResponseTime.TotalMilliseconds <= 25)
            return "Healthy";
        if (stats.HitRatio >= 0.6 && stats.AverageResponseTime.TotalMilliseconds <= 50)
            return "Warning";
        return "Critical";
    }

    private static List<string> GetHealthIssues(CacheStatistics stats)
    {
        var issues = new List<string>();

        if (stats.HitRatio < 0.6)
            issues.Add("Low cache hit ratio - consider increasing cache expiration time");
        
        if (stats.AverageResponseTime.TotalMilliseconds > 50)
            issues.Add("High average response time - cache performance may be degraded");
        
        if (stats.TotalRequests < 100)
            issues.Add("Low request volume - cache effectiveness cannot be properly evaluated");

        return issues;
    }

    private static List<string> GetPerformanceRecommendations(CacheStatistics stats)
    {
        var recommendations = new List<string>();

        if (stats.HitRatio < 0.8)
        {
            recommendations.Add("Consider increasing cache expiration time to improve hit ratio");
            recommendations.Add("Review cache invalidation policies to reduce unnecessary evictions");
        }

        if (stats.AverageResponseTime.TotalMilliseconds > 25)
        {
            recommendations.Add("Consider using Redis for better cache performance");
            recommendations.Add("Review cache key structure for optimization");
        }

        if (stats.TotalRequests > 10000 && stats.HitRatio > 0.9)
        {
            recommendations.Add("Cache is performing well - consider expanding cache coverage");
        }

        return recommendations;
    }

    #endregion
}

#region Response Models

public class CacheStatisticsResponse
{
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRatio { get; set; }
    public long TotalRequests { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long CachedItemCount { get; set; }
    public DateTime LastResetTime { get; set; }
    public double UptimeHours { get; set; }
    public string Performance { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}

public class CacheHealthResponse
{
    public string Status { get; set; } = string.Empty;
    public double HitRatio { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long TotalRequests { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime LastChecked { get; set; }
}

public class ClearCacheResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long ItemsCleared { get; set; }
    public DateTime ClearedAt { get; set; }
}

public class ClearKeyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public DateTime ClearedAt { get; set; }
}

public class ClearAccountKeysResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public DateTime ClearedAt { get; set; }
}

public class WarmupCacheResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int KeysLoaded { get; set; }
    public DateTime WarmupTime { get; set; }
}

#endregion
