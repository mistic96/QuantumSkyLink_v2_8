using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.MoonPay;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Integrations;
using PaymentGatewayService.Services.Interfaces;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace PaymentGatewayService.Controllers;

/// <summary>
/// Controller for MoonPay fiat-to-crypto and crypto-to-fiat operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoonPayController : ControllerBase
{
    private readonly IMoonPayService _moonPayService;
    private readonly IPaymentService _paymentService;
    private readonly PaymentDbContext _context;
    private readonly ILogger<MoonPayController> _logger;

    public MoonPayController(
        IMoonPayService moonPayService,
        IPaymentService paymentService,
        PaymentDbContext context,
        ILogger<MoonPayController> logger)
    {
        _moonPayService = moonPayService;
        _paymentService = paymentService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a fiat-to-crypto transaction (buy crypto)
    /// </summary>
    /// <param name="request">Buy transaction details</param>
    /// <returns>Transaction response with widget URL</returns>
    [HttpPost("buy")]
    [ProducesResponseType(typeof(MoonPayTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MoonPayTransactionResponse>> CreateBuyTransaction([FromBody] CreateMoonPayBuyRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user ID in token");
            }

            _logger.LogInformation("Creating MoonPay buy transaction for user {UserId}, {Amount} {Currency} to {CryptoCurrency}", 
                userId, request.Amount, request.FiatCurrency, request.CryptoCurrency);

            // Find MoonPay gateway
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.GatewayType == PaymentGatewayType.MoonPay && g.IsActive);

            if (gateway == null)
            {
                return BadRequest("MoonPay gateway is not configured or active");
            }

            // Create payment record for tracking
            var payment = new Payment
            {
                UserId = userId,
                PaymentGatewayId = gateway.Id,
                PaymentGateway = gateway.GatewayType,
                Amount = request.Amount,
                Currency = request.FiatCurrency,
                Type = PaymentType.Deposit,
                Status = PaymentStatus.Pending,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["cryptoCurrency"] = request.CryptoCurrency,
                    ["walletAddress"] = request.WalletAddress,
                    ["transactionType"] = "buy",
                    ["paymentMethod"] = request.PaymentMethod ?? "credit_debit_card"
                })
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Prepare MoonPay request
            var moonPayRequest = new MoonPayBuyRequest
            {
                CurrencyCode = request.CryptoCurrency,
                BaseCurrencyAmount = request.Amount,
                WalletAddress = request.WalletAddress,
                BaseCurrencyCode = request.FiatCurrency,
                AreFeesIncluded = request.IncludeFees,
                ExternalTransactionId = payment.Id.ToString(),
                Email = request.Email,
                ReturnUrl = request.ReturnUrl,
                PaymentMethod = request.PaymentMethod,
                Theme = request.Theme != null ? new MoonPayTheme
                {
                    PrimaryColor = request.Theme.PrimaryColor,
                    BackgroundColor = request.Theme.BackgroundColor,
                    BorderRadius = request.Theme.BorderRadius
                } : null
            };

            // Create transaction through MoonPay
            var result = await _moonPayService.CreateBuyTransactionAsync(moonPayRequest);

            // Update payment with MoonPay response
            payment.GatewayTransactionId = result.Id;
            payment.Status = result.Status.ToLower() switch
            {
                "completed" => PaymentStatus.Completed,
                "pending" => PaymentStatus.Pending,
                "failed" => PaymentStatus.Failed,
                _ => PaymentStatus.Processing
            };

            // Store additional details in metadata
            var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(payment.Metadata ?? "{}") ?? new();
            metadata["moonPayTransactionId"] = result.Id;
            metadata["widgetUrl"] = result.WidgetUrl ?? string.Empty;
            metadata["exchangeRate"] = result.ExchangeRate?.ToString() ?? string.Empty;
            metadata["cryptoAmount"] = result.CryptoAmount?.ToString() ?? string.Empty;
            payment.Metadata = JsonSerializer.Serialize(metadata);

            await _context.SaveChangesAsync();

            _logger.LogInformation("MoonPay buy transaction created. PaymentId: {PaymentId}, MoonPayId: {MoonPayId}, Status: {Status}", 
                payment.Id, result.Id, result.Status);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoonPay buy transaction");
            return StatusCode(500, new { error = "Failed to create transaction", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a crypto-to-fiat transaction (sell crypto)
    /// </summary>
    /// <param name="request">Sell transaction details</param>
    /// <returns>Transaction response</returns>
    [HttpPost("sell")]
    [ProducesResponseType(typeof(MoonPayTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MoonPayTransactionResponse>> CreateSellTransaction([FromBody] CreateMoonPaySellRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user ID in token");
            }

            _logger.LogInformation("Creating MoonPay sell transaction for user {UserId}, {Amount} {CryptoCurrency} to {FiatCurrency}", 
                userId, request.CryptoAmount, request.CryptoCurrency, request.FiatCurrency);

            // Find MoonPay gateway
            var gateway = await _context.PaymentGateways
                .FirstOrDefaultAsync(g => g.GatewayType == PaymentGatewayType.MoonPay && g.IsActive);

            if (gateway == null)
            {
                return BadRequest("MoonPay gateway is not configured or active");
            }

            // Create payment record
            var payment = new Payment
            {
                UserId = userId,
                PaymentGatewayId = gateway.Id,
                PaymentGateway = gateway.GatewayType,
                Amount = request.CryptoAmount,
                Currency = request.CryptoCurrency,
                Type = PaymentType.Withdrawal,
                Status = PaymentStatus.Pending,
                Metadata = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    ["fiatCurrency"] = request.FiatCurrency,
                    ["transactionType"] = "sell",
                    ["bankAccount"] = request.BankAccount ?? new object()
                })
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Prepare MoonPay request
            var moonPayRequest = new MoonPaySellRequest
            {
                BaseCurrencyCode = request.CryptoCurrency,
                BaseCurrencyAmount = request.CryptoAmount,
                QuoteCurrencyCode = request.FiatCurrency,
                ExternalTransactionId = payment.Id.ToString(),
                Email = request.Email,
                BankAccount = request.BankAccount
            };

            // Create sell transaction
            var result = await _moonPayService.CreateSellTransactionAsync(moonPayRequest);

            // Update payment
            payment.GatewayTransactionId = result.Id;
            payment.Status = result.Status.ToLower() switch
            {
                "completed" => PaymentStatus.Completed,
                "pending" => PaymentStatus.Pending,
                "failed" => PaymentStatus.Failed,
                _ => PaymentStatus.Processing
            };

            await _context.SaveChangesAsync();

            _logger.LogInformation("MoonPay sell transaction created. PaymentId: {PaymentId}, MoonPayId: {MoonPayId}", 
                payment.Id, result.Id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoonPay sell transaction");
            return StatusCode(500, new { error = "Failed to create sell transaction", message = ex.Message });
        }
    }

    /// <summary>
    /// Get transaction status
    /// </summary>
    /// <param name="transactionId">MoonPay transaction ID</param>
    /// <returns>Transaction status details</returns>
    [HttpGet("transaction/{transactionId}")]
    [ProducesResponseType(typeof(MoonPayTransactionStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MoonPayTransactionStatus>> GetTransactionStatus(string transactionId)
    {
        try
        {
            _logger.LogInformation("Getting MoonPay transaction status for {TransactionId}", transactionId);

            var result = await _moonPayService.GetTransactionStatusAsync(transactionId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay transaction status");
            return StatusCode(500, new { error = "Failed to get transaction status", message = ex.Message });
        }
    }

    /// <summary>
    /// Get supported cryptocurrencies
    /// </summary>
    /// <param name="fiatCurrency">Optional fiat currency filter</param>
    /// <returns>List of supported cryptocurrencies</returns>
    [HttpGet("currencies")]
    [ProducesResponseType(typeof(List<MoonPayCurrency>), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<List<MoonPayCurrency>>> GetSupportedCurrencies([FromQuery] string? fiatCurrency = null)
    {
        try
        {
            _logger.LogInformation("Getting MoonPay supported currencies for fiat {FiatCurrency}", fiatCurrency);

            var result = await _moonPayService.GetSupportedCurrenciesAsync(fiatCurrency);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay currencies");
            return StatusCode(500, new { error = "Failed to get currencies", message = ex.Message });
        }
    }

    /// <summary>
    /// Get exchange rate quote
    /// </summary>
    /// <param name="cryptoCurrency">Cryptocurrency code</param>
    /// <param name="fiatCurrency">Fiat currency code</param>
    /// <returns>Current exchange rate and fees</returns>
    [HttpGet("quote")]
    [ProducesResponseType(typeof(MoonPayQuote), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<MoonPayQuote>> GetQuote([FromQuery] string cryptoCurrency, [FromQuery] string fiatCurrency)
    {
        try
        {
            _logger.LogInformation("Getting MoonPay quote for {Crypto}/{Fiat}", cryptoCurrency, fiatCurrency);

            var result = await _moonPayService.GetQuoteAsync(cryptoCurrency, fiatCurrency);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay quote");
            return StatusCode(500, new { error = "Failed to get quote", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a widget session for embedded UI
    /// </summary>
    /// <param name="request">Widget configuration</param>
    /// <returns>Widget session with URL</returns>
    [HttpPost("widget")]
    [ProducesResponseType(typeof(MoonPayWidgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MoonPayWidgetResponse>> CreateWidgetSession([FromBody] CreateMoonPayWidgetRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            _logger.LogInformation("Creating MoonPay widget session for user {UserId}, flow {Flow}", userId, request.Flow);

            var widgetRequest = new MoonPayWidgetRequest
            {
                Flow = request.Flow,
                DefaultCurrencyCode = request.DefaultCryptoCurrency,
                DefaultBaseCurrencyAmount = request.DefaultAmount,
                WalletAddress = request.WalletAddress,
                Email = request.Email ?? userEmail,
                ExternalCustomerId = userId,
                Theme = request.Theme,
                Language = request.Language,
                ColorCode = request.ColorMode,
                CurrencyCode = request.LimitToCurrency,
                RedirectUrl = request.RedirectUrl
            };

            var result = await _moonPayService.CreateWidgetSessionAsync(widgetRequest);

            _logger.LogInformation("MoonPay widget session created. SessionId: {SessionId}", result.SessionId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoonPay widget session");
            return StatusCode(500, new { error = "Failed to create widget session", message = ex.Message });
        }
    }

    /// <summary>
    /// Screen a recipient for KYC/AML compliance
    /// </summary>
    /// <param name="request">Screening request</param>
    /// <returns>Screening result</returns>
    [HttpPost("screen")]
    [ProducesResponseType(typeof(MoonPayScreeningResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MoonPayScreeningResult>> ScreenRecipient([FromBody] MoonPayScreeningRequest request)
    {
        try
        {
            _logger.LogInformation("Screening recipient {Email} for MoonPay compliance", request.Email);

            var result = await _moonPayService.ScreenRecipientAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error screening recipient");
            return StatusCode(500, new { error = "Failed to screen recipient", message = ex.Message });
        }
    }

    /// <summary>
    /// Get transaction limits for a currency pair
    /// </summary>
    /// <param name="cryptoCurrency">Cryptocurrency code</param>
    /// <param name="fiatCurrency">Fiat currency code</param>
    /// <param name="paymentMethod">Payment method</param>
    /// <returns>Transaction limits and fees</returns>
    [HttpGet("limits")]
    [ProducesResponseType(typeof(MoonPayLimits), StatusCodes.Status200OK)]
    [AllowAnonymous] // Public endpoint
    public async Task<ActionResult<MoonPayLimits>> GetTransactionLimits(
        [FromQuery] string cryptoCurrency, 
        [FromQuery] string fiatCurrency, 
        [FromQuery] string paymentMethod = "credit_debit_card")
    {
        try
        {
            _logger.LogInformation("Getting MoonPay limits for {Crypto}/{Fiat} via {Method}", 
                cryptoCurrency, fiatCurrency, paymentMethod);

            var result = await _moonPayService.GetTransactionLimitsAsync(cryptoCurrency, fiatCurrency, paymentMethod);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MoonPay limits");
            return StatusCode(500, new { error = "Failed to get limits", message = ex.Message });
        }
    }
}

#region Request DTOs

public class CreateMoonPayBuyRequest
{
    public string CryptoCurrency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FiatCurrency { get; set; } = "USD";
    public string WalletAddress { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PaymentMethod { get; set; }
    public bool IncludeFees { get; set; } = false;
    public string? ReturnUrl { get; set; }
    public MoonPayThemeRequest? Theme { get; set; }
}

public class CreateMoonPaySellRequest
{
    public string CryptoCurrency { get; set; } = string.Empty;
    public decimal CryptoAmount { get; set; }
    public string FiatCurrency { get; set; } = "USD";
    public string? Email { get; set; }
    public MoonPayBankAccount? BankAccount { get; set; }
}

public class CreateMoonPayWidgetRequest
{
    public string Flow { get; set; } = "buy";
    public string? DefaultCryptoCurrency { get; set; }
    public decimal? DefaultAmount { get; set; }
    public string? WalletAddress { get; set; }
    public string? Email { get; set; }
    public MoonPayTheme? Theme { get; set; }
    public string? Language { get; set; }
    public string? ColorMode { get; set; }
    public string? LimitToCurrency { get; set; }
    public string? RedirectUrl { get; set; }
}

public class MoonPayThemeRequest
{
    public string? PrimaryColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? BorderRadius { get; set; }
}

#endregion
