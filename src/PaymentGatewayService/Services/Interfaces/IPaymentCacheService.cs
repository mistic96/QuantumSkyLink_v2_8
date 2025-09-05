using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Payment cache service interface
/// Handles Redis caching for payment operations
/// </summary>
public interface IPaymentCacheService
{
    /// <summary>
    /// Caches a payment for performance optimization
    /// </summary>
    /// <param name="payment">The payment to cache</param>
    Task CachePaymentAsync(Payment payment);

    /// <summary>
    /// Gets a cached payment by ID
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    /// <returns>The cached payment or null if not found</returns>
    Task<Payment?> GetPaymentAsync(Guid paymentId);

    /// <summary>
    /// Removes a payment from cache
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    Task RemovePaymentAsync(Guid paymentId);

    /// <summary>
    /// Caches payment method data
    /// </summary>
    /// <param name="paymentMethod">The payment method to cache</param>
    Task CachePaymentMethodAsync(PaymentMethod paymentMethod);

    /// <summary>
    /// Gets a cached payment method by ID
    /// </summary>
    /// <param name="paymentMethodId">The payment method ID</param>
    /// <returns>The cached payment method or null if not found</returns>
    Task<PaymentMethod?> GetPaymentMethodAsync(Guid paymentMethodId);

    /// <summary>
    /// Caches gateway configuration
    /// </summary>
    /// <param name="gateway">The gateway to cache</param>
    Task CacheGatewayAsync(PaymentGateway gateway);

    /// <summary>
    /// Gets cached gateway configuration
    /// </summary>
    /// <param name="gatewayId">The gateway ID</param>
    /// <returns>The cached gateway or null if not found</returns>
    Task<PaymentGateway?> GetGatewayAsync(Guid gatewayId);

    /// <summary>
    /// Invalidates all payment-related cache for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    Task InvalidateUserCacheAsync(Guid userId);
}
