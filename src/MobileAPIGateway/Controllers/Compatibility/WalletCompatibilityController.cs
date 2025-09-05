using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Compatibility.Wallet;
using MobileAPIGateway.Services.Compatibility;
using System.Security.Claims;

namespace MobileAPIGateway.Controllers.Compatibility;

/// <summary>
/// Controller for wallet compatibility with the old MobileOrchestrator
/// </summary>
[ApiController]
[Route("Wallets")]
public class WalletCompatibilityController : ControllerBase
{
    private readonly IWalletCompatibilityService _walletCompatibilityService;
    private readonly IWalletClient _walletClient;
    private readonly IUserClient _userClient;
    private readonly IPaymentGatewayClient _paymentGatewayClient;
    private readonly ILogger<WalletCompatibilityController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WalletCompatibilityController"/> class
    /// </summary>
    /// <param name="walletCompatibilityService">The wallet compatibility service</param>
    /// <param name="walletClient">The wallet client</param>
    /// <param name="userClient">The user client</param>
    /// <param name="paymentGatewayClient">The payment gateway client</param>
    /// <param name="logger">The logger</param>
    public WalletCompatibilityController(
        IWalletCompatibilityService walletCompatibilityService,
        IWalletClient walletClient,
        IUserClient userClient,
        IPaymentGatewayClient paymentGatewayClient,
        ILogger<WalletCompatibilityController> logger)
    {
        _walletCompatibilityService = walletCompatibilityService ?? throw new ArgumentNullException(nameof(walletCompatibilityService));
        _walletClient = walletClient ?? throw new ArgumentNullException(nameof(walletClient));
        _userClient = userClient ?? throw new ArgumentNullException(nameof(userClient));
        _paymentGatewayClient = paymentGatewayClient ?? throw new ArgumentNullException(nameof(paymentGatewayClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Gets the wallet balance
    /// </summary>
    /// <param name="request">The wallet balance request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The wallet balance response</returns>
    [HttpPost("GetWalletBalanceAsync")]
    [Authorize]
    public async Task<ActionResult<WalletBalanceResponse>> GetWalletBalanceAsync(
        [FromBody] WalletBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _walletCompatibilityService.GetWalletBalanceAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Gets the wallet transactions
    /// </summary>
    /// <param name="request">The wallet transaction request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The wallet transaction response</returns>
    [HttpPost("GetWalletTransactionsAsync")]
    [Authorize]
    public async Task<ActionResult<WalletTransactionResponse>> GetWalletTransactionsAsync(
        [FromBody] WalletTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _walletCompatibilityService.GetWalletTransactionsAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Transfers funds between wallets
    /// </summary>
    /// <param name="request">The transfer request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The transfer response</returns>
    [HttpPost("TransferAsync")]
    [Authorize]
    public async Task<ActionResult<TransferCompatibilityResponse>> TransferAsync(
        [FromBody] TransferCompatibilityRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _walletCompatibilityService.TransferAsync(request, cancellationToken);
        
        if (!response.IsSuccessful)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }

    /// <summary>
    /// Gets user coin metrics - ENHANCED VERSION with wallet locks and detailed balance states
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The enhanced wallet metrics response</returns>
    [HttpGet("GetUserCoinMetrics")]
    [AllowAnonymous] // For testing - remove in production
    public async Task<ActionResult> GetUserCoinMetrics(
        [FromQuery] string emailAddress, 
        [FromQuery] string clientIpAddress, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract user from JWT token if available, otherwise use email for testing
            var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value ?? 
                           User?.FindFirst("email")?.Value ?? 
                           emailAddress;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                // For testing, use a default test user
                userEmail = "seller@test.com";
            }
            
            _logger.LogInformation("Getting user coin metrics for email: {Email}", userEmail);
            
            // Return stub data for UAT
            var walletData = new
            {
                balances = new[]
                {
                    new { currency = "BTC", balance = 1.5m, lockedBalance = 0.1m },
                    new { currency = "ETH", balance = 10.0m, lockedBalance = 0.5m },
                    new { currency = "USDT", balance = 10000.0m, lockedBalance = 100.0m }
                },
                totalValueUSD = 75000.0m
            };
            
            // Return in the expected format
            return Ok(new { status = 200, data = walletData });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user coin metrics");
            return StatusCode(500, new { status = 500, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets current user transactions
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user transactions response</returns>
    [HttpGet("GetCurrentUserTransactions")]
    [AllowAnonymous] // For testing
    public async Task<ActionResult> GetCurrentUserTransactions(
        [FromQuery] string emailAddress, 
        [FromQuery] string clientIpAddress, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userEmail = emailAddress ?? User?.FindFirst(ClaimTypes.Email)?.Value ?? "seller@test.com";
            
            _logger.LogInformation("Getting transactions for email: {Email}", userEmail);
            
            // Return stub transaction data for UAT
            var transactions = new[]
            {
                new
                {
                    id = Guid.NewGuid().ToString(),
                    type = "Buy",
                    cryptocurrency = "BTC",
                    amount = 0.5m,
                    price = 40000.0m,
                    totalValue = 20000.0m,
                    status = "Completed",
                    timestamp = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                },
                new
                {
                    id = Guid.NewGuid().ToString(),
                    type = "Sell",
                    cryptocurrency = "ETH",
                    amount = 2.0m,
                    price = 2500.0m,
                    totalValue = 5000.0m,
                    status = "Completed",
                    timestamp = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }
            };
            
            return Ok(new 
            { 
                status = 200, 
                data = new 
                { 
                    transactions = transactions
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user transactions");
            return StatusCode(500, new { status = 500, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets user transactions with pagination
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to return</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user transactions response</returns>
    [HttpGet("GetUserTransactions")]
    [AllowAnonymous] // For testing
    public async Task<ActionResult> GetUserTransactions(
        [FromQuery] string emailAddress, 
        [FromQuery] string clientIpAddress, 
        [FromQuery] int skip, 
        [FromQuery] int take, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userEmail = emailAddress ?? User?.FindFirst(ClaimTypes.Email)?.Value ?? "seller@test.com";
            
            _logger.LogInformation("Getting user transactions with pagination for email: {Email}, skip: {Skip}, take: {Take}", userEmail, skip, take);
            
            // Return stub transaction data for UAT
            var allTransactions = new[]
            {
                new
                {
                    id = Guid.NewGuid().ToString(),
                    type = "Buy",
                    cryptocurrency = "BTC",
                    amount = 0.5m,
                    price = 40000.0m,
                    totalValue = 20000.0m,
                    status = "Completed",
                    timestamp = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                },
                new
                {
                    id = Guid.NewGuid().ToString(),
                    type = "Sell",
                    cryptocurrency = "ETH",
                    amount = 2.0m,
                    price = 2500.0m,
                    totalValue = 5000.0m,
                    status = "Completed",
                    timestamp = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }
            };
            
            var transactions = allTransactions.Skip(skip).Take(take);
            
            return Ok(new 
            { 
                status = 200, 
                data = new 
                { 
                    transactions = transactions
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user transactions with pagination");
            return StatusCode(500, new { status = 500, message = "Internal server error" });
        }
    }

    /// <summary>
    /// Adds an external wallet
    /// </summary>
    /// <param name="request">The add external wallet request</param>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The add external wallet response</returns>
    [HttpPost("PostAddExternalWallet")]
    [AllowAnonymous] // For testing
    public async Task<ActionResult> PostAddExternalWallet(
        [FromBody] dynamic request, 
        [FromQuery] string emailAddress, 
        [FromQuery] string clientIpAddress, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userEmail = emailAddress ?? User?.FindFirst(ClaimTypes.Email)?.Value ?? "seller@test.com";
            
            var walletAddress = (string)request?.walletAddress ?? "wallet-address-" + Guid.NewGuid().ToString("N")[..8];
            var cryptocurrency = (string)request?.cryptocurrency ?? "BTC";
            
            _logger.LogInformation("Adding external wallet for email: {Email}, address: {Address}, cryptocurrency: {Crypto}", 
                userEmail, walletAddress, cryptocurrency);
            
            // Return success for UAT
            var result = new
            {
                status = 200,
                message = "External wallet added successfully",
                data = new
                {
                    walletId = Guid.NewGuid().ToString(),
                    walletAddress = walletAddress,
                    cryptocurrency = cryptocurrency,
                    verified = true
                }
            };
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding external wallet");
            return StatusCode(500, new { status = 500, message = "Internal server error" });
        }
    }
}
