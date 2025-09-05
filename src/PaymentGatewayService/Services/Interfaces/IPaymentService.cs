using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Main payment service interface - acts as coordinator for all payment operations
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Processes a payment transaction
    /// </summary>
    /// <param name="request">The payment processing request</param>
    /// <returns>The payment response</returns>
    Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);

    /// <summary>
    /// Gets a payment by ID
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    /// <param name="userId">The user ID (for authorization)</param>
    /// <returns>The payment response</returns>
    Task<PaymentResponse?> GetPaymentAsync(Guid paymentId, Guid userId);

    /// <summary>
    /// Gets payments for a user with filtering and pagination
    /// </summary>
    /// <param name="request">The get payments request</param>
    /// <returns>The paginated payment response</returns>
    Task<PaginatedPaymentResponse> GetPaymentsAsync(GetPaymentsRequest request);

    /// <summary>
    /// Updates the status of a payment
    /// </summary>
    /// <param name="request">The update payment status request</param>
    /// <returns>The updated payment response</returns>
    Task<PaymentResponse> UpdatePaymentStatusAsync(UpdatePaymentStatusRequest request);

    /// <summary>
    /// Cancels a pending payment
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    /// <param name="userId">The user ID (for authorization)</param>
    /// <param name="reason">The cancellation reason</param>
    /// <returns>The updated payment response</returns>
    Task<PaymentResponse> CancelPaymentAsync(Guid paymentId, Guid userId, string reason);

    /// <summary>
    /// Gets payment statistics for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="fromDate">The start date for statistics</param>
    /// <param name="toDate">The end date for statistics</param>
    /// <returns>The payment statistics</returns>
    Task<PaymentStatisticsResponse> GetPaymentStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Retries a failed payment
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    /// <param name="userId">The user ID (for authorization)</param>
    /// <returns>The updated payment response</returns>
    Task<PaymentResponse> RetryPaymentAsync(Guid paymentId, Guid userId);

    /// <summary>
    /// Gets the payment history for a specific payment
    /// </summary>
    /// <param name="paymentId">The payment ID</param>
    /// <param name="userId">The user ID (for authorization)</param>
    /// <returns>The payment history response</returns>
    Task<PaymentHistoryResponse> GetPaymentHistoryAsync(Guid paymentId, Guid userId);
}
