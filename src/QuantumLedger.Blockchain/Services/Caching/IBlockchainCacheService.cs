using System;
using System.Threading.Tasks;

namespace QuantumLedger.Blockchain.Services.Caching
{
    /// <summary>
    /// Defines the contract for blockchain caching operations.
    /// </summary>
    public interface IBlockchainCacheService
    {
        /// <summary>
        /// Gets a cached value by key.
        /// </summary>
        /// <typeparam name="T">The type of the cached value.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached value if found, otherwise null.</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Sets a value in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="expiry">The expiration time. If null, uses default expiration.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;

        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        /// <param name="key">The cache key to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes all cache entries matching a pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match (supports wildcards).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Gets cache statistics.
        /// </summary>
        /// <returns>Cache statistics including hit/miss ratios.</returns>
        CacheStatistics GetStatistics();
    }

    /// <summary>
    /// Represents cache statistics.
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// Gets or sets the total number of cache hits.
        /// </summary>
        public long Hits { get; set; }

        /// <summary>
        /// Gets or sets the total number of cache misses.
        /// </summary>
        public long Misses { get; set; }

        /// <summary>
        /// Gets the cache hit ratio as a percentage.
        /// </summary>
        public double HitRatio => Hits + Misses > 0 ? (double)Hits / (Hits + Misses) * 100 : 0;

        /// <summary>
        /// Gets or sets the total number of entries in the cache.
        /// </summary>
        public long EntryCount { get; set; }

        /// <summary>
        /// Gets or sets the estimated memory usage in bytes.
        /// </summary>
        public long EstimatedMemoryUsage { get; set; }
    }
}
