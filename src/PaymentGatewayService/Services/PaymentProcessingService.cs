using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Interfaces;
using PaymentGatewayService.Utils;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Refit;

namespace PaymentGatewayService.Services;

/// <summary>
/// Payment processing service implementing IPaymentProcessingService
/// Handles core payment processing logic, status management, retry logic, and analytics
/// </summary>
public class PaymentProcessingService : IPaymentProcessingService
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentProcessingService> _logger;
    private readonly IGatewayIntegrationService _gatewayIntegration;
    private readonly IPaymentCacheService _cacheService;
    private readonly IPaymentValidationService _validationService;
    private readonly IFeeServiceApi _feeServiceApi;
    private readonly IQuantumLedgerHubClient? _ledgerClient;
    private readonly IRefundService? _refundService;

    public PaymentProcessingService(
        PaymentDbContext context,
        ILogger<PaymentProcessingService> logger,
        IGatewayIntegrationService gatewayIntegration,
        IPaymentCacheService cacheService,
        IPaymentValidationService validationService,
        IFeeServiceApi feeServiceApi,
        IQuantumLedgerHubClient? ledgerClient = null,
        IRefundService? refundService = null)
    {
        _context = context;
        _logger = logger;
        _gatewayIntegration = gatewayIntegration;
        _cacheService = cacheService;
        _validationService = validationService;
        _feeServiceApi = feeServiceApi;
        _ledgerClient = ledgerClient;
        _refundService = refundService;
    }

    /// <summary>
    /// Processes a payment request and creates a payment entity
    /// </summary>
    public async Task<Payment> ProcessPaymentAsync(ProcessPaymentRequest request, Guid correlationId)
    {
        _logger.LogInformation("Processing payment request. CorrelationId: {CorrelationId}, UserId: {UserId}, Amount: {Amount}, Currency: {Currency}", 
            correlationId, request.UserId, request.Amount, request.Currency);

        // Parse string user id (Request.UserId is a string in the Requests DTO)
        Guid? parsedUserId = null;
        if (!string.IsNullOrWhiteSpace(request.UserId) && Guid.TryParse(request.UserId, out var _parsedUserId))
        {
            parsedUserId = _parsedUserId;
        }

        try
        {
            // For deposit transactions, check for deposit code validation first
            if (request.Type == global::PaymentGatewayService.Models.PaymentType.Deposit)
            {
                try
                {
                    // Validate the payment request (including deposit code validation)
                    await _validationService.ValidatePaymentRequestAsync(request);
                }
                catch (ValidationException ex)
                {
                    // If deposit validation fails, trigger rejection flow
                    _logger.LogWarning("Deposit validation failed, initiating rejection flow. CorrelationId: {CorrelationId}, Reason: {Reason}", 
                        correlationId, ex.Message);
                    
                    return await ProcessDepositRejectionAsync(request, correlationId, ex.Message);
                }
            }
            else
            {
                // For non-deposit transactions, use standard validation
                await _validationService.ValidatePaymentRequestAsync(request);
            }

            // Get payment gateway (preferred or best available)
            PaymentGateway? gateway = null;
            if (request.PreferredGatewayId.HasValue)
            {
                gateway = await _context.PaymentGateways
                    .FirstOrDefaultAsync(g => g.Id == request.PreferredGatewayId.Value && g.IsActive);
            }

            if (gateway == null)
            {
                // Find best gateway for this payment
                var gateways = await _context.PaymentGateways
                    .AsNoTracking()
                    .Where(g => g.IsActive && 
                               g.SupportedCurrencies != null && 
                               g.SupportedCurrencies.Contains(request.Currency))
                    .OrderBy(g => g.Priority)
                    .ToListAsync();

                gateway = gateways.FirstOrDefault();
            }

            if (gateway == null)
            {
                throw new InvalidOperationException($"No active gateway found for currency {request.Currency}");
            }

            // Get payment method if specified
            PaymentMethod? paymentMethod = null;
            if (request.PaymentMethodId.HasValue && !parsedUserId.HasValue)
            {
                // No user id provided / parsed - cannot resolve payment method for user
            }
            else if (request.PaymentMethodId.HasValue && parsedUserId.HasValue)
            {
                paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId.Value && 
                                              pm.UserId == parsedUserId.Value && 
                                              pm.IsActive);

                if (paymentMethod == null)
                {
                    throw new InvalidOperationException($"Payment method with ID {request.PaymentMethodId.Value} not found or inactive");
                }

                // Validate payment method for user (use parsed Guid)
                await _validationService.ValidatePaymentMethodAsync(parsedUserId.Value, request.PaymentMethodId.Value);
            }

            // Calculate fees
            var feeAmount = CalculateFeeAmount(request.Amount, gateway);
            var netAmount = request.Amount - feeAmount;

            // Create payment entity
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = parsedUserId ?? Guid.Empty,
                    PaymentGatewayId = gateway.Id,
                    PaymentMethodId = paymentMethod?.Id,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Type = StatusMapper.ToEntity(request.Type),
                    Status = PaymentStatus.Pending,
                    Description = request.Description,
                    FeeAmount = feeAmount,
                    NetAmount = netAmount,
                    ClientIpAddress = request.ClientIpAddress,
                    UserAgent = request.UserAgent,
                    Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : "{}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

            // Set expiry date (24 hours from creation for pending payments)
            payment.ExpiresAt = DateTime.UtcNow.AddHours(24);

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Cache the payment
            await _cacheService.CachePaymentAsync(payment);

            // Record in QuantumLedger.Hub if available
            if (_ledgerClient != null)
            {
                // Record payment transaction
                var paymentLedgerEntry = new PaymentLedgerEntry
                {
                PaymentId = payment.Id,
                    UserId = payment.UserId.ToString(),
                    PaymentType = payment.Type.ToString(),
                    Amount = payment.Amount,
                    FeeAmount = payment.FeeAmount ?? 0m,
                    NetAmount = payment.NetAmount ?? 0m,
                    Currency = payment.Currency,
                    Status = payment.Status.ToString(),
                    DepositCode = request.DepositCode,
                    CreatedAt = payment.CreatedAt,
                    Metadata = request.Metadata ?? new Dictionary<string, object>()
                };

                var transactionId = await _ledgerClient.RecordPaymentTransactionAsync(paymentLedgerEntry);
                if (string.IsNullOrEmpty(transactionId))
                {
                    _logger.LogWarning("Failed to record payment in ledger, but local record created. PaymentId: {PaymentId}", payment.Id);
                }

                // If deposit code was used, record its usage
                if (!string.IsNullOrEmpty(request.DepositCode))
                {
                    var usageLedgerEntry = new DepositCodeUsageLedgerEntry
                    {
                    DepositCode = request.DepositCode,
                    PaymentId = payment.Id,
                    UserId = payment.UserId.ToString(),
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    UsedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                        {
                            ["correlationId"] = correlationId.ToString(),
                            ["paymentType"] = payment.Type.ToString()
                        }
                    };

                    var usageSuccess = await _ledgerClient.RecordDepositCodeUsageAsync(usageLedgerEntry);
                    if (!usageSuccess)
                    {
                        _logger.LogWarning("Failed to record deposit code usage in ledger. DepositCode: {DepositCode}", request.DepositCode);
                    }

                    // Update deposit code status in database
                    var depositCode = await _context.DepositCodes
                        .FirstOrDefaultAsync(dc => dc.Code.ToUpper() == request.DepositCode.ToUpper());
                    
                    if (depositCode != null)
                    {
                        depositCode.Status = DepositCodeStatus.Used;
                        depositCode.UsedAt = DateTime.UtcNow;
                        depositCode.PaymentId = payment.Id;
                        depositCode.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            _logger.LogInformation("Payment created successfully. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}", 
                correlationId, payment.Id);

            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment request. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Updates the status of a payment
    /// </summary>
    public async Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, string? gatewayTransactionId = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating payment status. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, NewStatus: {NewStatus}", 
            correlationId, paymentId, status);

        try
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException($"Payment with ID {paymentId} not found");
            }

            var oldStatus = payment.Status;
            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(gatewayTransactionId))
            {
                payment.GatewayTransactionId = gatewayTransactionId;
            }

            // Set completion timestamp for completed payments
            if (status == PaymentStatus.Completed && payment.CompletedAt == null)
            {
                payment.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemovePaymentAsync(paymentId);

            _logger.LogInformation("Payment status updated successfully. CorrelationId: {CorrelationId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}", 
                correlationId, oldStatus, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Cancels a payment
    /// </summary>
    public async Task CancelPaymentAsync(Guid paymentId, string reason)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Cancelling payment. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, Reason: {Reason}", 
            correlationId, paymentId, reason);

        try
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException($"Payment with ID {paymentId} not found");
            }

            // Check if payment can be cancelled
            if (payment.Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel a completed payment");
            }

            if (payment.Status == PaymentStatus.Cancelled)
            {
                throw new InvalidOperationException("Payment is already cancelled");
            }

            // Update payment status
            payment.Status = PaymentStatus.Cancelled;
            payment.UpdatedAt = DateTime.UtcNow;

            // Update metadata with cancellation reason
            var metadata = string.IsNullOrEmpty(payment.Metadata) || payment.Metadata == "{}" 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata) ?? new Dictionary<string, object>();

            metadata["cancellation_reason"] = reason;
            metadata["cancelled_at"] = DateTime.UtcNow.ToString("O");
            payment.Metadata = JsonSerializer.Serialize(metadata);

            await _context.SaveChangesAsync();

            // Clear cache
            await _cacheService.RemovePaymentAsync(paymentId);

            _logger.LogInformation("Payment cancelled successfully. CorrelationId: {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Retries a failed payment
    /// </summary>
    public async Task<PaymentAttempt> RetryPaymentAsync(Payment payment)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Retrying payment. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}", 
            correlationId, payment.Id);

        try
        {
            // Check if payment can be retried
            if (payment.Status != PaymentStatus.Failed)
            {
                throw new InvalidOperationException("Only failed payments can be retried");
            }

            // Check retry limits
            var attemptCount = await _context.PaymentAttempts
                .CountAsync(pa => pa.PaymentId == payment.Id);

            if (attemptCount >= 3) // Maximum 3 attempts
            {
                throw new InvalidOperationException("Maximum retry attempts exceeded");
            }

            // Check if payment has expired
            if (payment.ExpiresAt.HasValue && payment.ExpiresAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Payment has expired and cannot be retried");
            }

            // Create new payment attempt
            var attempt = new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                AttemptNumber = await GetNextAttemptNumberAsync(payment.Id),
                Status = PaymentAttemptStatus.Processing,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CreatedAt = DateTime.UtcNow
            };

            _context.PaymentAttempts.Add(attempt);

            // Reset payment status to pending
            payment.Status = PaymentStatus.Pending;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            try
            {
                // Execute payment through gateway
                var startTime = DateTime.UtcNow;
                var executionResult = await _gatewayIntegration.ExecutePaymentAsync(payment);
                var endTime = DateTime.UtcNow;

                // Update attempt with results
                attempt.Status = executionResult.Status == PaymentStatus.Completed 
                    ? PaymentAttemptStatus.Succeeded 
                    : PaymentAttemptStatus.Failed;
                attempt.GatewayTransactionId = executionResult.GatewayTransactionId;
                attempt.ErrorMessage = executionResult.ErrorMessage;
                attempt.ProcessedAt = endTime;
                attempt.ProcessingTimeMs = (int)(endTime - startTime).TotalMilliseconds;

                // Update payment status
                payment.Status = executionResult.Status;
                payment.GatewayTransactionId = executionResult.GatewayTransactionId;
                payment.UpdatedAt = DateTime.UtcNow;

                if (executionResult.Status == PaymentStatus.Completed)
                {
                    payment.CompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Clear cache
                await _cacheService.RemovePaymentAsync(payment.Id);

                _logger.LogInformation("Payment retry completed. CorrelationId: {CorrelationId}, Status: {Status}", 
                    correlationId, payment.Status);

                return attempt;
            }
            catch (Exception gatewayEx)
            {
                // Update attempt as failed
                attempt.Status = PaymentAttemptStatus.Failed;
                attempt.ErrorMessage = gatewayEx.Message;
                attempt.ProcessedAt = DateTime.UtcNow;

                // Update payment status
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogError(gatewayEx, "Gateway execution failed during retry. CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Gets payment statistics for a user
    /// </summary>
    public async Task<PaymentStatisticsResponse> GetPaymentStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Getting payment statistics. CorrelationId: {CorrelationId}, UserId: {UserId}", 
            correlationId, userId);

        try
        {
            var query = _context.Payments
                .AsNoTracking()
                .Where(p => p.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedAt <= toDate.Value);

            var payments = await query.ToListAsync();

            var totalPayments = payments.Count;
            var totalAmount = payments.Sum(p => p.Amount);
            var totalFees = payments.Sum(p => p.FeeAmount ?? 0m);
            var successfulPayments = payments.Count(p => p.Status == PaymentStatus.Completed);
            var failedPayments = payments.Count(p => p.Status == PaymentStatus.Failed);
            var pendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing);
            var averageAmount = totalPayments > 0 ? totalAmount / totalPayments : 0m;

            // Currency breakdown
            var currencyBreakdown = payments
                .GroupBy(p => p.Currency)
                .Select(g => new CurrencyStatistics
                {
                    Currency = g.Key,
                    TotalAmount = g.Sum(p => p.Amount),
                    PaymentCount = g.Count(),
                    AverageAmount = g.Any() ? g.Average(p => p.Amount) : 0m
                })
                .ToList();

            // Type breakdown
            var typeBreakdown = payments
                .GroupBy(p => p.Type)
                .Select(g => new PaymentTypeStatistics
                {
                    Type = StatusMapper.ToModel(g.Key),
                    TotalAmount = g.Sum(p => p.Amount),
                    PaymentCount = g.Count(),
                    AverageAmount = g.Any() ? g.Average(p => p.Amount) : 0m
                })
                .ToList();

            _logger.LogInformation("Payment statistics retrieved successfully. CorrelationId: {CorrelationId}, TotalPayments: {TotalPayments}", 
                correlationId, totalPayments);

            return new PaymentStatisticsResponse
            {
                UserId = userId,
                TotalPayments = totalPayments,
                TotalAmount = totalAmount,
                TotalFees = totalFees,
                SuccessfulPayments = successfulPayments,
                FailedPayments = failedPayments,
                PendingPayments = pendingPayments,
                AveragePaymentAmount = averageAmount,
                CurrencyBreakdown = currencyBreakdown,
                TypeBreakdown = typeBreakdown,
                FromDate = fromDate,
                ToDate = toDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    /// <summary>
    /// Calculates fee amount based on gateway configuration
    /// </summary>
    private static decimal CalculateFeeAmount(decimal amount, PaymentGateway gateway)
    {
        var percentageFee = amount * (gateway.FeePercentage / 100);
        var totalFee = percentageFee + gateway.FixedFee;
        return Math.Round(totalFee, 2);
    }

    /// <summary>
    /// Gets the next attempt number for a payment
    /// </summary>
    private async Task<int> GetNextAttemptNumberAsync(Guid paymentId)
    {
        var maxAttemptNumber = await _context.PaymentAttempts
            .Where(pa => pa.PaymentId == paymentId)
            .MaxAsync(pa => (int?)pa.AttemptNumber) ?? 0;

        return maxAttemptNumber + 1;
    }

    /// <summary>
    /// Processes deposit rejection with fee deduction and return to sender
    /// </summary>
    private async Task<Payment> ProcessDepositRejectionAsync(ProcessPaymentRequest request, Guid correlationId, string rejectionReason)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Processing deposit rejection. CorrelationId: {CorrelationId}, Reason: {Reason}", 
            correlationId, rejectionReason);

        try
        {
            // Get default gateway for rejection processing
            var gateway = await _context.PaymentGateways
                .AsNoTracking()
                .Where(g => g.IsActive && 
                           g.SupportedCurrencies != null && 
                           g.SupportedCurrencies.Contains(request.Currency))
                .OrderBy(g => g.Priority)
                .FirstOrDefaultAsync();

            if (gateway == null)
            {
                throw new InvalidOperationException($"No active gateway found for currency {request.Currency}");
            }

            // Calculate rejection fees based on deposit type
            decimal rejectionFees = 0;
            decimal netAmountAfterFees = request.Amount;
            string feeBreakdown = "";

            if (IsCryptocurrencyDeposit(gateway.GatewayType))
            {
                // Calculate crypto rejection fees
                var cryptoFeesRequest = new CryptoRejectionFeesRequest
                {
                    Amount = request.Amount,
                    Cryptocurrency = request.Currency,
                    Network = GetNetworkFromCurrency(request.Currency),
                    WalletAddress = GetWalletAddressFromMetadata(request.Metadata),
                    RejectionReason = rejectionReason
                };

                try
                {
                    var cryptoFeesResponse = await _feeServiceApi.CalculateCryptoRejectionFeesAsync(cryptoFeesRequest);
                    rejectionFees = cryptoFeesResponse.TotalFees;
                    netAmountAfterFees = cryptoFeesResponse.NetAmount;
                    feeBreakdown = $"Network Fees: {cryptoFeesResponse.NetworkFees:C}, Internal Fees: {cryptoFeesResponse.InternalFees:C}";
                }
                catch (Exception feeEx)
                {
                    _logger.LogWarning(feeEx, "Failed to calculate crypto rejection fees, using default. CorrelationId: {CorrelationId}", correlationId);
                    rejectionFees = CalculateDefaultCryptoRejectionFees(request.Amount, request.Currency);
                    netAmountAfterFees = request.Amount - rejectionFees;
                    feeBreakdown = $"Default crypto rejection fees: {rejectionFees:C}";
                }
            }
            else
            {
                // Calculate fiat rejection fees
                var fiatFeesRequest = new FiatRejectionFeesRequest
                {
                    Amount = request.Amount,
                    Currency = request.Currency,
                    GatewayType = gateway.GatewayType.ToString(),
                    PaymentMethodType = GetPaymentMethodTypeFromRequest(request),
                    RejectionReason = rejectionReason
                };

                try
                {
                    var fiatFeesResponse = await _feeServiceApi.CalculateFiatRejectionFeesAsync(fiatFeesRequest);
                    rejectionFees = fiatFeesResponse.TotalFees;
                    netAmountAfterFees = fiatFeesResponse.NetAmount;
                    feeBreakdown = $"Wire Fees: {fiatFeesResponse.WireFees:C}, Square Fees: {fiatFeesResponse.SquareFees:C}, Internal Fees: {fiatFeesResponse.InternalFees:C}";
                }
                catch (Exception feeEx)
                {
                    _logger.LogWarning(feeEx, "Failed to calculate fiat rejection fees, using default. CorrelationId: {CorrelationId}", correlationId);
                    rejectionFees = CalculateDefaultFiatRejectionFees(request.Amount, gateway.GatewayType);
                    netAmountAfterFees = request.Amount - rejectionFees;
                    feeBreakdown = $"Default fiat rejection fees: {rejectionFees:C}";
                }
            }

            // Ensure net amount is not negative
            if (netAmountAfterFees < 0)
            {
                _logger.LogWarning("Rejection fees exceed deposit amount. CorrelationId: {CorrelationId}, Amount: {Amount}, Fees: {Fees}", 
                    correlationId, request.Amount, rejectionFees);
                netAmountAfterFees = 0;
            }

            // Create rejected payment entity
                var rejectedPayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = (Guid.TryParse(request.UserId, out var _parsedUid) ? _parsedUid : Guid.Empty),
                    PaymentGatewayId = gateway.Id,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Type = PaymentType.Deposit,
                    Status = PaymentStatus.Failed, // Mark as failed due to rejection
                    Description = $"REJECTED DEPOSIT: {request.Description ?? "No description"} - {rejectionReason}",
                FeeAmount = rejectionFees,
                NetAmount = netAmountAfterFees,
                ClientIpAddress = request.ClientIpAddress,
                UserAgent = request.UserAgent,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["original_request"] = request.Metadata ?? new Dictionary<string, object>(),
                    ["rejection_reason"] = rejectionReason,
                    ["rejection_fees"] = rejectionFees,
                    ["fee_breakdown"] = feeBreakdown,
                    ["net_amount_after_fees"] = netAmountAfterFees,
                    ["rejected_at"] = DateTime.UtcNow.ToString("O"),
                    ["correlation_id"] = correlationId.ToString(),
                    ["deposit_code"] = request.DepositCode ?? "MISSING",
                    ["auto_rejected"] = true
                }),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CompletedAt = null, // Not completed since it's rejected
                ExpiresAt = DateTime.UtcNow.AddHours(1) // Short expiry for rejected payments
            };

            _context.Payments.Add(rejectedPayment);
            await _context.SaveChangesAsync();

            // Record rejection in QuantumLedger.Hub if available
            if (_ledgerClient != null && !string.IsNullOrEmpty(request.DepositCode))
            {
                var rejectionLedgerEntry = new DepositCodeRejectionLedgerEntry
                {
                    DepositCode = request.DepositCode,
                    PaymentId = rejectedPayment.Id,
                    RejectionReason = rejectionReason,
                    RefundAmount = netAmountAfterFees,
                    FeeAmount = rejectionFees,
                    Currency = request.Currency,
                    RejectedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["correlationId"] = correlationId.ToString(),
                        ["feeBreakdown"] = feeBreakdown,
                        ["originalAmount"] = request.Amount,
                        ["gatewayType"] = gateway.GatewayType.ToString()
                    }
                };

                var ledgerSuccess = await _ledgerClient.RecordDepositCodeRejectionAsync(rejectionLedgerEntry);
                if (!ledgerSuccess)
                {
                    _logger.LogWarning("Failed to record deposit rejection in ledger. DepositCode: {DepositCode}", request.DepositCode);
                }
            }

            // Log rejection details for monitoring and audit
            _logger.LogWarning("Deposit rejected and processed for return. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, OriginalAmount: {OriginalAmount}, RejectionFees: {RejectionFees}, NetReturnAmount: {NetReturnAmount}, Reason: {Reason}", 
                correlationId, rejectedPayment.Id, request.Amount, rejectionFees, netAmountAfterFees, rejectionReason);

            // Trigger actual return to sender process if refund service is available
            if (_refundService != null && netAmountAfterFees > 0)
            {
                try
                {
                    var returnToSenderResult = await _refundService.ProcessReturnToSenderAsync(
                        rejectedPayment.Id, 
                        netAmountAfterFees, 
                        rejectionReason);

                    if (returnToSenderResult.Success)
                    {
                        _logger.LogInformation("Return to sender completed successfully. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, TransactionId: {TransactionId}, ReturnAmount: {ReturnAmount}", 
                            correlationId, rejectedPayment.Id, returnToSenderResult.TransactionId, netAmountAfterFees);
                        
                        // Update payment metadata with refund information
                        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(rejectedPayment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                        metadata["refund_transaction_id"] = returnToSenderResult.TransactionId ?? "N/A";
                        metadata["refund_processed_at"] = returnToSenderResult.ProcessedAt.ToString("O");
                        rejectedPayment.Metadata = JsonSerializer.Serialize(metadata);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogError("Return to sender failed. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, Error: {Error}", 
                            correlationId, rejectedPayment.Id, returnToSenderResult.ErrorMessage);
                    }
                }
                catch (Exception refundEx)
                {
                    _logger.LogError(refundEx, "Error processing return to sender. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}", 
                        correlationId, rejectedPayment.Id);
                }
            }
            else
            {
                _logger.LogInformation("Return to sender not processed. RefundService: {Available}, NetAmount: {NetAmount}", 
                    _refundService != null, netAmountAfterFees);
            }

            // Record successful rejection processing metrics (monitoring removed)
            var duration = DateTime.UtcNow - startTime;
            _logger.LogDebug("Deposit rejection processed. Duration: {Duration}, Reason: {Reason}, OriginalAmount: {OriginalAmount}, Fees: {Fees}, NetReturn: {NetReturn}", 
                duration, rejectionReason, request.Amount, rejectionFees, netAmountAfterFees);

            return rejectedPayment;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Error processing deposit rejection. CorrelationId: {CorrelationId}", correlationId);
            
            // Record failed rejection processing metrics (monitoring removed)
            _logger.LogDebug("Failed to process deposit rejection (monitoring removed). Reason: {Reason}, Duration: {Duration}", rejectionReason, duration);
            
            throw;
        }
    }

    /// <summary>
    /// Determines if a gateway type handles cryptocurrency deposits
    /// </summary>
    private static bool IsCryptocurrencyDeposit(PaymentGatewayType gatewayType)
    {
        return gatewayType == PaymentGatewayType.CryptoWallet || 
               gatewayType == PaymentGatewayType.Coinbase;
    }

    /// <summary>
    /// Gets network name from currency symbol
    /// </summary>
    private static string GetNetworkFromCurrency(string currency)
    {
        return currency.ToUpperInvariant() switch
        {
            "BTC" => "Bitcoin",
            "ETH" => "Ethereum",
            "USDC" => "Ethereum",
            "USDT" => "Ethereum",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Extracts wallet address from request metadata
    /// </summary>
    private static string GetWalletAddressFromMetadata(Dictionary<string, object>? metadata)
    {
        if (metadata != null && metadata.TryGetValue("wallet_address", out var address))
        {
            return address.ToString() ?? "";
        }
        return "";
    }

    /// <summary>
    /// Gets payment method type from request
    /// </summary>
    private static string GetPaymentMethodTypeFromRequest(ProcessPaymentRequest request)
    {
        // This would typically be determined from the payment method ID
        // For now, return a default based on common patterns
        return "BankTransfer";
    }

    /// <summary>
    /// Calculates default crypto rejection fees when FeeService is unavailable
    /// </summary>
    private static decimal CalculateDefaultCryptoRejectionFees(decimal amount, string currency)
    {
        // Default crypto fees: network fee + 1% internal processing fee
        var networkFee = currency.ToUpperInvariant() switch
        {
            "BTC" => 0.0005m, // ~$25 at $50k BTC
            "ETH" => 0.01m,   // ~$25 at $2.5k ETH
            "USDC" => 15m,    // ~$15 USDC
            "USDT" => 15m,    // ~$15 USDT
            _ => amount * 0.02m // 2% for unknown cryptos
        };

        var internalFee = amount * 0.01m; // 1% internal processing fee
        return networkFee + internalFee;
    }

    /// <summary>
    /// Calculates default fiat rejection fees when FeeService is unavailable
    /// </summary>
    private static decimal CalculateDefaultFiatRejectionFees(decimal amount, PaymentGatewayType gatewayType)
    {
        return gatewayType switch
        {
            PaymentGatewayType.BankTransfer => 25m, // $25 wire fee
            PaymentGatewayType.Square => amount * 0.029m + 0.30m, // Square's standard rate
            PaymentGatewayType.Stripe => amount * 0.029m + 0.30m, // Stripe's standard rate
            _ => amount * 0.03m // 3% default fee
        };
    }
}
