using Microsoft.Extensions.Logging;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Models.PIXBrazil;
using PaymentGatewayService.Models.DotsDev;
using PaymentGatewayService.Models.MoonPay;
using PaymentGatewayService.Models.Coinbase;
using PaymentGatewayService.Services.Interfaces;
using PaymentGatewayService.Services.Integrations;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PaymentGatewayService.Models.Square;
using PaymentGatewayService.Utils;
using ModelPaymentStatus = PaymentGatewayService.Models.PaymentStatus;
using EntityPaymentStatus = PaymentGatewayService.Data.Entities.PaymentStatus;

namespace PaymentGatewayService.Services;

/// <summary>
/// Gateway integration service implementing IGatewayIntegrationService
/// Handles Square and Stripe SDK integration, gateway-specific payment execution, and health monitoring
/// </summary>
public class GatewayIntegrationService : IGatewayIntegrationService
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<GatewayIntegrationService> _logger;
    private readonly IPaymentCacheService _cacheService;
    private readonly IServiceProvider _serviceProvider;

    public GatewayIntegrationService(
        PaymentDbContext context,
        ILogger<GatewayIntegrationService> logger,
        IPaymentCacheService cacheService,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Executes a payment through the appropriate gateway
    /// </summary>
    public async Task<GatewayExecutionResult> ExecutePaymentAsync(Payment payment)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Executing payment through gateway. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, GatewayId: {GatewayId}", 
            correlationId, payment.Id, payment.PaymentGatewayId);

        try
        {
            // Load gateway entity from PaymentGatewayId if needed
            PaymentGateway? gateway = null;
            if (payment.PaymentGatewayId.HasValue)
            {
                var gatewayId = payment.PaymentGatewayId.Value;
                gateway = await _context.PaymentGateways
                    .FirstOrDefaultAsync(g => g.Id == gatewayId);
            }
            
            // Fallback to PaymentGateway enum if no specific gateway found
            var gatewayType = gateway?.GatewayType ?? payment.PaymentGateway;

            // Execute payment based on gateway type
            var result = gatewayType switch
            {
                PaymentGatewayType.Square => await ExecuteSquarePaymentAsync(payment),
                PaymentGatewayType.Stripe => await ExecuteStripePaymentAsync(payment),
                PaymentGatewayType.BankTransfer => await ExecuteBankTransferPaymentAsync(payment),
                PaymentGatewayType.CryptoWallet => await ExecuteCryptoWalletPaymentAsync(payment),
                PaymentGatewayType.PIXBrazil => await ExecutePIXBrazilPaymentAsync(payment),
                PaymentGatewayType.DotsDev => await ExecuteDotsDevPaymentAsync(payment),
                PaymentGatewayType.MoonPay => await ExecuteMoonPayPaymentAsync(payment),
                PaymentGatewayType.Coinbase => await ExecuteCoinbasePaymentAsync(payment),
                _ => throw new NotSupportedException($"Gateway type {gatewayType} is not supported")
            };

            _logger.LogInformation("Payment execution completed. CorrelationId: {CorrelationId}, Status: {Status}", 
                correlationId, result.Status);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing payment through gateway. CorrelationId: {CorrelationId}", correlationId);
            
            return new GatewayExecutionResult
            {
                Status = EntityPaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Verifies a payment method with the gateway
    /// </summary>
    public async Task<PaymentMethodVerificationResult> VerifyPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Verifying payment method with gateway. CorrelationId: {CorrelationId}, PaymentMethodId: {PaymentMethodId}", 
            correlationId, paymentMethod.Id.ToString());

        try
        {
            // Load gateway entity from PaymentGatewayId
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.Id == paymentMethod.PaymentGatewayId);
            
            // Use the gateway type from the loaded gateway entity, default to Square if not found
            var gatewayType = gateway?.GatewayType ?? PaymentGatewayType.Square;

            // Verify payment method based on gateway type
            var result = gatewayType switch
            {
                PaymentGatewayType.Square => await VerifySquarePaymentMethodAsync(paymentMethod),
                PaymentGatewayType.Stripe => await VerifyStripePaymentMethodAsync(paymentMethod),
                PaymentGatewayType.BankTransfer => await VerifyBankTransferPaymentMethodAsync(paymentMethod),
                PaymentGatewayType.CryptoWallet => await VerifyCryptoWalletPaymentMethodAsync(paymentMethod),
                PaymentGatewayType.PIXBrazil => await VerifyPIXBrazilPaymentMethodAsync(paymentMethod),
                PaymentGatewayType.DotsDev => await VerifyDotsDevPaymentMethodAsync(paymentMethod),
                PaymentGatewayType.MoonPay => await VerifyMoonPayPaymentMethodAsync(paymentMethod),
                PaymentGatewayType.Coinbase => await VerifyCoinbasePaymentMethodAsync(paymentMethod),
                _ => throw new NotSupportedException($"Gateway type {gatewayType} is not supported")
            };

            _logger.LogInformation("Payment method verification completed. CorrelationId: {CorrelationId}, IsValid: {IsValid}", 
                correlationId, result.IsValid);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment method with gateway. CorrelationId: {CorrelationId}", correlationId);
            
            return new PaymentMethodVerificationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message,
                Metadata = new Dictionary<string, string>
                {
                    ["error_type"] = ex.GetType().Name,
                    ["correlation_id"] = correlationId
                }
            };
        }
    }

    /// <summary>
    /// Gets the health status of all gateways
    /// </summary>
    public async Task<GatewayHealthResponse> GetGatewayHealthAsync()
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Checking gateway health. CorrelationId: {CorrelationId}", correlationId);

        try
        {
            var gateways = await _context.PaymentGateways
                .AsNoTracking()
                .Where(g => g.IsActive)
                .ToListAsync();

            var healthChecks = new List<Task<GatewayHealthStatus>>();

            foreach (var gateway in gateways)
            {
                healthChecks.Add(CheckIndividualGatewayHealthAsync(gateway));
            }

            var results = await Task.WhenAll(healthChecks);
            var overallHealthy = results.All(r => r.IsHealthy);

            _logger.LogInformation("Gateway health check completed. CorrelationId: {CorrelationId}, OverallHealthy: {OverallHealthy}", 
                correlationId, overallHealthy);

            return new GatewayHealthResponse
            {
                IsHealthy = overallHealthy,
                Message = overallHealthy ? "All gateways are healthy" : "One or more gateways are unhealthy",
                GatewayStatuses = results,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking gateway health. CorrelationId: {CorrelationId}", correlationId);
            
            return new GatewayHealthResponse
            {
                IsHealthy = false,
                Message = $"Health check failed: {ex.Message}",
                GatewayStatuses = new List<GatewayHealthStatus>(),
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Processes a refund through the gateway
    /// </summary>
    public async Task<GatewayRefundResult> ProcessRefundAsync(Refund refund)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Processing refund through gateway. CorrelationId: {CorrelationId}, RefundId: {RefundId}", 
            correlationId, refund.Id);

        try
        {
            // Load payment and gateway
            var payment = await _context.Payments
                .Include(p => p.PaymentGateway)
                .FirstOrDefaultAsync(p => p.Id == refund.PaymentId);

            if (payment == null)
            {
                throw new InvalidOperationException($"Payment not found for refund {refund.Id}");
            }

            // Load payment gateway entity if needed for refund processing
            PaymentGateway? gateway = null;
            if (payment.PaymentGatewayId.HasValue)
            {
                gateway = await _context.PaymentGateways.FirstOrDefaultAsync(g => g.Id == payment.PaymentGatewayId.Value);
            }
            
            var gatewayType = gateway?.GatewayType ?? payment.PaymentGateway;

            // Process refund based on gateway type  
            var result = gatewayType switch
            {
                PaymentGatewayType.Square => await ProcessSquareRefundAsync(refund, payment),
                PaymentGatewayType.Stripe => await ProcessStripeRefundAsync(refund, payment),
                PaymentGatewayType.BankTransfer => await ProcessBankTransferRefundAsync(refund, payment),
                PaymentGatewayType.CryptoWallet => await ProcessCryptoWalletRefundAsync(refund, payment),
                PaymentGatewayType.PIXBrazil => await ProcessPIXBrazilRefundAsync(refund, payment),
                PaymentGatewayType.DotsDev => await ProcessDotsDevRefundAsync(refund, payment),
                PaymentGatewayType.MoonPay => await ProcessMoonPayRefundAsync(refund, payment),
                PaymentGatewayType.Coinbase => await ProcessCoinbaseRefundAsync(refund, payment),
                _ => throw new NotSupportedException($"Gateway type {gatewayType} is not supported")
            };

            _logger.LogInformation("Refund processing completed. CorrelationId: {CorrelationId}, Status: {Status}", 
                correlationId, result.Status);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund through gateway. CorrelationId: {CorrelationId}", correlationId);
            
            return new GatewayRefundResult
            {
                Status = Data.Entities.RefundStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Selects the best gateway for a payment
    /// </summary>
    public async Task<PaymentGateway> SelectGatewayAsync(Payment payment)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Selecting gateway for payment. CorrelationId: {CorrelationId}, PaymentId: {PaymentId}, Amount: {Amount}, Currency: {Currency}", 
            correlationId, payment.Id, payment.Amount, payment.Currency);

        try
        {
            var gateways = await _context.PaymentGateways
                .AsNoTracking()
                .Where(g => g.IsActive && 
                           g.SupportedCurrencies != null && 
                           g.SupportedCurrencies.Contains(payment.Currency))
                .Where(g => (g.MinimumAmount == null || payment.Amount >= g.MinimumAmount) &&
                           (g.MaximumAmount == null || payment.Amount <= g.MaximumAmount))
                .OrderBy(g => g.Priority)
                .ThenBy(g => g.FeePercentage + g.FixedFee) // Prefer lower fees
                .ToListAsync();

            if (!gateways.Any())
            {
                throw new InvalidOperationException($"No suitable gateways found for currency {payment.Currency} and amount {payment.Amount}");
            }

            // Check gateway health and select the best one
            foreach (var gateway in gateways)
            {
                var healthStatus = await CheckIndividualGatewayHealthAsync(gateway);
                if (healthStatus.IsHealthy)
                {
                    _logger.LogInformation("Selected gateway for payment. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}, GatewayType: {GatewayType}", 
                        correlationId, gateway.Id, gateway.GatewayType);

                    return gateway;
                }
            }

            // If no healthy gateways, return the first one (fallback)
            var fallbackGateway = gateways.First();
            _logger.LogWarning("No healthy gateways found, using fallback. CorrelationId: {CorrelationId}, GatewayId: {GatewayId}", 
                correlationId, fallbackGateway.Id);

            return fallbackGateway;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting gateway for payment. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
    }

    #region Square Integration

    /// <summary>
    /// Executes a payment through Square
    /// </summary>
private async Task<GatewayExecutionResult> ExecuteSquarePaymentAsync(Payment payment)
{
    _logger.LogInformation("Executing Square payment. PaymentId: {PaymentId}", payment.Id);

    try
    {
        var squareService = _serviceProvider.GetService<ISquareService>();
        if (squareService == null)
        {
            throw new InvalidOperationException("Square service is not registered");
        }

        // Build Square request from payment metadata
        var sourceId = GetMetadataValue(payment.Metadata, "sourceId") 
                       ?? GetMetadataValue(payment.Metadata, "squareToken")
                       ?? GetMetadataValue(payment.Metadata, "cardToken");

        if (string.IsNullOrWhiteSpace(sourceId))
        {
            throw new InvalidOperationException("Square sourceId (payment token) is required in payment metadata");
        }

        var locationId = GetMetadataValue(payment.Metadata, "locationId");
        var note = payment.Description;

        var req = new SquarePaymentRequest
        {
            SourceId = sourceId,
            AmountMoney = MoneyConverter.ToMinorUnits(payment.Amount, payment.Currency),
            Currency = payment.Currency,
            IdempotencyKey = payment.Id.ToString(),
            LocationId = locationId,
            Note = note
        };

        var (status, gatewayId, error) = await squareService.CreatePaymentAsync(req, CancellationToken.None);

        return new GatewayExecutionResult
        {
            Status = status,
            GatewayTransactionId = gatewayId,
            ErrorMessage = error
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Square payment execution failed. PaymentId: {PaymentId}", payment.Id);
        throw;
    }
}

    /// <summary>
    /// Verifies a payment method with Square
    /// </summary>
private async Task<PaymentMethodVerificationResult> VerifySquarePaymentMethodAsync(PaymentMethod paymentMethod)
{
    _logger.LogInformation("Verifying Square payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

    try
    {
        // Lightweight verification: ensure required token/sourceId is present in metadata
        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(paymentMethod.Metadata ?? "{}");
        var token = metadata?.GetValueOrDefault("sourceId")?.ToString()
                    ?? metadata?.GetValueOrDefault("squareToken")?.ToString()
                    ?? metadata?.GetValueOrDefault("cardToken")?.ToString();

        var isValid = !string.IsNullOrWhiteSpace(token);

        return new PaymentMethodVerificationResult
        {
            IsValid = isValid,
            ErrorMessage = isValid ? null : "Missing Square sourceId/card token in payment method metadata",
            Metadata = new Dictionary<string, string>
            {
                ["gateway"] = "Square",
                ["verified_at"] = DateTime.UtcNow.ToString("O")
            }
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Square payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
        throw;
    }
}

    /// <summary>
    /// Processes a refund through Square
    /// </summary>
private async Task<GatewayRefundResult> ProcessSquareRefundAsync(Refund refund, Payment payment)
{
    _logger.LogInformation("Processing Square refund. RefundId: {RefundId}", refund.Id);

    try
    {
        var squareService = _serviceProvider.GetService<ISquareService>();
        if (squareService == null)
        {
            throw new InvalidOperationException("Square service is not registered");
        }

        if (string.IsNullOrWhiteSpace(payment.GatewayTransactionId))
        {
            throw new InvalidOperationException("Original Square payment id (GatewayTransactionId) is missing");
        }

        var amountMinor = MoneyConverter.ToMinorUnits(refund.Amount, refund.Currency ?? payment.Currency);
        var (status, gatewayRefundId, error) = await squareService.CreateRefundAsync(
            payment.GatewayTransactionId,
            amountMinor,
            refund.Id.ToString(),
            refund.Reason,
            CancellationToken.None
        );

        var entityStatus = status;

        return new GatewayRefundResult
        {
            Status = entityStatus switch
            {
                EntityPaymentStatus.Refunded => Data.Entities.RefundStatus.Completed,
                EntityPaymentStatus.Processing => Data.Entities.RefundStatus.Processing,
                EntityPaymentStatus.Failed => Data.Entities.RefundStatus.Failed,
                _ => Data.Entities.RefundStatus.Pending
            },
            GatewayRefundId = gatewayRefundId,
            ErrorMessage = error
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Square refund processing failed. RefundId: {RefundId}", refund.Id);
        throw;
    }
}

    #endregion

    #region Stripe Integration

    /// <summary>
    /// Executes a payment through Stripe
    /// </summary>
    private async Task<GatewayExecutionResult> ExecuteStripePaymentAsync(Payment payment)
    {
        _logger.LogInformation("Executing Stripe payment. PaymentId: {PaymentId}", payment.Id);

        try
        {
            // TODO: Implement actual Stripe SDK integration
            // For now, simulate the payment execution
            await Task.Delay(150); // Simulate API call

            // Simulate success for amounts under $2000, failure for others
            var isSuccess = payment.Amount < 2000;

            return new GatewayExecutionResult
            {
                Status = isSuccess ? PaymentStatus.Completed : PaymentStatus.Failed,
                GatewayTransactionId = isSuccess ? $"pi_{Guid.NewGuid():N}" : null,
                ErrorMessage = isSuccess ? null : "Simulated Stripe payment failure"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe payment execution failed. PaymentId: {PaymentId}", payment.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifies a payment method with Stripe
    /// </summary>
    private async Task<PaymentMethodVerificationResult> VerifyStripePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _logger.LogInformation("Verifying Stripe payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

        try
        {
            // TODO: Implement actual Stripe SDK integration
            await Task.Delay(75); // Simulate API call

            return new PaymentMethodVerificationResult
            {
                IsValid = true,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "Stripe",
                    ["simulated"] = "true",
                    ["verified_at"] = DateTime.UtcNow.ToString("O")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    /// <summary>
    /// Processes a refund through Stripe
    /// </summary>
    private async Task<GatewayRefundResult> ProcessStripeRefundAsync(Refund refund, Payment payment)
    {
        _logger.LogInformation("Processing Stripe refund. RefundId: {RefundId}", refund.Id);

        try
        {
            // TODO: Implement actual Stripe SDK integration
            await Task.Delay(120); // Simulate API call

            return new GatewayRefundResult
            {
                Status = Data.Entities.RefundStatus.Completed,
                GatewayRefundId = $"re_{Guid.NewGuid():N}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe refund processing failed. RefundId: {RefundId}", refund.Id);
            throw;
        }
    }

    #endregion

    #region Bank Transfer Integration

    /// <summary>
    /// Executes a bank transfer payment
    /// </summary>
    private async Task<GatewayExecutionResult> ExecuteBankTransferPaymentAsync(Payment payment)
    {
        _logger.LogInformation("Executing bank transfer payment. PaymentId: {PaymentId}", payment.Id);

        try
        {
            // TODO: Implement actual bank transfer integration
            await Task.Delay(200); // Simulate longer processing time

            return new GatewayExecutionResult
            {
                Status = PaymentStatus.Processing, // Bank transfers typically take longer
                GatewayTransactionId = $"bt_{Guid.NewGuid():N}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bank transfer payment execution failed. PaymentId: {PaymentId}", payment.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifies a bank transfer payment method
    /// </summary>
    private async Task<PaymentMethodVerificationResult> VerifyBankTransferPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _logger.LogInformation("Verifying bank transfer payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

        try
        {
            // TODO: Implement actual bank account verification
            await Task.Delay(100); // Simulate API call

            return new PaymentMethodVerificationResult
            {
                IsValid = true,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "BankTransfer",
                    ["simulated"] = "true",
                    ["verification_method"] = "micro_deposits"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bank transfer payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    /// <summary>
    /// Processes a bank transfer refund
    /// </summary>
    private async Task<GatewayRefundResult> ProcessBankTransferRefundAsync(Refund refund, Payment payment)
    {
        _logger.LogInformation("Processing bank transfer refund. RefundId: {RefundId}", refund.Id);

        try
        {
            // TODO: Implement actual bank transfer refund
            await Task.Delay(150); // Simulate API call

            return new GatewayRefundResult
            {
                Status = Data.Entities.RefundStatus.Processing, // Bank transfer refunds take time
                GatewayRefundId = $"bt_refund_{Guid.NewGuid():N}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bank transfer refund processing failed. RefundId: {RefundId}", refund.Id);
            throw;
        }
    }

    #endregion

    #region Crypto Wallet Integration

    /// <summary>
    /// Executes a crypto wallet payment
    /// </summary>
    private async Task<GatewayExecutionResult> ExecuteCryptoWalletPaymentAsync(Payment payment)
    {
        _logger.LogInformation("Executing crypto wallet payment. PaymentId: {PaymentId}", payment.Id);

        try
        {
            // TODO: Implement actual crypto wallet integration
            await Task.Delay(300); // Simulate blockchain confirmation time

            return new GatewayExecutionResult
            {
                Status = PaymentStatus.Completed,
                GatewayTransactionId = $"0x{Guid.NewGuid():N}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crypto wallet payment execution failed. PaymentId: {PaymentId}", payment.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifies a crypto wallet payment method
    /// </summary>
    private async Task<PaymentMethodVerificationResult> VerifyCryptoWalletPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _logger.LogInformation("Verifying crypto wallet payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

        try
        {
            // TODO: Implement actual wallet verification
            await Task.Delay(100); // Simulate API call

            return new PaymentMethodVerificationResult
            {
                IsValid = true,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "CryptoWallet",
                    ["simulated"] = "true",
                    ["wallet_type"] = "MetaMask"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crypto wallet payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    /// <summary>
    /// Processes a crypto wallet refund
    /// </summary>
    private async Task<GatewayRefundResult> ProcessCryptoWalletRefundAsync(Refund refund, Payment payment)
    {
        _logger.LogInformation("Processing crypto wallet refund. RefundId: {RefundId}", refund.Id);

        try
        {
            // TODO: Implement actual crypto refund
            await Task.Delay(250); // Simulate blockchain transaction

            return new GatewayRefundResult
            {
                Status = Data.Entities.RefundStatus.Completed,
                GatewayRefundId = $"0x{Guid.NewGuid():N}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crypto wallet refund processing failed. RefundId: {RefundId}", refund.Id);
            throw;
        }
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Checks the health of an individual gateway
    /// </summary>
    private async Task<GatewayHealthStatus> CheckIndividualGatewayHealthAsync(PaymentGateway gateway)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // TODO: Implement actual health checks for each gateway
            // For now, simulate health checks
            var delay = gateway.GatewayType switch
            {
                PaymentGatewayType.Square => 50,
                PaymentGatewayType.Stripe => 75,
                PaymentGatewayType.BankTransfer => 100,
                PaymentGatewayType.CryptoWallet => 200,
                PaymentGatewayType.PIXBrazil => 30,
                PaymentGatewayType.DotsDev => 40,
                PaymentGatewayType.MoonPay => 60,
                PaymentGatewayType.Coinbase => 50,
                _ => 100
            };

            await Task.Delay(delay);

            var endTime = DateTime.UtcNow;
            var responseTime = (int)(endTime - startTime).TotalMilliseconds;

            // Simulate occasional failures
            var isHealthy = Random.Shared.Next(1, 101) > 5; // 95% success rate

            return new GatewayHealthStatus
            {
                GatewayId = gateway.Id,
                GatewayName = gateway.Name,
                IsHealthy = true,
                Message = "Gateway is healthy",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                LastChecked = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var responseTime = (int)(endTime - startTime).TotalMilliseconds;

            return new GatewayHealthStatus
            {
                GatewayId = gateway.Id,
                GatewayName = gateway.Name,
                IsHealthy = false,
                Message = $"Gateway health check failed: {ex.Message}",
                ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
                LastChecked = DateTime.UtcNow
            };
        }
    }

    #endregion

    #region PIX Brazil Integration

    /// <summary>
    /// Executes a payment through PIX Brazil
    /// </summary>
    private async Task<GatewayExecutionResult> ExecutePIXBrazilPaymentAsync(Payment payment)
    {
        _logger.LogInformation("Executing PIX Brazil payment. PaymentId: {PaymentId}", payment.Id);

        try
        {
            var pixService = _serviceProvider.GetService<IPIXBrazilService>();
            if (pixService == null)
            {
                throw new InvalidOperationException("PIX Brazil service is not registered");
            }

            // Determine if this is a payout or charge based on payment type
            if (payment.Type == PaymentType.Withdrawal)
            {
                // PIX Payout (money out)
                var payoutRequest = new PIXPayoutRequest
                {
                    AmountInCents = (int)(payment.Amount * 100),
                    Currency = payment.Currency,
                    TargetPixKey = GetMetadataValue(payment.Metadata, "pixKey") ?? string.Empty,
                    TargetPixKeyType = GetMetadataValue(payment.Metadata, "pixKeyType") ?? "cpf",
                    TargetDocument = GetMetadataValue(payment.Metadata, "targetDocument"),
                    TargetName = GetMetadataValue(payment.Metadata, "targetName") ?? "Recipient",
                    Description = $"Payment {payment.Id}",
                    IdempotencyKey = payment.Id.ToString()
                };

                var result = await pixService.CreatePayoutAsync(payoutRequest);

                return new GatewayExecutionResult
                {
                    Status = result.Status.ToLower() switch
                    {
                        "completed" => PaymentStatus.Completed,
                        "processing" => PaymentStatus.Processing,
                        "failed" => PaymentStatus.Failed,
                        _ => PaymentStatus.Pending
                    },
                    GatewayTransactionId = result.Id,
                    ErrorMessage = result.Status.ToLower() == "failed" ? "PIX payout failed" : null
                };
            }
            else
            {
                // PIX Charge (money in)
                var chargeRequest = new PIXChargeRequest
                {
                    Amount = (int)(payment.Amount * 100),
                    Currency = payment.Currency,
                    PaymentMethod = "PIX_DYNAMIC_QR",
                    Description = $"Charge for payment {payment.Id}",
                    ExpirationInfo = new PIXExpirationInfo { Seconds = 3600 }, // 1 hour
                    Payer = payment.UserId != Guid.Empty ? new PIXPayer
                    {
                        Name = GetMetadataValue(payment.Metadata, "payerName") ?? "Payer",
                        Document = GetMetadataValue(payment.Metadata, "payerDocument") ?? string.Empty,
                        Email = GetMetadataValue(payment.Metadata, "payerEmail")
                    } : null
                };

                var result = await pixService.CreateChargeAsync(chargeRequest);

                // Store QR code in payment metadata
                if (!string.IsNullOrEmpty(result.QRCode))
                {
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                    metadata["qrCode"] = result.QRCode;
                    metadata["qrCodeBase64"] = result.QRCodeBase64 ?? string.Empty;
                    payment.Metadata = JsonSerializer.Serialize(metadata);
                }

                return new GatewayExecutionResult
                {
                    Status = PaymentStatus.Pending, // PIX charges start as pending until paid
                    GatewayTransactionId = result.Id
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PIX Brazil payment execution failed. PaymentId: {PaymentId}", payment.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifies a payment method with PIX Brazil
    /// </summary>
    private async Task<PaymentMethodVerificationResult> VerifyPIXBrazilPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _logger.LogInformation("Verifying PIX Brazil payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

        try
        {
            var pixService = _serviceProvider.GetService<IPIXBrazilService>();
            if (pixService == null)
            {
                throw new InvalidOperationException("PIX Brazil service is not registered");
            }

            // Extract PIX key information from metadata
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(paymentMethod.Metadata ?? "{}");
            var pixKey = metadata?.GetValueOrDefault("pixKey")?.ToString();
            var pixKeyType = metadata?.GetValueOrDefault("pixKeyType")?.ToString() ?? "cpf";

            if (string.IsNullOrEmpty(pixKey))
            {
                return new PaymentMethodVerificationResult
                {
                    IsValid = false,
                    ErrorMessage = "PIX key is required"
                };
            }

            var isValid = await pixService.ValidatePixKeyAsync(pixKey, pixKeyType);

            return new PaymentMethodVerificationResult
            {
                IsValid = isValid,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "PIXBrazil",
                    ["pixKeyType"] = pixKeyType,
                    ["verified_at"] = DateTime.UtcNow.ToString("O")
                },
                ErrorMessage = isValid ? null : "Invalid PIX key"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PIX Brazil payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    /// <summary>
    /// Processes a refund through PIX Brazil
    /// </summary>
    private async Task<GatewayRefundResult> ProcessPIXBrazilRefundAsync(Refund refund, Payment payment)
    {
        _logger.LogInformation("Processing PIX Brazil refund. RefundId: {RefundId}", refund.Id);

        try
        {
            var pixService = _serviceProvider.GetService<IPIXBrazilService>();
            if (pixService == null)
            {
                throw new InvalidOperationException("PIX Brazil service is not registered");
            }

            // PIX refunds are implemented as reverse payouts
            var refundRequest = new PIXPayoutRequest
            {
                AmountInCents = (int)(refund.Amount * 100),
                Currency = payment.Currency,
                TargetPixKey = GetMetadataValue(refund.Metadata, "refundPixKey") ?? 
                              GetMetadataValue(payment.Metadata, "payerPixKey") ?? string.Empty,
                TargetPixKeyType = GetMetadataValue(refund.Metadata, "refundPixKeyType") ?? 
                                  GetMetadataValue(payment.Metadata, "payerPixKeyType") ?? "cpf",
                TargetDocument = GetMetadataValue(refund.Metadata, "refundDocument") ?? 
                                GetMetadataValue(payment.Metadata, "payerDocument"),
                TargetName = GetMetadataValue(refund.Metadata, "refundName") ?? 
                            GetMetadataValue(payment.Metadata, "payerName") ?? "Refund Recipient",
                Description = $"Refund for payment {payment.Id}",
                IdempotencyKey = refund.Id.ToString()
            };

            var result = await pixService.CreatePayoutAsync(refundRequest);

            return new GatewayRefundResult
            {
                Status = result.Status.ToLower() switch
                {
                    "completed" => Data.Entities.RefundStatus.Completed,
                    "processing" => Data.Entities.RefundStatus.Processing,
                    "failed" => Data.Entities.RefundStatus.Failed,
                    _ => Data.Entities.RefundStatus.Pending
                },
                GatewayRefundId = result.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PIX Brazil refund processing failed. RefundId: {RefundId}", refund.Id);
            throw;
        }
    }

    #endregion

    #region DotsDev Integration

    /// <summary>
    /// Executes a payment through Dots.dev (global payouts)
    /// </summary>
    private async Task<GatewayExecutionResult> ExecuteDotsDevPaymentAsync(Payment payment)
    {
        _logger.LogInformation("Executing Dots.dev payment. PaymentId: {PaymentId}", payment.Id);

        try
        {
            var dotsDevService = _serviceProvider.GetService<IDotsDevService>();
            if (dotsDevService == null)
            {
                throw new InvalidOperationException("Dots.dev service is not registered");
            }

            // Dots.dev only supports withdrawals/payouts
            if (payment.Type != PaymentType.Withdrawal)
            {
                throw new InvalidOperationException("Dots.dev only supports withdrawal/payout transactions");
            }

            // Create payout request
            var payoutRequest = new DotsDevPayoutRequest
            {
                AmountInCents = (int)(payment.Amount * 100),
                Currency = payment.Currency,
                IdempotencyKey = payment.Id.ToString(),
                Description = $"Payout {payment.Id}",
                Recipient = new DotsDevRecipient
                {
                    Name = GetMetadataValue(payment.Metadata, "recipientName") ?? "Recipient",
                    Email = GetMetadataValue(payment.Metadata, "recipientEmail") ?? string.Empty,
                    Country = GetMetadataValue(payment.Metadata, "recipientCountry") ?? "US",
                    Phone = GetMetadataValue(payment.Metadata, "recipientPhone"),
                    PaymentMethod = GetMetadataValue(payment.Metadata, "paymentMethod")
                },
                Metadata = new Dictionary<string, object>
                {
                    ["paymentId"] = payment.Id.ToString(),
                    ["userId"] = payment.UserId.ToString()
                }
            };

            var result = await dotsDevService.CreatePayoutAsync(payoutRequest);

            return new GatewayExecutionResult
            {
                Status = result.Status.ToLower() switch
                {
                    "completed" => PaymentStatus.Completed,
                    "processing" => PaymentStatus.Processing,
                    "failed" => PaymentStatus.Failed,
                    _ => PaymentStatus.Pending
                },
                GatewayTransactionId = result.Id,
                ErrorMessage = result.Error?.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dots.dev payment execution failed. PaymentId: {PaymentId}", payment.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifies a payment method with Dots.dev
    /// </summary>
    private async Task<PaymentMethodVerificationResult> VerifyDotsDevPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _logger.LogInformation("Verifying Dots.dev payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

        try
        {
            var dotsDevService = _serviceProvider.GetService<IDotsDevService>();
            if (dotsDevService == null)
            {
                throw new InvalidOperationException("Dots.dev service is not registered");
            }

            // Extract recipient information from metadata
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(paymentMethod.Metadata ?? "{}");
            var country = metadata?.GetValueOrDefault("country")?.ToString() ?? "US";
            
            // Validate recipient data
            var validationResult = await dotsDevService.ValidateRecipientAsync(country, metadata ?? new Dictionary<string, object>());

            return new PaymentMethodVerificationResult
            {
                IsValid = validationResult.IsValid,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "DotsDev",
                    ["country"] = country,
                    ["verified_at"] = DateTime.UtcNow.ToString("O"),
                    ["suggested_methods"] = string.Join(",", validationResult.SuggestedMethods)
                },
                ErrorMessage = validationResult.IsValid ? null : string.Join("; ", validationResult.Errors.Select(e => e.Message))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dots.dev payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    /// <summary>
    /// Processes a refund through Dots.dev
    /// </summary>
    private async Task<GatewayRefundResult> ProcessDotsDevRefundAsync(Refund refund, Payment payment)
    {
        _logger.LogInformation("Processing Dots.dev refund. RefundId: {RefundId}", refund.Id);

        try
        {
            // Dots.dev doesn't support direct refunds for payouts
            // Refunds would need to be processed as new incoming payments
            _logger.LogWarning("Dots.dev does not support direct refunds for payouts. Manual processing required.");

            return new GatewayRefundResult
            {
                Status = Data.Entities.RefundStatus.Failed,
                ErrorMessage = "Dots.dev payouts cannot be refunded directly. Please process as a new incoming payment."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dots.dev refund processing failed. RefundId: {RefundId}", refund.Id);
            throw;
        }
    }

    #endregion

    #region MoonPay Integration

    /// <summary>
    /// Executes a payment through MoonPay (fiat-to-crypto or crypto-to-fiat)
    /// </summary>
    private async Task<GatewayExecutionResult> ExecuteMoonPayPaymentAsync(Payment payment)
    {
        _logger.LogInformation("Executing MoonPay payment. PaymentId: {PaymentId}", payment.Id);

        try
        {
            var moonPayService = _serviceProvider.GetService<IMoonPayService>();
            if (moonPayService == null)
            {
                throw new InvalidOperationException("MoonPay service is not registered");
            }

            // Determine transaction type from payment metadata
            var transactionType = GetMetadataValue(payment.Metadata, "transactionType") ?? "buy";

            if (transactionType == "buy")
            {
                // Fiat-to-crypto (deposit/buy)
                var buyRequest = new MoonPayBuyRequest
                {
                    CurrencyCode = GetMetadataValue(payment.Metadata, "cryptoCurrency") ?? "BTC",
                    BaseCurrencyAmount = payment.Amount,
                    WalletAddress = GetMetadataValue(payment.Metadata, "walletAddress") ?? string.Empty,
                    BaseCurrencyCode = payment.Currency,
                    AreFeesIncluded = bool.Parse(GetMetadataValue(payment.Metadata, "includeFees") ?? "false"),
                    ExternalTransactionId = payment.Id.ToString(),
                    Email = GetMetadataValue(payment.Metadata, "email"),
                    PaymentMethod = GetMetadataValue(payment.Metadata, "paymentMethod") ?? "credit_debit_card"
                };

                var result = await moonPayService.CreateBuyTransactionAsync(buyRequest);

                // Store widget URL in metadata
                if (!string.IsNullOrEmpty(result.WidgetUrl))
                {
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
                    metadata["widgetUrl"] = result.WidgetUrl;
                    metadata["moonPayTransactionId"] = result.Id;
                    payment.Metadata = JsonSerializer.Serialize(metadata);
                }

                return new GatewayExecutionResult
                {
                    Status = result.Status.ToLower() switch
                    {
                        "completed" => PaymentStatus.Completed,
                        "pending" => PaymentStatus.Pending,
                        "failed" => PaymentStatus.Failed,
                        _ => PaymentStatus.Processing
                    },
                    GatewayTransactionId = result.Id,
                    ErrorMessage = result.FailureReason
                };
            }
            else if (transactionType == "sell")
            {
                // Crypto-to-fiat (withdrawal/sell)
                var sellRequest = new MoonPaySellRequest
                {
                    BaseCurrencyCode = GetMetadataValue(payment.Metadata, "cryptoCurrency") ?? "BTC",
                    BaseCurrencyAmount = payment.Amount,
                    QuoteCurrencyCode = payment.Currency,
                    ExternalTransactionId = payment.Id.ToString(),
                    Email = GetMetadataValue(payment.Metadata, "email"),
                    BankAccount = JsonSerializer.Deserialize<MoonPayBankAccount>(
                        GetMetadataValue(payment.Metadata, "bankAccount") ?? "{}")
                };

                var result = await moonPayService.CreateSellTransactionAsync(sellRequest);

                return new GatewayExecutionResult
                {
                    Status = result.Status.ToLower() switch
                    {
                        "completed" => PaymentStatus.Completed,
                        "pending" => PaymentStatus.Pending,
                        "failed" => PaymentStatus.Failed,
                        _ => PaymentStatus.Processing
                    },
                    GatewayTransactionId = result.Id,
                    ErrorMessage = result.FailureReason
                };
            }
            else
            {
                throw new InvalidOperationException($"Unknown MoonPay transaction type: {transactionType}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoonPay payment execution failed. PaymentId: {PaymentId}", payment.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifies a payment method with MoonPay
    /// </summary>
    private async Task<PaymentMethodVerificationResult> VerifyMoonPayPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _logger.LogInformation("Verifying MoonPay payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

        try
        {
            var moonPayService = _serviceProvider.GetService<IMoonPayService>();
            if (moonPayService == null)
            {
                throw new InvalidOperationException("MoonPay service is not registered");
            }

            // Extract verification data from metadata
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(paymentMethod.Metadata ?? "{}");
            var email = metadata?.GetValueOrDefault("email")?.ToString();
            var country = metadata?.GetValueOrDefault("country")?.ToString();

            if (string.IsNullOrEmpty(email))
            {
                return new PaymentMethodVerificationResult
                {
                    IsValid = false,
                    ErrorMessage = "Email is required for MoonPay verification"
                };
            }

            // Perform KYC/AML screening
            var screeningRequest = new MoonPayScreeningRequest
            {
                Email = email,
                Country = country,
                Name = metadata?.GetValueOrDefault("name")?.ToString(),
                DateOfBirth = metadata?.GetValueOrDefault("dateOfBirth")?.ToString(),
                State = metadata?.GetValueOrDefault("state")?.ToString()
            };

            var screeningResult = await moonPayService.ScreenRecipientAsync(screeningRequest);

            return new PaymentMethodVerificationResult
            {
                IsValid = screeningResult.IsPassed,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "MoonPay",
                    ["riskLevel"] = screeningResult.RiskLevel ?? "unknown",
                    ["verified_at"] = DateTime.UtcNow.ToString("O"),
                    ["requiredVerificationLevel"] = screeningResult.RequiredVerificationLevel ?? "basic"
                },
                ErrorMessage = screeningResult.IsPassed ? null : string.Join("; ", screeningResult.RejectionReasons)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoonPay payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    /// <summary>
    /// Processes a refund through MoonPay
    /// </summary>
    private async Task<GatewayRefundResult> ProcessMoonPayRefundAsync(Refund refund, Payment payment)
    {
        _logger.LogInformation("Processing MoonPay refund. RefundId: {RefundId}", refund.Id);

        try
        {
            // MoonPay doesn't support direct refunds for crypto transactions
            // Refunds would need to be processed as new transactions in the opposite direction
            _logger.LogWarning("MoonPay does not support direct refunds for crypto transactions. Manual processing required.");

            return new GatewayRefundResult
            {
                Status = Data.Entities.RefundStatus.Failed,
                ErrorMessage = "MoonPay transactions cannot be refunded directly. Please process as a new transaction in the opposite direction."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoonPay refund processing failed. RefundId: {RefundId}", refund.Id);
            throw;
        }
    }

    #endregion

    #region Coinbase Integration

    /// <summary>
    /// Executes a payment through Coinbase (cryptocurrency trading)
    /// </summary>
    private async Task<GatewayExecutionResult> ExecuteCoinbasePaymentAsync(Payment payment)
    {
        _logger.LogInformation("Executing Coinbase payment. PaymentId: {PaymentId}", payment.Id);

        try
        {
            var coinbaseService = _serviceProvider.GetService<ICoinbaseService>();
            if (coinbaseService == null)
            {
                throw new InvalidOperationException("Coinbase service is not registered");
            }

            // Extract order details from metadata
            var productId = GetMetadataValue(payment.Metadata, "productId") ?? "BTC-USDC";
            var orderType = GetMetadataValue(payment.Metadata, "orderType") ?? "market";
            var side = payment.Type == PaymentType.Deposit ? "buy" : "sell";

            // Create order request
            var orderRequest = new CoinbaseOrderRequest
            {
                ClientOrderId = payment.Id.ToString(),
                ProductId = productId,
                Side = side,
                OrderConfiguration = BuildCoinbaseOrderConfiguration(payment, orderType, side)
            };

            var result = await coinbaseService.CreateOrderAsync(orderRequest);

            // Store order details in metadata
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new Dictionary<string, object>();
            metadata["coinbaseOrderId"] = result.OrderId;
            metadata["averageFilledPrice"] = result.AverageFilledPrice;
            metadata["filledSize"] = result.FilledSize;
            metadata["fee"] = result.Fee;
            metadata["status"] = result.Status;
            payment.Metadata = JsonSerializer.Serialize(metadata);

            return new GatewayExecutionResult
            {
                Status = result.Status.ToLower() switch
                {
                    "filled" => PaymentStatus.Completed,
                    "pending" => PaymentStatus.Pending,
                    "cancelled" => PaymentStatus.Cancelled,
                    "failed" => PaymentStatus.Failed,
                    _ => PaymentStatus.Processing
                },
                GatewayTransactionId = result.OrderId,
                ErrorMessage = result.RejectReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Coinbase payment execution failed. PaymentId: {PaymentId}", payment.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifies a payment method with Coinbase
    /// </summary>
    private async Task<PaymentMethodVerificationResult> VerifyCoinbasePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _logger.LogInformation("Verifying Coinbase payment method. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);

        try
        {
            var coinbaseService = _serviceProvider.GetService<ICoinbaseService>();
            if (coinbaseService == null)
            {
                throw new InvalidOperationException("Coinbase service is not registered");
            }

            // Validate connection and API credentials
            var isValid = await coinbaseService.ValidateConnectionAsync();

            if (isValid)
            {
                // Additionally check if the user has any accounts
                var accounts = await coinbaseService.GetAccountsAsync();
                isValid = accounts.Any();
            }

            return new PaymentMethodVerificationResult
            {
                IsValid = isValid,
                Metadata = new Dictionary<string, string>
                {
                    ["gateway"] = "Coinbase",
                    ["verified_at"] = DateTime.UtcNow.ToString("O"),
                    ["accountsAvailable"] = isValid.ToString()
                },
                ErrorMessage = isValid ? null : "Coinbase connection validation failed or no accounts available"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Coinbase payment method verification failed. PaymentMethodId: {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    /// <summary>
    /// Processes a refund through Coinbase
    /// </summary>
    private async Task<GatewayRefundResult> ProcessCoinbaseRefundAsync(Refund refund, Payment payment)
    {
        _logger.LogInformation("Processing Coinbase refund. RefundId: {RefundId}", refund.Id);

        try
        {
            var coinbaseService = _serviceProvider.GetService<ICoinbaseService>();
            if (coinbaseService == null)
            {
                throw new InvalidOperationException("Coinbase service is not registered");
            }

            // Get original order ID
            var originalOrderId = GetMetadataValue(payment.Metadata, "coinbaseOrderId");
            if (string.IsNullOrEmpty(originalOrderId))
            {
                throw new InvalidOperationException("Original Coinbase order ID not found");
            }

            // Coinbase doesn't support direct refunds - need to create a reverse trade
            var originalOrder = await coinbaseService.GetOrderAsync(originalOrderId);
            
            // Create reverse order (if original was buy, create sell and vice versa)
            var reverseSide = originalOrder.Side.ToLower() == "buy" ? "sell" : "buy";
            var productId = originalOrder.ProductId;
            
            var refundOrderRequest = new CoinbaseOrderRequest
            {
                ClientOrderId = refund.Id.ToString(),
                ProductId = productId,
                Side = reverseSide,
                OrderConfiguration = new CoinbaseOrderConfiguration
                {
                    MarketOrder = new MarketOrderConfig
                    {
                        BaseSize = originalOrder.FilledSize // Use the filled size from original order
                    }
                }
            };

            var result = await coinbaseService.CreateOrderAsync(refundOrderRequest);

            return new GatewayRefundResult
            {
                Status = result.Status.ToLower() switch
                {
                    "filled" => Data.Entities.RefundStatus.Completed,
                    "pending" => Data.Entities.RefundStatus.Processing,
                    "cancelled" => Data.Entities.RefundStatus.Failed,
                    "failed" => Data.Entities.RefundStatus.Failed,
                    _ => Data.Entities.RefundStatus.Pending
                },
                GatewayRefundId = result.OrderId,
                ErrorMessage = result.RejectReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Coinbase refund processing failed. RefundId: {RefundId}", refund.Id);
            throw;
        }
    }

    /// <summary>
    /// Builds Coinbase order configuration from payment metadata
    /// </summary>
    private CoinbaseOrderConfiguration BuildCoinbaseOrderConfiguration(Payment payment, string orderType, string side)
    {
        var config = new CoinbaseOrderConfiguration();

        switch (orderType.ToLower())
        {
            case "market":
                if (side == "buy" && payment.Currency.EndsWith("USD") || payment.Currency.EndsWith("USDC"))
                {
                    // For buy orders with USD amount
                    config.MarketOrder = new MarketOrderConfig
                    {
                        QuoteSize = payment.Amount.ToString()
                    };
                }
                else
                {
                    // For sell orders or crypto amounts
                    config.MarketOrder = new MarketOrderConfig
                    {
                        BaseSize = payment.Amount.ToString()
                    };
                }
                break;

            case "limit":
                var limitPrice = GetMetadataValue(payment.Metadata, "limitPrice");
                if (!string.IsNullOrEmpty(limitPrice))
                {
                    config.LimitOrderGTC = new LimitOrderConfig
                    {
                        BaseSize = payment.Amount.ToString(),
                        LimitPrice = limitPrice,
                        PostOnly = bool.Parse(GetMetadataValue(payment.Metadata, "postOnly") ?? "false")
                    };
                }
                break;

            case "stop_limit":
                var stopPrice = GetMetadataValue(payment.Metadata, "stopPrice");
                var stopLimitPrice = GetMetadataValue(payment.Metadata, "limitPrice");
                if (!string.IsNullOrEmpty(stopPrice) && !string.IsNullOrEmpty(stopLimitPrice))
                {
                    config.StopLimitOrderGTC = new StopLimitOrderConfig
                    {
                        BaseSize = payment.Amount.ToString(),
                        LimitPrice = stopLimitPrice,
                        StopPrice = stopPrice,
                        StopDirection = GetMetadataValue(payment.Metadata, "stopDirection") ?? "stop_direction_stop_down"
                    };
                }
                break;
        }

        return config;
    }

    #endregion

    #region Helper Methods

    private string? GetMetadataValue(string? metadataJson, string key)
    {
        if (string.IsNullOrEmpty(metadataJson))
            return null;
        
        try
        {
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
            return metadata?.GetValueOrDefault(key)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
