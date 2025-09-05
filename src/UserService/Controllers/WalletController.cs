using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletManagementService _walletManagementService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(IWalletManagementService walletManagementService, ILogger<WalletController> logger)
    {
        _walletManagementService = walletManagementService;
        _logger = logger;
    }

    /// <summary>
    /// Create wallet for user
    /// </summary>
    /// <param name="request">Wallet creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created wallet information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserWalletResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserWalletResponse>> CreateWallet(
        [FromBody] CreateWalletRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var wallet = await _walletManagementService.CreateWalletAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetWalletByAddress), new { address = wallet.WalletAddress }, wallet);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wallet");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get wallet by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet information</returns>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(UserWalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserWalletResponse>> GetWallet(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wallet = await _walletManagementService.GetWalletAsync(userId, cancellationToken);
            return Ok(wallet);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get wallet by wallet address
    /// </summary>
    /// <param name="address">Wallet address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet information</returns>
    [HttpGet("address/{address}")]
    [ProducesResponseType(typeof(UserWalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserWalletResponse>> GetWalletByAddress(
        string address,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var wallet = await _walletManagementService.GetWalletByAddressAsync(address, cancellationToken);
            return Ok(wallet);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet by address {Address}", address);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update wallet information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Updated wallet information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated wallet information</returns>
    [HttpPut("user/{userId:guid}")]
    [ProducesResponseType(typeof(UserWalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserWalletResponse>> UpdateWallet(
        Guid userId,
        [FromBody] UpdateWalletRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var wallet = await _walletManagementService.UpdateWalletAsync(userId, request, cancellationToken);
            return Ok(wallet);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating wallet for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get wallet balance
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet balance information</returns>
    [HttpGet("user/{userId:guid}/balance")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletBalanceResponse>> GetWalletBalance(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var balance = await _walletManagementService.GetWalletBalanceAsync(userId, cancellationToken);
            return Ok(balance);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet balance for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update wallet balance
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Balance update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated wallet balance</returns>
    [HttpPut("user/{userId:guid}/balance")]
    [ProducesResponseType(typeof(WalletBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletBalanceResponse>> UpdateWalletBalance(
        Guid userId,
        [FromBody] UpdateWalletBalanceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var balance = await _walletManagementService.UpdateWalletBalanceAsync(userId, request, cancellationToken);
            return Ok(balance);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating wallet balance for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Freeze wallet
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Freeze wallet request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("user/{userId:guid}/freeze")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FreezeWallet(
        Guid userId,
        [FromBody] FreezeWalletRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _walletManagementService.FreezeWalletAsync(userId, request.Reason, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Wallet not found" });
            }

            return Ok(new { message = "Wallet frozen successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error freezing wallet for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Unfreeze wallet
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("user/{userId:guid}/unfreeze")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnfreezeWallet(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _walletManagementService.UnfreezeWalletAsync(userId, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Wallet not found" });
            }

            return Ok(new { message = "Wallet unfrozen successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfreezing wallet for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate wallet address
    /// </summary>
    /// <param name="address">Wallet address to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate/{address}")]
    [ProducesResponseType(typeof(WalletValidationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletValidationResponse>> ValidateWalletAddress(
        string address,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isValid = await _walletManagementService.ValidateWalletAddressAsync(address, cancellationToken);
            return Ok(new WalletValidationResponse { IsValid = isValid, Address = address });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating wallet address {Address}", address);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get wallet transaction history
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet transaction history</returns>
    [HttpGet("user/{userId:guid}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<WalletTransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<WalletTransactionResponse>>> GetWalletTransactions(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _walletManagementService.GetWalletTransactionsAsync(userId, page, pageSize, cancellationToken);
            return Ok(transactions);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet transactions for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
