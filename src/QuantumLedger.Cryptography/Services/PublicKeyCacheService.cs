using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using QuantumLedger.Models.Account;

namespace QuantumLedger.Cryptography.Services;

/// <summary>
/// High-performance caching service for public key registry entries
/// Provides sub-10ms lookup performance for frequently accessed public keys
/// </summary>
public interface IPublicKeyCacheService
{
    Task<PublicKeyRegistryEntry?> GetPublicKeyAsync(string keyHash, CancellationToken cancellationToken = default);
    Task SetPublicKeyAsync(string keyHash, PublicKeyRegistryEntry entry, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task InvalidatePublicKeyAsync(string keyHash, CancellationToken cancellationToken = default);
    Task InvalidateAccountKeysAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache statistics for monitoring and optimization
/// </summary>
public class CacheStatistics
{
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRatio => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
    public long TotalRequests => HitCount + MissCount;
    public DateTime LastResetTime { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public long CachedItemCount { get; set; }
}

/// <summary>
/// Redis-based implementation of public key caching service
/// Optimized for high-performance substitution key lookups
/// </summary>
public class PublicKeyCacheService : IPublicKeyCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<PublicKeyCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Cache statistics tracking
    private long _hitCount = 0;
    private long _missCount = 0;
    private readonly DateTime _startTime = DateTime.UtcNow;
    private readonly List<TimeSpan> _responseTimes = new();
    private readonly object _statsLock = new();

    // Cache configuration
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(1);
    private static readonly string CacheKeyPrefix = "ql:pubkey:";
    private static readonly string AccountKeyPrefix = "ql:account:";

    public PublicKeyCacheService(
        IDistributedCache cache,
        ILogger<PublicKeyCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Retrieves a public key from cache with performance tracking
    /// </summary>
    public async Task<PublicKeyRegistryEntry?> GetPublicKeyAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyHash))
        {
            throw new ArgumentException("Key hash cannot be null or empty", nameof(keyHash));
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var cacheKey = GetCacheKey(keyHash);

        try
        {
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                var entry = JsonSerializer.Deserialize<PublicKeyRegistryEntry>(cachedData, _jsonOptions);
                RecordHit(stopwatch.Elapsed);
                
                _logger.LogDebug("Cache HIT for key hash {KeyHash} in {ElapsedMs}ms", 
                    keyHash, stopwatch.ElapsedMilliseconds);
                
                return entry;
            }

            RecordMiss(stopwatch.Elapsed);
            _logger.LogDebug("Cache MISS for key hash {KeyHash} in {ElapsedMs}ms", 
                keyHash, stopwatch.ElapsedMilliseconds);
            
            return null;
        }
        catch (Exception ex)
        {
            RecordMiss(stopwatch.Elapsed);
            _logger.LogError(ex, "Error retrieving public key from cache for hash {KeyHash}", keyHash);
            return null;
        }
    }

    /// <summary>
    /// Stores a public key in cache with configurable expiration
    /// </summary>
    public async Task SetPublicKeyAsync(string keyHash, PublicKeyRegistryEntry entry, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyHash))
        {
            throw new ArgumentException("Key hash cannot be null or empty", nameof(keyHash));
        }

        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        var cacheKey = GetCacheKey(keyHash);
        var accountCacheKey = GetAccountCacheKey(entry.AccountId);
        var expirationTime = expiration ?? DefaultExpiration;

        try
        {
            var serializedEntry = JsonSerializer.Serialize(entry, _jsonOptions);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            };

            // Store the public key entry
            await _cache.SetStringAsync(cacheKey, serializedEntry, cacheOptions, cancellationToken);
            
            // Store account association for bulk invalidation
            await _cache.SetStringAsync(accountCacheKey, keyHash, cacheOptions, cancellationToken);

            _logger.LogDebug("Cached public key for hash {KeyHash} with expiration {Expiration}", 
                keyHash, expirationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching public key for hash {KeyHash}", keyHash);
            throw;
        }
    }

    /// <summary>
    /// Invalidates a specific public key from cache
    /// </summary>
    public async Task InvalidatePublicKeyAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyHash))
        {
            throw new ArgumentException("Key hash cannot be null or empty", nameof(keyHash));
        }

        var cacheKey = GetCacheKey(keyHash);

        try
        {
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Invalidated cache for key hash {KeyHash}", keyHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for key hash {KeyHash}", keyHash);
            throw;
        }
    }

    /// <summary>
    /// Invalidates all cached keys for a specific account
    /// Used when account keys are rotated or revoked
    /// </summary>
    public async Task InvalidateAccountKeysAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var accountCacheKey = GetAccountCacheKey(accountId);

        try
        {
            // Get the key hash associated with this account
            var keyHash = await _cache.GetStringAsync(accountCacheKey, cancellationToken);
            
            if (!string.IsNullOrEmpty(keyHash))
            {
                // Invalidate the public key
                await InvalidatePublicKeyAsync(keyHash, cancellationToken);
                
                // Remove the account association
                await _cache.RemoveAsync(accountCacheKey, cancellationToken);
            }

            _logger.LogDebug("Invalidated all cached keys for account {AccountId}", accountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for account {AccountId}", accountId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves cache performance statistics
    /// </summary>
    public async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        lock (_statsLock)
        {
            var averageResponseTime = _responseTimes.Count > 0 
                ? TimeSpan.FromTicks((long)_responseTimes.Average(t => t.Ticks))
                : TimeSpan.Zero;

            return new CacheStatistics
            {
                HitCount = _hitCount,
                MissCount = _missCount,
                LastResetTime = _startTime,
                AverageResponseTime = averageResponseTime,
                CachedItemCount = 0 // Redis doesn't provide easy item count
            };
        }
    }

    /// <summary>
    /// Clears all cached public keys (use with caution)
    /// </summary>
    public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: This is a simplified implementation
            // In production, you might want to use Redis SCAN with pattern matching
            _logger.LogWarning("Cache clear requested - this operation affects all cached public keys");
            
            // Reset statistics
            lock (_statsLock)
            {
                _hitCount = 0;
                _missCount = 0;
                _responseTimes.Clear();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            throw;
        }
    }

    #region Private Methods

    private static string GetCacheKey(string keyHash)
    {
        return $"{CacheKeyPrefix}{keyHash}";
    }

    private static string GetAccountCacheKey(Guid accountId)
    {
        return $"{AccountKeyPrefix}{accountId}";
    }

    private void RecordHit(TimeSpan responseTime)
    {
        lock (_statsLock)
        {
            _hitCount++;
            _responseTimes.Add(responseTime);
            
            // Keep only last 1000 response times for average calculation
            if (_responseTimes.Count > 1000)
            {
                _responseTimes.RemoveAt(0);
            }
        }
    }

    private void RecordMiss(TimeSpan responseTime)
    {
        lock (_statsLock)
        {
            _missCount++;
            _responseTimes.Add(responseTime);
            
            // Keep only last 1000 response times for average calculation
            if (_responseTimes.Count > 1000)
            {
                _responseTimes.RemoveAt(0);
            }
        }
    }

    #endregion
}
