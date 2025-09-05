using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Wallet;
using MobileAPIGateway.Services;
using MobileAPIGateway.Clients;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Wallet controller with enhanced deposit code support
/// </summary>
[ApiController]
[Route("api/wallets")]
[Authorize]
public class WalletController : BaseController
{
    private readonly IWalletService _walletService;
    private readonly IPaymentGatewayClient _paymentGatewayClient;
    private readonly ILogger<WalletController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletController"/> class
    /// </summary>
    /// <param name="walletService">Wallet service</param>
    /// <param name="paymentGatewayClient">Payment gateway client for deposit code validation</param>
    /// <param name="logger">Logger</param>
    public WalletController(
        IWalletService walletService,
        IPaymentGatewayClient paymentGatewayClient,
        ILogger<WalletController> logger)
    {
        _walletService = walletService;
        _paymentGatewayClient = paymentGatewayClient;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets the wallet balances for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet balances</returns>
    [HttpGet("balances")]
    public async Task<ActionResult<IEnumerable<WalletBalance>>> GetCurrentUserWalletBalancesAsync(CancellationToken cancellationToken = default)
    {
        var walletBalances = await _walletService.GetCurrentUserWalletBalancesAsync(cancellationToken);
        return Ok(walletBalances);
    }
    
    /// <summary>
    /// Gets the wallet balance for a specific wallet
    /// </summary>
    /// <param name="walletId">Wallet ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet balance</returns>
    [HttpGet("{walletId}/balance")]
    public async Task<ActionResult<WalletBalance>> GetWalletBalanceAsync(string walletId, CancellationToken cancellationToken = default)
    {
        var walletBalance = await _walletService.GetWalletBalanceAsync(walletId, cancellationToken);
        return Ok(walletBalance);
    }
    
    /// <summary>
    /// Gets the wallet transactions for the current user
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet transactions</returns>
    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<WalletTransaction>>> GetCurrentUserWalletTransactionsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var walletTransactions = await _walletService.GetCurrentUserWalletTransactionsAsync(page, pageSize, cancellationToken);
        return Ok(walletTransactions);
    }
    
    /// <summary>
    /// Gets the wallet transactions for a specific wallet
    /// </summary>
    /// <param name="walletId">Wallet ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet transactions</returns>
    [HttpGet("{walletId}/transactions")]
    public async Task<ActionResult<IEnumerable<WalletTransaction>>> GetWalletTransactionsByWalletIdAsync(string walletId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var walletTransactions = await _walletService.GetWalletTransactionsByWalletIdAsync(walletId, page, pageSize, cancellationToken);
        return Ok(walletTransactions);
    }
    
