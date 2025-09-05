using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PaymentGatewayService.Services;

/// <summary>
/// Payment cache service implementing IPaymentCacheService
/// Handles Redis integration for payment data, cache invalidation strategies, and performance optimization
/// </summary>
public class PaymentCacheService : IPaymentCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<PaymentCacheService> _logger;

    // Cache configuration constants
    private const int DefaultCacheExpirationMinutes = 30;
    private const int PaymentCacheExpirationMinutes = 60;
    private const int PaymentMethodCacheExpirationMinutes = 120;
    private const int GatewayCacheExpirationMinutes = 240;

    // Cache key prefixes
    private const string PaymentKeyPrefix = "payment:";
    private const string PaymentMethodKeyPrefix = "payment_method:";
    private const string GatewayKeyPrefix = "gateway:";
    private const string UserPaymentsKeyPrefix = "user_payments:";
    private const string PaymentStatsKeyPrefix = "payment_stats:";

    public PaymentCacheService(
        IDistributedCache cache,
        ILogger<PaymentCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Caches a payment entity
    /// </summary>
    public async Task CachePaymentAsync(Payment payment)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Caching payment. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}", 
            correlationId, payment.Id);

        try
        {
            var key = GetPaymentKey(payment.Id);
            var serializedPayment = JsonSerializer.Serialize(payment, GetJsonSerializerOptions());
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PaymentCacheExpirationMinutes)
            };

            await _cache.SetStringAsync(key, serializedPayment, options);

            _logger.LogInformation("Payment cached successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching payment. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - caching failures shouldn't break the application
        }
    }

    /// <summary>
    /// Gets a cached payment by ID
    /// </summary>
    public async Task<Payment?> GetPaymentAsync(Guid paymentId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving cached payment. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}", 
            correlationId, paymentId);

        try
        {
            var key = GetPaymentKey(paymentId);
            var cachedPayment = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedPayment))
            {
                _logger.LogInformation("Payment not found in cache. CorrelationId: {CorrelationId}, Key: {Key}", 
                    correlationId, key);
                return null;
            }

            var payment = JsonSerializer.Deserialize<Payment>(cachedPayment, GetJsonSerializerOptions());
            
            _logger.LogInformation("Payment retrieved from cache successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached payment. CorrelationId: {CorrelationId}", correlationId);
            return null; // Return null on cache errors to fall back to database
        }
    }

    /// <summary>
    /// Removes a payment from cache
    /// </summary>
    public async Task RemovePaymentAsync(Guid paymentId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Removing payment from cache. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}", 
            correlationId, paymentId);

        try
        {
            var key = GetPaymentKey(paymentId);
            await _cache.RemoveAsync(key);

            _logger.LogInformation("Payment removed from cache successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing payment from cache. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - cache removal failures shouldn't break the application
        }
    }

    /// <summary>
    /// Caches a payment method entity
    /// </summary>
    public async Task CachePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Caching payment method. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
            correlationId, paymentMethod.Id);

        try
        {
            var key = GetPaymentMethodKey(paymentMethod.Id);
            var serializedPaymentMethod = JsonSerializer.Serialize(paymentMethod, GetJsonSerializerOptions());
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PaymentMethodCacheExpirationMinutes)
            };

            await _cache.SetStringAsync(key, serializedPaymentMethod, options);

            _logger.LogInformation("Payment method cached successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching payment method. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - caching failures shouldn't break the application
        }
    }

    /// <summary>
    /// Gets a cached payment method by ID
    /// </summary>
    public async Task<PaymentMethod?> GetPaymentMethodAsync(Guid paymentMethodId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving cached payment method. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
            correlationId, paymentMethodId);

        try
        {
            var key = GetPaymentMethodKey(paymentMethodId);
            var cachedPaymentMethod = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedPaymentMethod))
            {
                _logger.LogInformation("Payment method not found in cache. CorrelationId: {CorrelationId}, Key: {Key}", 
                    correlationId, key);
                return null;
            }

            var paymentMethod = JsonSerializer.Deserialize<PaymentMethod>(cachedPaymentMethod, GetJsonSerializerOptions());
            
            _logger.LogInformation("Payment method retrieved from cache successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);

            return paymentMethod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached payment method. CorrelationId: {CorrelationId}", correlationId);
            return null; // Return null on cache errors to fall back to database
        }
    }

    /// <summary>
    /// Caches a payment gateway entity
    /// </summary>
    public async Task CacheGatewayAsync(PaymentGateway gateway)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Caching payment gateway. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, gateway.Id);

        try
        {
            var key = GetGatewayKey(gateway.Id);
            var serializedGateway = JsonSerializer.Serialize(gateway, GetJsonSerializerOptions());
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(GatewayCacheExpirationMinutes)
            };

            await _cache.SetStringAsync(key, serializedGateway, options);

            _logger.LogInformation("Payment gateway cached successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching payment gateway. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - caching failures shouldn't break the application
        }
    }

    /// <summary>
    /// Gets cached gateway configuration
    /// </summary>
    public async Task<PaymentGateway?> GetGatewayAsync(Guid gatewayId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving cached payment gateway. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
            correlationId, gatewayId);

        try
        {
            var key = GetGatewayKey(gatewayId);
            var cachedGateway = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedGateway))
            {
                _logger.LogInformation("Payment gateway not found in cache. CorrelationId: {CorrelationId}, Key: {Key}", 
                    correlationId, key);
                return null;
            }

            var gateway = JsonSerializer.Deserialize<PaymentGateway>(cachedGateway, GetJsonSerializerOptions());
            
            _logger.LogInformation("Payment gateway retrieved from cache successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);

            return gateway;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached payment gateway. CorrelationId: {CorrelationId}", correlationId);
            return null; // Return null on cache errors to fall back to database
        }
    }

    /// <summary>
    /// Invalidates all payment-related cache for a user
    /// </summary>
    public async Task InvalidateUserCacheAsync(Guid userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Invalidating user cache. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            // Remove user payments from cache
            await RemoveUserPaymentsAsync(userId);

            // Remove payment statistics from cache
            await RemovePaymentStatsAsync(userId);

            _logger.LogInformation("User cache invalidated successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user cache. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - cache invalidation failures shouldn't break the application
        }
    }

    #region Additional Cache Methods (Not in Interface)

    /// <summary>
    /// Caches user payments list
    /// </summary>
    public async Task CacheUserPaymentsAsync(Guid userId, List<Payment> payments, int page = 1, int pageSize = 20)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Caching user payments. CorrelationId: {CorrelationId}, UserId: {UserId}, Count: {Count}", 
            correlationId, userId, payments.Count);

        try
        {
            var key = GetUserPaymentsKey(userId, page, pageSize);
            var serializedPayments = JsonSerializer.Serialize(payments, GetJsonSerializerOptions());
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DefaultCacheExpirationMinutes)
            };

            await _cache.SetStringAsync(key, serializedPayments, options);

            _logger.LogInformation("User payments cached successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching user payments. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - caching failures shouldn't break the application
        }
    }

    /// <summary>
    /// Retrieves cached user payments
    /// </summary>
    public async Task<List<Payment>?> GetCachedUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving cached user payments. CorrelationId: {CorrelationId}, UserId: {UserId}, Page: {Page}, PageSize: {PageSize}", 
            correlationId, userId, page, pageSize);

        try
        {
            var key = GetUserPaymentsKey(userId, page, pageSize);
            var cachedPayments = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedPayments))
            {
                _logger.LogInformation("User payments not found in cache. CorrelationId: {CorrelationId}, Key: {Key}", 
                    correlationId, key);
                return null;
            }

            var payments = JsonSerializer.Deserialize<List<Payment>>(cachedPayments, GetJsonSerializerOptions());
            
            _logger.LogInformation("User payments retrieved from cache successfully. CorrelationId: {CorrelationId}, Key: {Key}, Count: {Count}", 
                correlationId, key, payments?.Count ?? 0);

            return payments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached user payments. CorrelationId: {CorrelationId}", correlationId);
            return null; // Return null on cache errors to fall back to database
        }
    }

    /// <summary>
    /// Removes user payments from cache
    /// </summary>
    private async Task RemoveUserPaymentsAsync(Guid userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Removing user payments from cache. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            // Remove all cached pages for this user
            // In a real implementation, you might want to track which pages are cached
            // For now, we'll remove common page combinations
            var pagesToRemove = new[] { 1, 2, 3, 4, 5 };
            var pageSizesToRemove = new[] { 10, 20, 50 };

            var removalTasks = new List<Task>();

            foreach (var page in pagesToRemove)
            {
                foreach (var pageSize in pageSizesToRemove)
                {
                    var key = GetUserPaymentsKey(userId, page, pageSize);
                    removalTasks.Add(_cache.RemoveAsync(key));
                }
            }

            await Task.WhenAll(removalTasks);

            _logger.LogInformation("User payments removed from cache successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user payments from cache. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - cache removal failures shouldn't break the application
        }
    }

    /// <summary>
    /// Caches payment statistics
    /// </summary>
    public async Task CachePaymentStatsAsync(Guid userId, object stats, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Caching payment statistics. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            var key = GetPaymentStatsKey(userId, fromDate, toDate);
            var serializedStats = JsonSerializer.Serialize(stats, GetJsonSerializerOptions());
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DefaultCacheExpirationMinutes)
            };

            await _cache.SetStringAsync(key, serializedStats, options);

            _logger.LogInformation("Payment statistics cached successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching payment statistics. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - caching failures shouldn't break the application
        }
    }

    /// <summary>
    /// Retrieves cached payment statistics
    /// </summary>
    public async Task<T?> GetCachedPaymentStatsAsync<T>(Guid userId, DateTime? fromDate = null, DateTime? toDate = null) where T : class
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrieving cached payment statistics. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            var key = GetPaymentStatsKey(userId, fromDate, toDate);
            var cachedStats = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedStats))
            {
                _logger.LogInformation("Payment statistics not found in cache. CorrelationId: {CorrelationId}, Key: {Key}", 
                    correlationId, key);
                return null;
            }

            var stats = JsonSerializer.Deserialize<T>(cachedStats, GetJsonSerializerOptions());
            
            _logger.LogInformation("Payment statistics retrieved from cache successfully. CorrelationId: {CorrelationId}, Key: {Key}", 
                correlationId, key);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached payment statistics. CorrelationId: {CorrelationId}", correlationId);
            return null; // Return null on cache errors to fall back to database
        }
    }

    /// <summary>
    /// Removes payment statistics from cache
    /// </summary>
    private async Task RemovePaymentStatsAsync(Guid userId)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Removing payment statistics from cache. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            // Remove stats for common date ranges
            var dateRangesToRemove = new (DateTime?, DateTime?)[]
            {
                (null, null), // All time
                (DateTime.UtcNow.Date.AddDays(-30), null), // Last 30 days
                (DateTime.UtcNow.Date.AddDays(-7), null), // Last 7 days
                (DateTime.UtcNow.Date, null) // Today
            };

            var removalTasks = new List<Task>();

            foreach (var (fromDate, toDate) in dateRangesToRemove)
            {
                var key = GetPaymentStatsKey(userId, fromDate, toDate);
                removalTasks.Add(_cache.RemoveAsync(key));
            }

            await Task.WhenAll(removalTasks);

            _logger.LogInformation("Payment statistics removed from cache successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing payment statistics from cache. CorrelationId: {CorrelationId}", correlationId);
            // Don't throw - cache removal failures shouldn't break the application
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Gets the cache key for a payment
    /// </summary>
    private static string GetPaymentKey(Guid paymentId) => $"{PaymentKeyPrefix}{paymentId}";

    /// <summary>
    /// Gets the cache key for a payment method
    /// </summary>
    private static string GetPaymentMethodKey(Guid paymentMethodId) => $"{PaymentMethodKeyPrefix}{paymentMethodId}";

    /// <summary>
    /// Gets the cache key for a payment gateway
    /// </summary>
    private static string GetGatewayKey(Guid gatewayId) => $"{GatewayKeyPrefix}{gatewayId}";

    /// <summary>
    /// Gets the cache key for user payments
    /// </summary>
    private static string GetUserPaymentsKey(Guid userId, int page, int pageSize) => 
        $"{UserPaymentsKeyPrefix}{userId}:page_{page}:size_{pageSize}";

    /// <summary>
    /// Gets the cache key for payment statistics
    /// </summary>
    private static string GetPaymentStatsKey(Guid userId, DateTime? fromDate, DateTime? toDate)
    {
        var fromDateStr = fromDate?.ToString("yyyy-MM-dd") ?? "all";
        var toDateStr = toDate?.ToString("yyyy-MM-dd") ?? "all";
        return $"{PaymentStatsKeyPrefix}{userId}:from_{fromDateStr}:to_{toDateStr}";
    }

    /// <summary>
    /// Gets JSON serializer options for consistent serialization
    /// </summary>
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #endregion
}
