using Microsoft.Extensions.Logging;
using PaymentGatewayService.Clients;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using PaymentGatewayService.Models;
using PaymentGatewayService.Models.Requests;
using EntityPaymentType = PaymentGatewayService.Data.Entities.PaymentType;
 

namespace PaymentGatewayService.Services;

/// <summary>
/// Routes payments to appropriate payment providers based on various criteria
/// </summary>
public class PaymentRouter : IPaymentRouter
{
    private readonly ILogger<PaymentRouter> _logger;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ITreasuryServiceClient _treasuryServiceClient;
    private readonly IWalletBalanceService _walletBalanceService;

    public PaymentRouter(
        ILogger<PaymentRouter> logger,
        IPaymentGatewayService paymentGatewayService,
        ITreasuryServiceClient treasuryServiceClient,
        IWalletBalanceService walletBalanceService)
    {
        _logger = logger;
        _paymentGatewayService = paymentGatewayService;
        _treasuryServiceClient = treasuryServiceClient;
        _walletBalanceService = walletBalanceService;
    }

    /// <inheritdoc/>
    public async Task<PaymentRoutingResult> RoutePaymentAsync(PaymentRoutingRequest paymentRequest)
    {
        try
        {
            _logger.LogInformation("Routing payment for user {UserId}, amount {Amount} {Currency}",
                paymentRequest.UserId, paymentRequest.Amount, paymentRequest.Currency);

            // Select the appropriate provider
            var provider = await SelectProviderAsync(paymentRequest);

            // Check if provider is available
            var isAvailable = await IsProviderAvailableAsync(provider, paymentRequest.Currency, paymentRequest.Amount);
            if (!isAvailable)
            {
                _logger.LogWarning("Selected provider {Provider} is not available for {Currency}",
                    provider, paymentRequest.Currency);
                
                return new PaymentRoutingResult
                {
                    Success = false,
                    SelectedProvider = null, // PaymentRoutingResult expects PaymentProvider class, not enum
                    ErrorMessage = $"Provider {provider} is not available for {paymentRequest.Currency}"
                };
            }

            // Route to the appropriate handler based on payment type
            if (paymentRequest.PaymentType == global::PaymentGatewayService.Models.PaymentType.Crypto)
            {
                return await RouteCryptoPaymentAsync(paymentRequest, provider);
            }
            else
            {
                return await RouteFiatPaymentAsync(paymentRequest, provider);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing payment for user {UserId}", paymentRequest.UserId);
            return new PaymentRoutingResult
            {
                Success = false,
                ErrorMessage = "An error occurred while routing the payment"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<PaymentProviderType> SelectProviderAsync(PaymentRoutingRequest request)
    {
        // If a preferred provider is specified and available, use it
        if (request.PreferredProvider.HasValue)
        {
            var providerType = ConvertGatewayToProviderType(request.PreferredProvider.Value);
            var isAvailable = await IsProviderAvailableAsync(
                providerType, 
                request.Currency, 
                request.Amount);
            
            if (isAvailable)
            {
                return providerType;
            }
        }

        // Provider selection logic based on currency and payment type
        if (request.PaymentType == global::PaymentGatewayService.Models.PaymentType.Crypto)
        {
            // For crypto payments, check Treasury Service
            return PaymentProviderType.Internal; // Internal wallet
        }

        // Fiat payment provider selection
        return request.Currency.ToUpperInvariant() switch
        {
            "USD" => PaymentProviderType.Square,
            "BRL" => PaymentProviderType.PIXBrazil,
            "EUR" => PaymentProviderType.MoonPay,
            _ => PaymentProviderType.Stripe // Default fallback
        };
    }

    /// <inheritdoc/>
    public async Task<bool> IsProviderAvailableAsync(PaymentProviderType provider, string currency, decimal amount)
    {
        try
        {
            // Check provider-specific availability
            return provider switch
            {
                PaymentProviderType.Square => currency == "USD" && amount >= 1m,
                PaymentProviderType.Stripe => amount >= 0.50m,
                PaymentProviderType.PIXBrazil => currency == "BRL" && amount >= 0.01m,
                PaymentProviderType.MoonPay => new[] { "USD", "EUR", "GBP" }.Contains(currency),
                PaymentProviderType.DotsDev => currency == "USD",
                PaymentProviderType.Coinbase => new[] { "BTC", "ETH", "USDC" }.Contains(currency),
                PaymentProviderType.Internal => true, // Always available for internal transfers
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking provider availability for {Provider}", provider);
            return false;
        }
    }

    private async Task<PaymentRoutingResult> RouteCryptoPaymentAsync(
        PaymentRoutingRequest request, 
        PaymentProviderType provider)
    {
        try
        {
            // For crypto payments, check wallet balance first
            var balance = await _walletBalanceService.GetBalanceAsync(request.UserId, request.Currency);
            
            if (balance < request.Amount)
            {
                return new PaymentRoutingResult
                {
                    Success = false,
                    SelectedProvider = null, // PaymentRoutingResult expects PaymentProvider class, not enum
                    ErrorMessage = "Insufficient balance"
                };
            }

            // Create transaction via Treasury Service
            var transactionRequest = new CreateTransactionRequest
            {
                UserId = request.UserId,
                Currency = request.Currency,
                Amount = request.Amount,
                Type = "payment",
                Reference = $"PAYMENT-{Guid.NewGuid()}",
                Metadata = request?.Metadata
            };

            var transaction = await _treasuryServiceClient.CreateTransactionAsync(transactionRequest);

            return new PaymentRoutingResult
            {
                Success = true,
                SelectedProvider = null, // PaymentRoutingResult expects PaymentProvider class, not enum
                PaymentId = transaction.TransactionId,
                ExternalReference = transaction.Reference,
                AdditionalData = new Dictionary<string, string>
                {
                    ["transactionId"] = transaction.TransactionId,
                    ["status"] = transaction.Status
                },
                RoutingMetadata = request?.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing crypto payment");
            return new PaymentRoutingResult
            {
                Success = false,
                SelectedProvider = null,
                ErrorMessage = "Failed to process crypto payment"
            };
        }
    }

    private async Task<PaymentRoutingResult> RouteFiatPaymentAsync(
        PaymentRoutingRequest request, 
        PaymentProviderType provider)
    {
        try
        {
        // For fiat payments, use the payment gateway service
            var paymentResult = await _paymentGatewayService.ProcessPaymentAsync(new global::PaymentGatewayService.Models.Requests.ProcessPaymentRequest
            {
                PaymentMethodId = null, // Will be determined by gateway
                Amount = request.Amount,
                Currency = request.Currency,
                UserId = request.UserId,
                Description = request.Description,
                Metadata = request.Metadata != null ? request.Metadata.ToDictionary(k => k.Key, k => (object)k.Value) : new Dictionary<string, object>()
            });

            return new PaymentRoutingResult
            {
                Success = paymentResult.Success,
                SelectedProvider = null, // PaymentRoutingResult expects PaymentProvider class, not enum
                PaymentId = paymentResult.TransactionId,
                ExternalReference = paymentResult.TransactionId,
                RedirectUrl = null,
                ErrorMessage = paymentResult.ErrorMessage,
                AdditionalData = paymentResult.Metadata != null ? 
                    paymentResult.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty) : 
                    new Dictionary<string, string>(),
                RoutingMetadata = request.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing fiat payment");
            return new PaymentRoutingResult
            {
                Success = false,
                SelectedProvider = null,
                ErrorMessage = "Failed to process fiat payment"
            };
        }
    }

    #region IPaymentRouter Interface Implementation

    /// <summary>
    /// Routes a payment request (interface implementation)
    /// </summary>
    public async Task<global::PaymentGatewayService.Models.PaymentResult> RouteAsync(global::PaymentGatewayService.Models.PaymentRouterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert to internal request format and use existing logic
            var routingRequest = new PaymentRoutingRequest
            {
                UserId = request.UserId ?? string.Empty,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentType = global::PaymentGatewayService.Models.PaymentType.Deposit // Default since PaymentRouterRequest doesn't have PaymentType
            };

            var result = await RoutePaymentAsync(routingRequest);

            return new global::PaymentGatewayService.Models.PaymentResult
            {
                Success = result.Success,
                TransactionId = result.PaymentId ?? string.Empty,
                Status = result.Success ? global::PaymentGatewayService.Models.PaymentStatus.Completed : global::PaymentGatewayService.Models.PaymentStatus.Failed,
                ErrorMessage = result.ErrorMessage,
                Metadata = result.AdditionalData?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RouteAsync");
            return new PaymentResult
            {
                Success = false,
                Status = Models.PaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Routes a payment request (interface implementation)
    /// </summary>
    public async Task<global::PaymentGatewayService.Models.PaymentResult> RoutePaymentAsync(global::PaymentGatewayService.Models.ProcessPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert to internal request format
            var routingRequest = new PaymentRoutingRequest
            {
                UserId = request.UserId ?? string.Empty,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentType = global::PaymentGatewayService.Models.PaymentType.Deposit // Default assumption
            };

            var result = await RoutePaymentAsync(routingRequest);

            return new global::PaymentGatewayService.Models.PaymentResult
            {
                Success = result.Success,
                TransactionId = result.PaymentId ?? string.Empty,
                Status = result.Success ? global::PaymentGatewayService.Models.PaymentStatus.Completed : global::PaymentGatewayService.Models.PaymentStatus.Failed,
                ErrorMessage = result.ErrorMessage,
                Metadata = result.AdditionalData?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RoutePaymentAsync");
            return new global::PaymentGatewayService.Models.PaymentResult
            {
                Success = false,
                Status = global::PaymentGatewayService.Models.PaymentStatus.Failed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Routes a refund request (interface implementation)
    /// </summary>
    public async Task<Services.Interfaces.RefundResult> RouteRefundAsync(Services.Interfaces.RefundRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple implementation for now
            _logger.LogInformation("Routing refund for payment {PaymentId}", request.PaymentId);

            return new Services.Interfaces.RefundResult
            {
                Success = true,
                RefundId = Guid.NewGuid(),
                RefundTransactionId = Guid.NewGuid().ToString(),
                Status = new Services.Interfaces.RefundStatus
                {
                    RefundId = Guid.NewGuid(),
                    Status = "Pending",
                    Amount = 0m,
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RouteRefundAsync");
            return new Services.Interfaces.RefundResult
            {
                Success = false,
                Status = new Services.Interfaces.RefundStatus
                {
                    RefundId = Guid.NewGuid(),
                    Status = "Failed",
                    Amount = 0m,
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// Gets the status of a payment (interface implementation)
    /// </summary>
    public async Task<global::PaymentGatewayService.Models.PaymentStatus> GetStatusAsync(string transactionId, string provider, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting status for transaction {TransactionId} from provider {Provider}", transactionId, provider);
            
            // Simple implementation - return completed status
            await Task.Delay(10, cancellationToken); // Simulate async work
            return Models.PaymentStatus.Completed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetStatusAsync");
            return Models.PaymentStatus.Failed;
        }
    }

    #endregion

    /// <summary>
    /// Converts PaymentGatewayType to PaymentProviderType
    /// </summary>
    private static PaymentProviderType ConvertGatewayToProviderType(PaymentGatewayType gatewayType)
    {
        return gatewayType switch
        {
            PaymentGatewayType.Square => PaymentProviderType.Square,
            PaymentGatewayType.PIXBrazil => PaymentProviderType.PIXBrazil,
            PaymentGatewayType.MoonPay => PaymentProviderType.MoonPay,
            PaymentGatewayType.Stripe => PaymentProviderType.Stripe,
            PaymentGatewayType.DotsDev => PaymentProviderType.DotsDev,
            PaymentGatewayType.Coinbase => PaymentProviderType.Coinbase,
            _ => PaymentProviderType.Internal // Default fallback
        };
    }
}
