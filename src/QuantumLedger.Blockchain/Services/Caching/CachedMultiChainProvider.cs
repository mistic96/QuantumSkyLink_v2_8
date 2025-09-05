using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumLedger.Blockchain.Models;
using QuantumLedger.Blockchain.Services.MultiChain;

namespace QuantumLedger.Blockchain.Services.Caching
{
    /// <summary>
    /// Cached wrapper for the MultiChain provider that implements caching for blockchain operations.
    /// </summary>
    public class CachedMultiChainProvider : IBlockchainProvider
    {
        private readonly MultiChainProvider _innerProvider;
        private readonly IBlockchainCacheService _cacheService;
        private readonly ILogger<CachedMultiChainProvider> _logger;

        // Cache key prefixes
        private const string ACCOUNT_STATE_PREFIX = "account_state";
        private const string TRANSACTION_STATUS_PREFIX = "transaction_status";
        private const string BLOCK_INFO_PREFIX = "block_info";
        private const string BLOCKCHAIN_STATUS_PREFIX = "blockchain_status";

        // Cache expiration times
        private static readonly TimeSpan AccountStateCacheExpiry = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan TransactionStatusCacheExpiry = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan BlockInfoCacheExpiry = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan BlockchainStatusCacheExpiry = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Initializes a new instance of the CachedMultiChainProvider class.
        /// </summary>
        /// <param name="innerProvider">The inner MultiChain provider to wrap.</param>
        /// <param name="cacheService">The cache service to use.</param>
        /// <param name="logger">The logger instance.</param>
        public CachedMultiChainProvider(
            MultiChainProvider innerProvider,
            IBlockchainCacheService cacheService,
            ILogger<CachedMultiChainProvider> logger)
        {
            _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public string ProviderName => $"Cached{_innerProvider.ProviderName}";

        /// <inheritdoc/>
        public async Task<bool> IsHealthyAsync()
        {
            // Health checks are not cached as they should be real-time
            return await _innerProvider.IsHealthyAsync();
        }

        /// <inheritdoc/>
        public async Task<BlockchainStatus> GetStatusAsync()
        {
            // Use string.Create for zero-allocation cache key generation
            var cacheKey = string.Create(
                BLOCKCHAIN_STATUS_PREFIX.Length + 7, // ":status" = 7 chars
                BLOCKCHAIN_STATUS_PREFIX,
                static (span, prefix) =>
                {
                    prefix.AsSpan().CopyTo(span);
                    ":status".AsSpan().CopyTo(span[prefix.Length..]);
                });
            
            try
            {
                // Try to get from cache first
                var cachedStatus = await _cacheService.GetAsync<BlockchainStatus>(cacheKey);
                if (cachedStatus != null)
                {
                    _logger.LogDebug("Blockchain status retrieved from cache");
                    return cachedStatus;
                }

                // Get from provider and cache the result
                var status = await _innerProvider.GetStatusAsync();
                
                if (status.IsConnected)
                {
                    await _cacheService.SetAsync(cacheKey, status, BlockchainStatusCacheExpiry);
                    _logger.LogDebug("Blockchain status cached for {Expiry}", BlockchainStatusCacheExpiry);
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blockchain status with caching");
                // Fallback to direct provider call
                return await _innerProvider.GetStatusAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<Result<string>> SubmitTransactionAsync(Transaction transaction)
        {
            // Transaction submissions are never cached as they are write operations
            var result = await _innerProvider.SubmitTransactionAsync(transaction);
            
            if (result.IsSuccess)
            {
                // Invalidate related cache entries when a transaction is submitted
                await InvalidateTransactionRelatedCaches(transaction);
                _logger.LogDebug("Invalidated cache entries for transaction submission");
            }
            
            return result;
        }

        /// <inheritdoc/>
        public async Task<Result<TransactionStatus>> GetTransactionStatusAsync(string txId)
        {
            // Use string.Create for zero-allocation cache key generation
            var cacheKey = string.Create(
                TRANSACTION_STATUS_PREFIX.Length + 1 + txId.Length,
                (prefix: TRANSACTION_STATUS_PREFIX, id: txId),
                static (span, state) =>
                {
                    state.prefix.AsSpan().CopyTo(span);
                    span[state.prefix.Length] = ':';
                    state.id.AsSpan().CopyTo(span[(state.prefix.Length + 1)..]);
                });
            
            try
            {
                // Try to get from cache first
                var cachedStatus = await _cacheService.GetAsync<TransactionStatus>(cacheKey);
                if (cachedStatus != null)
                {
                    _logger.LogDebug("Transaction status retrieved from cache for txId: {TxId}", txId);
                    return Result<TransactionStatus>.Success(cachedStatus);
                }

                // Get from provider
                var result = await _innerProvider.GetTransactionStatusAsync(txId);
                
                if (result.IsSuccess)
                {
                    // Cache confirmed transactions for longer, pending transactions for shorter time
                    var expiry = result.Data.Status == TransactionStatuses.Confirmed 
                        ? TimeSpan.FromMinutes(10) 
                        : TransactionStatusCacheExpiry;
                    
                    await _cacheService.SetAsync(cacheKey, result.Data, expiry);
                    _logger.LogDebug("Transaction status cached for txId: {TxId}, expiry: {Expiry}", txId, expiry);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction status with caching for txId: {TxId}", txId);
                // Fallback to direct provider call
                return await _innerProvider.GetTransactionStatusAsync(txId);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<BlockInfo>> GetBlockInfoAsync(string blockId)
        {
            // Use string.Create for zero-allocation cache key generation
            var cacheKey = string.Create(
                BLOCK_INFO_PREFIX.Length + 1 + blockId.Length,
                (prefix: BLOCK_INFO_PREFIX, id: blockId),
                static (span, state) =>
                {
                    state.prefix.AsSpan().CopyTo(span);
                    span[state.prefix.Length] = ':';
                    state.id.AsSpan().CopyTo(span[(state.prefix.Length + 1)..]);
                });
            
            try
            {
                // Try to get from cache first
                var cachedBlockInfo = await _cacheService.GetAsync<BlockInfo>(cacheKey);
                if (cachedBlockInfo != null)
                {
                    _logger.LogDebug("Block info retrieved from cache for blockId: {BlockId}", blockId);
                    return Result<BlockInfo>.Success(cachedBlockInfo);
                }

                // Get from provider
                var result = await _innerProvider.GetBlockInfoAsync(blockId);
                
                if (result.IsSuccess)
                {
                    // Block info is immutable once created, so cache for a long time
                    await _cacheService.SetAsync(cacheKey, result.Data, BlockInfoCacheExpiry);
                    _logger.LogDebug("Block info cached for blockId: {BlockId}", blockId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting block info with caching for blockId: {BlockId}", blockId);
                // Fallback to direct provider call
                return await _innerProvider.GetBlockInfoAsync(blockId);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<AccountState>> GetAccountStateAsync(string address)
        {
            // Use string.Create for zero-allocation cache key generation
            var cacheKey = string.Create(
                ACCOUNT_STATE_PREFIX.Length + 1 + address.Length,
                (prefix: ACCOUNT_STATE_PREFIX, addr: address),
                static (span, state) =>
                {
                    state.prefix.AsSpan().CopyTo(span);
                    span[state.prefix.Length] = ':';
                    state.addr.AsSpan().CopyTo(span[(state.prefix.Length + 1)..]);
                });
            
            try
            {
                // Try to get from cache first
                var cachedAccountState = await _cacheService.GetAsync<AccountState>(cacheKey);
                if (cachedAccountState != null)
                {
                    _logger.LogDebug("Account state retrieved from cache for address: {Address}", address);
                    return Result<AccountState>.Success(cachedAccountState);
                }

                // Get from provider
                var result = await _innerProvider.GetAccountStateAsync(address);
                
                if (result.IsSuccess)
                {
                    await _cacheService.SetAsync(cacheKey, result.Data, AccountStateCacheExpiry);
                    _logger.LogDebug("Account state cached for address: {Address}", address);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account state with caching for address: {Address}", address);
                // Fallback to direct provider call
                return await _innerProvider.GetAccountStateAsync(address);
            }
        }

        /// <summary>
        /// Invalidates cache entries related to a transaction.
        /// </summary>
        /// <param name="transaction">The transaction that was submitted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task InvalidateTransactionRelatedCaches(Transaction transaction)
        {
            try
            {
                // Invalidate account states for involved addresses using optimized cache key generation
                if (!string.IsNullOrEmpty(transaction.FromAddress))
                {
                    var fromCacheKey = string.Create(
                        ACCOUNT_STATE_PREFIX.Length + 1 + transaction.FromAddress.Length,
                        (prefix: ACCOUNT_STATE_PREFIX, addr: transaction.FromAddress),
                        static (span, state) =>
                        {
                            state.prefix.AsSpan().CopyTo(span);
                            span[state.prefix.Length] = ':';
                            state.addr.AsSpan().CopyTo(span[(state.prefix.Length + 1)..]);
                        });
                    await _cacheService.RemoveAsync(fromCacheKey);
                }

                if (!string.IsNullOrEmpty(transaction.ToAddress))
                {
                    var toCacheKey = string.Create(
                        ACCOUNT_STATE_PREFIX.Length + 1 + transaction.ToAddress.Length,
                        (prefix: ACCOUNT_STATE_PREFIX, addr: transaction.ToAddress),
                        static (span, state) =>
                        {
                            state.prefix.AsSpan().CopyTo(span);
                            span[state.prefix.Length] = ':';
                            state.addr.AsSpan().CopyTo(span[(state.prefix.Length + 1)..]);
                        });
                    await _cacheService.RemoveAsync(toCacheKey);
                }

                // Invalidate blockchain status as it may have changed
                await _cacheService.RemoveByPatternAsync($"{BLOCKCHAIN_STATUS_PREFIX}:*");
                
                _logger.LogDebug("Invalidated cache entries for transaction from {FromAddress} to {ToAddress}", 
                    transaction.FromAddress, transaction.ToAddress);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating transaction-related cache entries");
                // Don't throw as this is not critical for the transaction submission
            }
        }

        /// <summary>
        /// Gets cache statistics from the underlying cache service.
        /// </summary>
        /// <returns>Cache statistics.</returns>
        public CacheStatistics GetCacheStatistics()
        {
            return _cacheService.GetStatistics();
        }

        /// <summary>
        /// Clears all cache entries.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ClearCacheAsync()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync("*");
                _logger.LogInformation("All cache entries cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                throw;
            }
        }

        /// <summary>
        /// Clears cache entries for a specific address.
        /// </summary>
        /// <param name="address">The address to clear cache for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ClearAddressCacheAsync(string address)
        {
            try
            {
                // Use string.Create for zero-allocation cache key generation
                var cacheKey = string.Create(
                    ACCOUNT_STATE_PREFIX.Length + 1 + address.Length,
                    (prefix: ACCOUNT_STATE_PREFIX, addr: address),
                    static (span, state) =>
                    {
                        state.prefix.AsSpan().CopyTo(span);
                        span[state.prefix.Length] = ':';
                        state.addr.AsSpan().CopyTo(span[(state.prefix.Length + 1)..]);
                    });
                await _cacheService.RemoveAsync(cacheKey);
                _logger.LogDebug("Cache cleared for address: {Address}", address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for address: {Address}", address);
                throw;
            }
        }
    }
}
