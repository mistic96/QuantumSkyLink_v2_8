using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Core payment processing service interface
/// Handles payment validation, creation, and status management
/// </summary>
public interface IPaymentProcessingService
{
    /// <summary>
    /// Processes a payment request and creates a payment entity
    /// </summary>
    /// <param name="request">The payment processing request</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <returns>The created payment entity</returns>
    Task<Payment> ProcessPaymentAsync(ProcessPaymentRequest request, Guid correlationId);

    /// <summary>
    /// Updates the status of a payment
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    /// <param name="status">The new payment status</param>
    /// <param name="gatewayTransactionId">The gateway transaction ID</param>
    Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string? gatewayTransactionId = null);

    /// <summary>
    /// Cancels a payment
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    /// <param name="reason">The cancellation reason</param>
    Task CancelPaymentAsync(Guid paymentId, string reason);

    /// <summary>
    /// Retries a failed payment
    /// </summary>
    /// <param name="payment">The payment to retry</param>
    /// <returns>The retry result</returns>
    Task<PaymentAttempt> RetryPaymentAsync(Payment payment);

    /// <summary>
    /// Gets payment statistics for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="fromDate">The start date for statistics</param>
    /// <param name="toDate">The end date for statistics</param>
    /// <returns>The payment statistics</returns>
    Task<PaymentStatisticsResponse> GetPaymentStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
}
