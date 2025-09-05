using Microsoft.Extensions.Caching.Distributed;
using SignatureService.Models;
using System.Text.Json;

namespace SignatureService.Services;

/// <summary>
/// High-performance nonce tracking service for replay attack prevention
/// Uses Redis for distributed nonce storage with optimized performance
/// </summary>
public class NonceTrackingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<NonceTrackingService> _logger;
    
    // Cache settings for performance optimization
    private static readonly TimeSpan NonceRetentionTime = TimeSpan.FromMinutes(10); // 2x timestamp window
    private static readonly TimeSpan SequenceNumberCacheTime = TimeSpan.FromSeconds(30);

    public NonceTrackingService(
        IDistributedCache cache,
        ILogger<NonceTrackingService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates nonce and sequence number for replay protection
    /// Target: ≤200ms response time
    /// </summary>
    public async Task<NonceValidationResult> ValidateNonceAsync(
        string accountId,
        string nonce,
        long sequenceNumber,
        DateTime timestamp)
    {
        try
        {
            _logger.LogDebug("Validating nonce for account {AccountId}, nonce {Nonce}, sequence {SequenceNumber}", 
                accountId, nonce, sequenceNumber);

            // 1. Check if nonce already exists (replay attack detection)
            var nonceKey = GetNonceKey(accountId, nonce);
            var existingNonce = await _cache.GetStringAsync(nonceKey);
            
            if (!string.IsNullOrEmpty(existingNonce))
            {
                _logger.LogWarning("Replay attack detected: nonce {Nonce} already used for account {AccountId}", 
                    nonce, accountId);
                
                return new NonceValidationResult
                {
                    IsValid = false,
                    Error = "Nonce already used (replay attack detected)",
                    ValidatedAt = DateTime.UtcNow
                };
            }

            // 2. Validate sequence number (must be monotonically increasing)
            var sequenceValidation = await ValidateSequenceNumberAsync(accountId, sequenceNumber);
            if (!sequenceValidation.IsValid)
            {
                return sequenceValidation;
            }

            // 3. Validate timestamp window (already done in SignatureValidationService, but double-check)
            var timeDifference = Math.Abs((DateTime.UtcNow - timestamp).TotalMinutes);
            if (timeDifference > 5)
            {
                _logger.LogWarning("Timestamp outside acceptable window for account {AccountId}: {TimeDifference} minutes", 
                    accountId, timeDifference);
                
                return new NonceValidationResult
                {
                    IsValid = false,
                    Error = "Timestamp outside acceptable window (5 minutes)",
                    ValidatedAt = DateTime.UtcNow
                };
            }

            _logger.LogDebug("Nonce validation successful for account {AccountId}", accountId);
            
            return new NonceValidationResult
            {
                IsValid = true,
                ValidatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating nonce for account {AccountId}", accountId);
            
            return new NonceValidationResult
            {
                IsValid = false,
                Error = $"Internal nonce validation error: {ex.Message}",
                ValidatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Records a nonce to prevent replay attacks
    /// Target: ≤50ms response time (async, non-blocking)
    /// </summary>
    public async Task RecordNonceAsync(string accountId, string nonce, long sequenceNumber)
    {
        try
        {
            _logger.LogDebug("Recording nonce for account {AccountId}, nonce {Nonce}, sequence {SequenceNumber}", 
                accountId, nonce, sequenceNumber);

            // 1. Store nonce with expiration
            var nonceKey = GetNonceKey(accountId, nonce);
            var nonceData = new
            {
                account_id = accountId,
                nonce = nonce,
                sequence_number = sequenceNumber,
                recorded_at = DateTime.UtcNow
            };

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = NonceRetentionTime
            };

            await _cache.SetStringAsync(nonceKey, JsonSerializer.Serialize(nonceData), cacheOptions);

            // 2. Update latest sequence number for the account
            await UpdateLatestSequenceNumberAsync(accountId, sequenceNumber);

            _logger.LogDebug("Nonce recorded successfully for account {AccountId}", accountId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording nonce for account {AccountId}", accountId);
            // Don't throw - this is async and shouldn't fail the main validation
        }
    }

    /// <summary>
    /// Validates sequence number is monotonically increasing
    /// </summary>
    private async Task<NonceValidationResult> ValidateSequenceNumberAsync(string accountId, long sequenceNumber)
    {
        try
        {
            var sequenceKey = GetSequenceKey(accountId);
            var lastSequenceStr = await _cache.GetStringAsync(sequenceKey);
            
            if (!string.IsNullOrEmpty(lastSequenceStr) && long.TryParse(lastSequenceStr, out var lastSequence))
            {
                if (sequenceNumber <= lastSequence)
                {
                    _logger.LogWarning("Invalid sequence number for account {AccountId}: {SequenceNumber} <= {LastSequence}", 
                        accountId, sequenceNumber, lastSequence);
                    
                    return new NonceValidationResult
                    {
                        IsValid = false,
                        Error = $"Sequence number must be greater than {lastSequence}",
                        ValidatedAt = DateTime.UtcNow
                    };
                }
            }

            return new NonceValidationResult
            {
                IsValid = true,
                ValidatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating sequence number for account {AccountId}", accountId);
            
            return new NonceValidationResult
            {
                IsValid = false,
                Error = $"Internal sequence validation error: {ex.Message}",
                ValidatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Updates the latest sequence number for an account
    /// </summary>
    private async Task UpdateLatestSequenceNumberAsync(string accountId, long sequenceNumber)
    {
        try
        {
            var sequenceKey = GetSequenceKey(accountId);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = SequenceNumberCacheTime
            };

            await _cache.SetStringAsync(sequenceKey, sequenceNumber.ToString(), cacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sequence number for account {AccountId}", accountId);
            // Don't throw - this is a performance optimization, not critical
        }
    }

    /// <summary>
    /// Gets the cache key for a nonce
    /// </summary>
    private static string GetNonceKey(string accountId, string nonce)
    {
        return $"nonce:{accountId}:{nonce}";
    }

    /// <summary>
    /// Gets the cache key for sequence number tracking
    /// </summary>
    private static string GetSequenceKey(string accountId)
    {
        return $"sequence:{accountId}";
    }

    /// <summary>
    /// Gets nonce statistics for monitoring and debugging
    /// </summary>
    public async Task<Dictionary<string, object>> GetNonceStatsAsync(string accountId)
    {
        try
        {
            var sequenceKey = GetSequenceKey(accountId);
            var lastSequenceStr = await _cache.GetStringAsync(sequenceKey);
            
            var stats = new Dictionary<string, object>
            {
                ["account_id"] = accountId,
                ["last_sequence_number"] = lastSequenceStr ?? "none",
                ["timestamp"] = DateTime.UtcNow
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nonce stats for account {AccountId}", accountId);
            
            return new Dictionary<string, object>
            {
                ["account_id"] = accountId,
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Cleans up expired nonces (for maintenance)
    /// Note: Redis handles expiration automatically, but this can be used for manual cleanup
    /// </summary>
    public async Task CleanupExpiredNoncesAsync()
    {
        try
        {
            _logger.LogInformation("Starting nonce cleanup process");
            
            // Redis automatically handles expiration, but we can log the cleanup
            // In a production system, you might want to implement custom cleanup logic here
            
            _logger.LogInformation("Nonce cleanup completed (Redis handles automatic expiration)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during nonce cleanup");
        }
    }
}
