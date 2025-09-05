using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuantumLedger.Blockchain.Services.Caching
{
    /// <summary>
    /// Configuration options for the memory cache service.
    /// </summary>
    public class MemoryCacheOptions
    {
        /// <summary>
        /// Gets or sets the default expiration time for cache entries.
        /// </summary>
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the maximum size of the cache in MB.
        /// </summary>
        public int MaxSizeMB { get; set; } = 100;

        /// <summary>
        /// Gets or sets the sliding expiration time for frequently accessed items.
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(2);
    }

    /// <summary>
    /// In-memory implementation of the blockchain cache service.
    /// </summary>
    public class MemoryCacheService : IBlockchainCacheService, IDisposable
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly MemoryCacheOptions _options;
        private readonly ConcurrentDictionary<string, bool> _cacheKeys;
        private readonly Timer _cleanupTimer;
        
        // Statistics tracking
        private long _hits;
        private long _misses;
        private long _entryCount;

        /// <summary>
        /// Initializes a new instance of the MemoryCacheService class.
        /// </summary>
        /// <param name="memoryCache">The memory cache instance.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="options">The cache configuration options.</param>
        public MemoryCacheService(
            IMemoryCache memoryCache,
            ILogger<MemoryCacheService> logger,
            IOptions<MemoryCacheOptions> options)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? new MemoryCacheOptions();
            _cacheKeys = new ConcurrentDictionary<string, bool>();
            
            // Set up periodic cleanup timer (every 5 minutes)
            _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            
            _logger.LogInformation("MemoryCacheService initialized with default expiration: {DefaultExpiration}, max size: {MaxSizeMB}MB",
                _options.DefaultExpiration, _options.MaxSizeMB);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            try
            {
                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    Interlocked.Increment(ref _hits);
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    
                    if (cachedValue is string jsonString)
                    {
                        return JsonSerializer.Deserialize<T>(jsonString);
                    }
                    
                    return cachedValue as T;
                }

                Interlocked.Increment(ref _misses);
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving value from cache for key: {Key}", key);
                Interlocked.Increment(ref _misses);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                var expirationTime = expiry ?? _options.DefaultExpiration;
                
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expirationTime,
                    SlidingExpiration = _options.SlidingExpiration,
                    Priority = CacheItemPriority.Normal,
                    Size = EstimateObjectSize(value)
                };

                // Add removal callback to track entry count
                cacheEntryOptions.RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
                {
                    _cacheKeys.TryRemove(evictedKey.ToString(), out _);
                    Interlocked.Decrement(ref _entryCount);
                    _logger.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", evictedKey, reason);
                });

                // Serialize complex objects to JSON for consistent storage
                object cacheValue;
                if (value is string stringValue)
                {
                    cacheValue = stringValue;
                }
                else
                {
                    cacheValue = JsonSerializer.Serialize(value);
                }
                
                _memoryCache.Set(key, cacheValue, cacheEntryOptions);
                _cacheKeys.TryAdd(key, true);
                Interlocked.Increment(ref _entryCount);
                
                _logger.LogDebug("Cache entry set for key: {Key}, Expiration: {Expiration}", key, expirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            try
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
                _logger.LogDebug("Cache entry removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

            try
            {
                // Convert wildcard pattern to regex
                var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

                var keysToRemove = _cacheKeys.Keys.Where(key => regex.IsMatch(key)).ToList();
                
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                }

                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
                throw;
            }
        }

        /// <inheritdoc/>
        public CacheStatistics GetStatistics()
        {
            return new CacheStatistics
            {
                Hits = _hits,
                Misses = _misses,
                EntryCount = _entryCount,
                EstimatedMemoryUsage = EstimateMemoryUsage()
            };
        }

        /// <summary>
        /// Estimates the size of an object in bytes.
        /// </summary>
        /// <param name="obj">The object to estimate.</param>
        /// <returns>The estimated size in bytes.</returns>
        private static long EstimateObjectSize(object obj)
        {
            if (obj == null) return 0;
            
            try
            {
                var json = JsonSerializer.Serialize(obj);
                return System.Text.Encoding.UTF8.GetByteCount(json);
            }
            catch
            {
                // Fallback estimation
                return obj.ToString()?.Length * 2 ?? 0; // Rough estimate for string representation
            }
        }

        /// <summary>
        /// Estimates the total memory usage of the cache.
        /// </summary>
        /// <returns>The estimated memory usage in bytes.</returns>
        private long EstimateMemoryUsage()
        {
            // This is a rough estimation since IMemoryCache doesn't provide direct memory usage info
            return _entryCount * 1024; // Assume average 1KB per entry
        }

        /// <summary>
        /// Cleanup timer callback to remove expired entries and log statistics.
        /// </summary>
        /// <param name="state">Timer state (unused).</param>
        private void CleanupExpiredEntries(object state)
        {
            try
            {
                var stats = GetStatistics();
                _logger.LogInformation(
                    "Cache statistics - Hits: {Hits}, Misses: {Misses}, Hit Ratio: {HitRatio:F2}%, Entries: {EntryCount}, Memory: {MemoryUsage} bytes",
                    stats.Hits, stats.Misses, stats.HitRatio, stats.EntryCount, stats.EstimatedMemoryUsage);

                // Trigger garbage collection if memory usage is high
                if (stats.EstimatedMemoryUsage > _options.MaxSizeMB * 1024 * 1024)
                {
                    _logger.LogWarning("Cache memory usage ({MemoryUsage} bytes) exceeds limit ({MaxSizeMB}MB), triggering cleanup",
                        stats.EstimatedMemoryUsage, _options.MaxSizeMB);
                    
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache cleanup");
            }
        }

        /// <summary>
        /// Disposes the cache service and cleanup timer.
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _logger.LogInformation("MemoryCacheService disposed");
        }
    }
}
