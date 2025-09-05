using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Interfaces;
using Mapster;
using PaymentGatewayService.Utils;
using EntityPaymentStatus = PaymentGatewayService.Data.Entities.PaymentStatus;
using ModelPaymentStatus = PaymentGatewayService.Models.PaymentStatus;

namespace PaymentGatewayService.Services;

/// <summary>
/// Main payment service coordinator - delegates to specialized services
/// Follows the coordinator pattern established in NotificationService
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentProcessingService _processingService;
    private readonly IPaymentValidationService _validationService;
    private readonly IGatewayIntegrationService _gatewayService;
    private readonly IPaymentCacheService _cacheService;
    private readonly ILogger<PaymentService> _logger;
    private readonly IPaymentRouter _paymentRouter;

    public PaymentService(
        PaymentDbContext context,
        IPaymentProcessingService processingService,
        IPaymentValidationService validationService,
        IGatewayIntegrationService gatewayService,
        IPaymentCacheService cacheService,
        ILogger<PaymentService> logger,
        IPaymentRouter paymentRouter)
        {
            _context = context;
            _processingService = processingService;
            _validationService = validationService;
            _gatewayService = gatewayService;
            _cacheService = cacheService;
            _logger = logger;
            _paymentRouter = paymentRouter;
        }

    /// <summary>
    /// Processes a payment transaction through the specialized processing pipeline
    /// </summary>
    public async Task<PaymentResponse> ProcessPaymentAsync(global::PaymentGatewayService.Models.Requests.ProcessPaymentRequest request)
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Processing payment for user {UserId}, amount {Amount} {Currency} (CorrelationId: {CorrelationId})",
            request.UserId, request.Amount, request.Currency, correlationId);

        try
        {
            // Step 1: Validate payment request
            await _validationService.ValidatePaymentRequestAsync(request);

            // Step 2: Process payment through specialized service (creates Payment record and attempts)
            var payment = await _processingService.ProcessPaymentAsync(request, correlationId);

                // If caller provided an explicit payment method selection (or metadata hint), route via PaymentRouter
                var methodHint = request.Metadata != null && request.Metadata.TryGetValue("method", out var mh) ? mh?.ToString() :
                             request.Metadata != null && request.Metadata.TryGetValue("paymentMethod", out var pmh) ? pmh?.ToString() : null;

                if (request.PaymentMethodId.HasValue || !string.IsNullOrWhiteSpace(methodHint))
                {
                    // Map to router request (use available PaymentRouterRequest fields)
                    var routerReq = new global::PaymentGatewayService.Models.PaymentRouterRequest
                    {
                        UserId = payment.UserId.ToString(),
                        Currency = request.Currency,
                        Amount = request.Amount,
                        PaymentMethod = !string.IsNullOrWhiteSpace(methodHint) ? methodHint! : "Fiat",
                        Metadata = request.Metadata != null ? request.Metadata.ToDictionary(k => k.Key, k => (object)(k.Value?.ToString() ?? string.Empty)) : new Dictionary<string, object>(),
                        ReturnUrl = request.ReturnUrl ?? string.Empty,
                        CancelUrl = request.CancelUrl ?? string.Empty
                    };

                    var routerResp = await _paymentRouter.RouteAsync(routerReq);

                    // Update payment status based on router result (routerResp is PaymentResult)
                    if (routerResp.Success)
                    {
                        await _processingService.UpdatePaymentStatusAsync(payment.Id, EntityPaymentStatus.Completed, routerResp.TransactionId);
                    }
                    else
                    {
                        await _processingService.UpdatePaymentStatusAsync(payment.Id, EntityPaymentStatus.Failed, routerResp.TransactionId);
                    }

                    // Cache payment data
                    await _cacheService.CachePaymentAsync(payment);

                    // Return a mapped PaymentResponse (use existing payment record)
                    return payment.Adapt<PaymentResponse>();
                }

            // No explicit method selection - continue legacy gateway flow
            // Step 3: Execute payment through gateway integration
            var result = await _gatewayService.ExecutePaymentAsync(payment);

            // Step 4: Update payment status based on gateway result
            await _processingService.UpdatePaymentStatusAsync(payment.Id, result.Status, result.GatewayTransactionId);

            // Step 5: Cache payment data for performance
            await _cacheService.CachePaymentAsync(payment);

            _logger.LogInformation("Payment processed successfully: {PaymentId} (CorrelationId: {CorrelationId})",
                payment.Id, correlationId);

            return payment.Adapt<PaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed for user {UserId} (CorrelationId: {CorrelationId})",
                request.UserId, correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets a payment by ID with user authorization
    /// </summary>
    public async Task<PaymentResponse?> GetPaymentAsync(Guid paymentId, Guid userId)
    {
        _logger.LogInformation("Getting payment {PaymentId} for user {UserId}", paymentId, userId);

        try
        {
            // Try cache first for performance
            var cachedPayment = await _cacheService.GetPaymentAsync(paymentId);
            if (cachedPayment != null && cachedPayment.UserId == userId)
            {
                return cachedPayment.Adapt<PaymentResponse>();
            }

            // Fallback to database
            var payment = await _context.Payments
                .Include(p => p.PaymentAttempts)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found for user {UserId}", paymentId, userId);
                return null;
            }

            // Cache for future requests
            await _cacheService.CachePaymentAsync(payment);

            return payment.Adapt<PaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId} for user {UserId}", paymentId, userId);
            throw;
        }
    }

    /// <summary>
    /// Gets payments for a user with filtering and pagination
    /// </summary>
    public async Task<PaginatedPaymentResponse> GetPaymentsAsync(GetPaymentsRequest request)
    {
        _logger.LogInformation("Getting payments for user {UserId}, page {Page}, size {PageSize}",
            request.UserId, request.Page, request.PageSize);

        try
        {
            var query = _context.Payments
                .Where(p => p.UserId == request.UserId)
                .Include(p => p.PaymentAttempts)
                .AsQueryable();

            // Apply filters
            if (request.Status.HasValue)
                query = query.Where(p => p.Status == StatusMapper.ToEntity(request.Status.Value));

            if (request.Type.HasValue)
                query = query.Where(p => p.Type == StatusMapper.ToEntity(request.Type.Value));

            if (request.FromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(p => p.CreatedAt <= request.ToDate.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var paymentResponses = payments.Adapt<List<PaymentResponse>>();

            return new PaginatedPaymentResponse
            {
                Payments = paymentResponses,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for user {UserId}", request.UserId);
            throw;
        }
    }

    /// <summary>
    /// Updates the status of a payment
    /// </summary>
    public async Task<PaymentResponse> UpdatePaymentStatusAsync(UpdatePaymentStatusRequest request)
    {
        _logger.LogInformation("Updating payment {PaymentId} status to {Status}", request.PaymentId, request.Status);

        try
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentAttempts)
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

            if (payment == null)
                throw new InvalidOperationException($"Payment {request.PaymentId} not found");

            // Delegate to processing service for status update logic
            await _processingService.UpdatePaymentStatusAsync(request.PaymentId, (Data.Entities.PaymentStatus)request.Status, request.GatewayTransactionId);

            // Refresh payment data
            await _context.Entry(payment).ReloadAsync();

            // Update cache
            await _cacheService.CachePaymentAsync(payment);

            _logger.LogInformation("Payment {PaymentId} status updated to {Status}", request.PaymentId, request.Status);

            return payment.Adapt<PaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId} status", request.PaymentId);
            throw;
        }
    }

    /// <summary>
    /// Cancels a pending payment
    /// </summary>
    public async Task<PaymentResponse> CancelPaymentAsync(Guid paymentId, Guid userId, string reason)
    {
        _logger.LogInformation("Cancelling payment {PaymentId} for user {UserId}, reason: {Reason}",
            paymentId, userId, reason);

        try
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

            if (payment == null)
                throw new InvalidOperationException($"Payment {paymentId} not found for user {userId}");

            if (payment.Status != EntityPaymentStatus.Pending)
                throw new InvalidOperationException($"Cannot cancel payment {paymentId} with status {payment.Status}");

            // Delegate to processing service for cancellation logic
            await _processingService.CancelPaymentAsync(paymentId, reason);

            // Refresh payment data
            await _context.Entry(payment).ReloadAsync();

            // Update cache
            await _cacheService.CachePaymentAsync(payment);

            _logger.LogInformation("Payment {PaymentId} cancelled successfully", paymentId);

            return payment.Adapt<PaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
            throw;
        }
    }

    /// <summary>
    /// Gets payment statistics for a user
    /// </summary>
    public async Task<PaymentStatisticsResponse> GetPaymentStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        _logger.LogInformation("Getting payment statistics for user {UserId}", userId);

        try
        {
            // Delegate to analytics service for statistics calculation
            return await _processingService.GetPaymentStatisticsAsync(userId, fromDate, toDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Retries a failed payment
    /// </summary>
    public async Task<PaymentResponse> RetryPaymentAsync(Guid paymentId, Guid userId)
    {
        _logger.LogInformation("Retrying payment {PaymentId} for user {UserId}", paymentId, userId);

        try
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentAttempts)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

            if (payment == null)
                throw new InvalidOperationException($"Payment {paymentId} not found for user {userId}");

            if (payment.Status != EntityPaymentStatus.Failed)
                throw new InvalidOperationException($"Cannot retry payment {paymentId} with status {payment.Status}");

            // Delegate to processing service for retry logic
            var result = await _processingService.RetryPaymentAsync(payment);

            // Update cache
            await _cacheService.CachePaymentAsync(payment);

            _logger.LogInformation("Payment {PaymentId} retry completed with status {Status}", paymentId, result.Status);

            return payment.Adapt<PaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment {PaymentId}", paymentId);
            throw;
        }
    }

    /// <summary>
    /// Gets the payment history for a specific payment
    /// </summary>
    public async Task<PaymentHistoryResponse> GetPaymentHistoryAsync(Guid paymentId, Guid userId)
    {
        _logger.LogInformation("Getting payment history for {PaymentId}, user {UserId}", paymentId, userId);

        try
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentAttempts)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

            if (payment == null)
                throw new InvalidOperationException($"Payment {paymentId} not found for user {userId}");

                var history = new PaymentHistoryResponse
                {
                    PaymentId = payment.Id,
                    UserId = payment.UserId,
                    CurrentStatus = StatusMapper.ToModel(payment.Status),
                    PaymentAttempts = payment.PaymentAttempts.Adapt<List<PaymentAttemptResponse>>(),
                    Refunds = new List<RefundResponse>(), // TODO: Add Refunds navigation property to Payment entity
                    CreatedAt = payment.CreatedAt,
                    UpdatedAt = payment.UpdatedAt.GetValueOrDefault()
                };

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for {PaymentId}", paymentId);
            throw;
        }
    }
}