    /// <summary>
    /// Gets a specific wallet transaction
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet transaction</returns>
    [HttpGet("transactions/{transactionId}")]
    public async Task<ActionResult<WalletTransaction>> GetWalletTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var walletTransaction = await _walletService.GetWalletTransactionAsync(transactionId, cancellationToken);
        return Ok(walletTransaction);
    }
    
    /// <summary>
    /// Transfers funds between wallets
    /// </summary>
    /// <param name="request">Transfer request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transfer response</returns>
    [HttpPost("transfer")]
    public async Task<ActionResult<TransferResponse>> TransferAsync([FromBody] TransferRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _walletService.TransferAsync(request, cancellationToken);
        return Ok(response);
    }
    
    /// <summary>
    /// Withdraws funds from a wallet
    /// </summary>
    /// <param name="request">Withdraw request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Withdraw response</returns>
    [HttpPost("withdraw")]
    public async Task<ActionResult<WithdrawResponse>> WithdrawAsync([FromBody] WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _walletService.WithdrawAsync(request, cancellationToken);
        return Ok(response);
    }
    
    /// <summary>
    /// Gets a deposit address for a wallet with enhanced deposit code validation
    /// </summary>
    /// <param name="request">Enhanced deposit request with deposit code support</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deposit response with validation results</returns>
    /// <response code="200">Deposit address generated successfully</response>
    /// <response code="400">Invalid request or deposit code validation failed</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(EnhancedDepositResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EnhancedDepositResponse>> GetDepositAddressAsync([FromBody] DepositRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        _logger.LogInformation("Enhanced deposit address requested. CorrelationId: {CorrelationId}, UserId: {UserId}, PaymentMethod: {PaymentMethod}, DepositCode: {HasDepositCode}", 
            correlationId, CurrentUser?.UserId, request.PaymentMethod, !string.IsNullOrEmpty(request.DepositCode));

        try
        {
            var enhancedResponse = new EnhancedDepositResponse
            {
                CorrelationId = correlationId,
                RequestTimestamp = DateTime.UtcNow
            };

            // Validate deposit code if provided (required for fiat deposits)
            if (!string.IsNullOrEmpty(request.DepositCode))
            {
                if (request.Amount == null)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Amount Required",
                        Detail = "Amount is required when using a deposit code",
                        Status = StatusCodes.Status400BadRequest,
                        Extensions = { ["correlationId"] = correlationId }
                    });
                }

                var validationRequest = new DepositCodeValidationRequest
                {
                    DepositCode = request.DepositCode.ToUpperInvariant(),
                    Amount = request.Amount.Value,
                    Currency = request.CurrencyCode,
                    IsRealTimeValidation = true
                };

                var validationResult = await _paymentGatewayClient.ValidateDepositCodeAsync(validationRequest, cancellationToken);
                enhancedResponse.DepositCodeValidation = validationResult;

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Deposit code validation failed. CorrelationId: {CorrelationId}, Error: {Error}", 
                        correlationId, validationResult.ErrorMessage);
                    
                    enhancedResponse.Success = false;
                    enhancedResponse.ErrorMessage = "Deposit code validation failed: " + validationResult.ErrorMessage;
                    
                    return BadRequest(enhancedResponse);
                }

                _logger.LogInformation("Deposit code validated successfully. CorrelationId: {CorrelationId}", correlationId);
            }
            else if (request.PaymentMethod == DepositPaymentMethod.Fiat)
            {
                // Default to auto-generate for fiat if no code was provided
                if (!request.AutoGenerateCode && string.IsNullOrEmpty(request.DepositCode))
                {
                    request.AutoGenerateCode = true;
                }

                // Auto-generate deposit code for fiat deposits if not provided
                if (request.AutoGenerateCode && string.IsNullOrEmpty(request.DepositCode))
                {
                    var generationRequest = new DepositCodeGenerationRequest
                    {
                        Amount = request.Amount,
                        Currency = request.CurrencyCode,
                        GenerateQrCode = true,
                        Metadata = request.Metadata
                    };

                    var generationResult = await _paymentGatewayClient.GenerateDepositCodeAsync(generationRequest, cancellationToken);
                    enhancedResponse.GeneratedDepositCode = generationResult;

                    if (generationResult.Success)
                    {
                        request.DepositCode = generationResult.DepositCode;
                        _logger.LogInformation("Auto-generated deposit code for fiat deposit. CorrelationId: {CorrelationId}, DepositCode: {DepositCode}", 
                            correlationId, generationResult.DepositCode);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to auto-generate deposit code. CorrelationId: {CorrelationId}, Error: {Error}", 
                            correlationId, generationResult.ErrorMessage);
                    }
                }
            }

            // Process the deposit through the wallet service
            var depositResponse = await _walletService.GetDepositAddressAsync(request, cancellationToken);
            
            // Enhance the response with validation results and mobile features
            enhancedResponse.Success = true;
            enhancedResponse.DepositAddress = depositResponse.DepositAddress;
            enhancedResponse.TransactionId = depositResponse.TransactionId;
            enhancedResponse.EstimatedConfirmationTime = depositResponse.EstimatedConfirmationTime;
            enhancedResponse.ExpiryTime = depositResponse.ExpiryTime;
            enhancedResponse.MinimumAmount = depositResponse.MinimumAmount;
            enhancedResponse.MaximumAmount = depositResponse.MaximumAmount;
            enhancedResponse.NetworkFee = depositResponse.NetworkFee;
            enhancedResponse.Instructions = depositResponse.Instructions;

            // Add mobile-specific features
            enhancedResponse.MobileFeatures = new DepositMobileFeatures
            {
                QrCodeDataUrl = GenerateDepositQrCode(depositResponse.DepositAddress, request.Amount, request.CurrencyCode),
                OfflineSupported = request.PaymentMethod == DepositPaymentMethod.Crypto,
                BiometricRequired = request.Amount > 1000,
                PushNotificationsEnabled = true,
                DeepLinkUrl = GenerateDeepLink(depositResponse.DepositAddress, request.Amount, request.CurrencyCode)
            };

            // Provide provider info for Square to the mobile client
            if (request.PaymentMethod == DepositPaymentMethod.Fiat && request.Amount.HasValue)
            {
                var captureMode = request.Metadata?.CaptureMode?.ToLowerInvariant() ?? "client";
                if (captureMode == "payment_link")
                {
                    var (checkoutUrl, expiresAt, referenceId, error) = await _paymentGatewayClient.CreateSquarePaymentLinkAsync(
                        request.Amount.Value,
                        request.CurrencyCode,
                        request.ReferenceId,
                        request.Metadata?.Email,
                        cancellationToken);

                    enhancedResponse.Provider = new ProviderInfo
                    {
                        ProviderId = "SQUARE",
                        CaptureMode = "payment_link",
                        Parameters = new Dictionary<string, object>
                        {
                            ["checkoutUrl"] = checkoutUrl ?? string.Empty,
                            ["expiresAt"] = expiresAt?.ToString("O") ?? string.Empty,
                            ["referenceId"] = referenceId ?? string.Empty,
                            ["error"] = error ?? string.Empty
                        }
                    };
                }
                else
                {
                    var parameters = await _paymentGatewayClient.GetSquareClientParamsAsync(
                        request.Amount.Value, request.CurrencyCode, request.ReferenceId, cancellationToken);

                    enhancedResponse.Provider = new ProviderInfo
                    {
                        ProviderId = "SQUARE",
                        CaptureMode = "client",
                        Parameters = parameters
                    };
                }
            }

            // If deposit code was used, process through PaymentGatewayService
            if (!string.IsNullOrEmpty(request.DepositCode))
            {
                var enhancedDepositRequest = new EnhancedDepositRequest
                {
                    WalletId = request.WalletId,
                    CurrencyCode = request.CurrencyCode,
                    BlockchainNetwork = request.BlockchainNetwork,
                    ReferenceId = request.ReferenceId,
                    DepositCode = request.DepositCode,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    AutoGenerateCode = request.AutoGenerateCode,
                    Metadata = request.Metadata,
                    UseRealTimeValidation = true,
                    ClientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                };

                var processingResult = await _paymentGatewayClient.ProcessDepositWithCodeAsync(enhancedDepositRequest, cancellationToken);
                enhancedResponse.DepositProcessing = processingResult;

                if (!processingResult.Success)
                {
                    _logger.LogWarning("Deposit processing with code failed. CorrelationId: {CorrelationId}, Error: {Error}", 
                        correlationId, processingResult.ErrorMessage);
                }
            }

            _logger.LogInformation("Enhanced deposit address request completed successfully. CorrelationId: {CorrelationId}", correlationId);
            return Ok(enhancedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing enhanced deposit request. CorrelationId: {CorrelationId}", correlationId);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while processing the deposit request",
                Status = StatusCodes.Status500InternalServerError,
                Extensions = { ["correlationId"] = correlationId }
            });
        }
    }

    /// <summary>
    /// Validates a deposit code before initiating a deposit
    /// </summary>
    /// <param name="request">Deposit code pre-validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Pre-validation result for mobile UX optimization</returns>
    /// <response code="200">Pre-validation completed successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("deposit/pre-validate")]
    [ProducesResponseType(typeof(DepositPreValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DepositPreValidationResponse>> PreValidateDepositAsync([FromBody] DepositPreValidationRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        _logger.LogInformation("Deposit pre-validation requested. CorrelationId: {CorrelationId}, UserId: {UserId}, DepositCode: {DepositCode}", 
            correlationId, CurrentUser?.UserId, request.DepositCode);

        try
        {
            var validationRequest = new DepositCodeValidationRequest
            {
                DepositCode = request.DepositCode.ToUpperInvariant(),
                Amount = request.Amount,
                Currency = request.Currency,
                IsRealTimeValidation = true
            };

            var validationResult = await _paymentGatewayClient.ValidateDepositCodeAsync(validationRequest, cancellationToken);

            var preValidationResponse = new DepositPreValidationResponse
            {
                CorrelationId = correlationId,
                IsValid = validationResult.IsValid,
                ValidationDetails = validationResult.ValidationDetails,
                ErrorMessage = validationResult.ErrorMessage,
                SuggestedActions = validationResult.SuggestedActions ?? new List<string>(),
                CanProceedWithDeposit = validationResult.IsValid,
                EstimatedProcessingTime = validationResult.IsValid ? TimeSpan.FromMinutes(5) : null,
                RequiredVerifications = GetRequiredVerifications(request.Amount),
                MobileOptimizations = new MobileValidationOptimizations
                {
                    ShowAmountField = validationResult.IsValid && validationResult.ValidationDetails?.ExpectedAmount == null,
                    ShowCurrencySelector = validationResult.IsValid && string.IsNullOrEmpty(validationResult.ValidationDetails?.ExpectedCurrency),
                    EnableBiometricAuth = request.Amount > 1000,
                    SuggestOfflineMode = false
                }
            };

            _logger.LogInformation("Deposit pre-validation completed. CorrelationId: {CorrelationId}, IsValid: {IsValid}", 
                correlationId, preValidationResponse.IsValid);

            return Ok(preValidationResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during deposit pre-validation. CorrelationId: {CorrelationId}", correlationId);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred during pre-validation",
                Status = StatusCodes.Status500InternalServerError,
                Extensions = { ["correlationId"] = correlationId }
            });
        }
    }

    /// <summary>
    /// Generates a QR code data URL for deposit address
    /// </summary>
    private static string? GenerateDepositQrCode(string? depositAddress, decimal? amount, string currency)
    {
        if (string.IsNullOrEmpty(depositAddress))
            return null;

        // Generate QR code content (simplified - in production, use proper QR code library)
        var qrContent = amount.HasValue ? 
            $"{currency.ToUpperInvariant()}:{depositAddress}?amount={amount.Value}" : 
            $"{currency.ToUpperInvariant()}:{depositAddress}";
            
        // Return placeholder data URL (in production, generate actual QR code image)
        return $"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";
    }

    /// <summary>
    /// Generates a deep link URL for mobile app integration
    /// </summary>
    private static string GenerateDeepLink(string? depositAddress, decimal? amount, string currency)
    {
        if (string.IsNullOrEmpty(depositAddress))
            return string.Empty;

        var baseUrl = "quantumskylink://deposit";
        var parameters = new List<string>
        {
            $"address={Uri.EscapeDataString(depositAddress)}",
            $"currency={Uri.EscapeDataString(currency)}"
        };

        if (amount.HasValue)
        {
            parameters.Add($"amount={amount.Value}");
        }

        return $"{baseUrl}?{string.Join("&", parameters)}";
    }

    /// <summary>
    /// Gets required verifications based on deposit amount
    /// </summary>
    private static List<string> GetRequiredVerifications(decimal amount)
    {
        var verifications = new List<string>();

        if (amount > 1000)
        {
            verifications.Add("Biometric authentication");
        }

        if (amount > 10000)
        {
            verifications.Add("Enhanced KYC verification");
            verifications.Add("Source of funds documentation");
        }

        return verifications;
    }
}
