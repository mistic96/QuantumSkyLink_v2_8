using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using Mapster;

namespace InfrastructureService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InfrastructureController : ControllerBase
{
    private readonly IInfrastructureService _infrastructureService;
    private readonly ILogger<InfrastructureController> _logger;

    public InfrastructureController(
        IInfrastructureService infrastructureService,
        ILogger<InfrastructureController> logger)
    {
        _infrastructureService = infrastructureService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }

    // Wallet Management Endpoints
    [HttpPost("wallets")]
    public async Task<ActionResult<WalletResponse>> CreateWallet([FromBody] CreateWalletRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            request.UserId = currentUserId; // Ensure user can only create wallets for themselves
            
            var wallet = await _infrastructureService.CreateWalletAsync(request);
            return Ok(wallet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wallet for user {UserId}", request.UserId);
            return StatusCode(500, "An error occurred while creating the wallet");
        }
    }

    [HttpGet("wallets/{walletId}")]
    public async Task<ActionResult<WalletResponse>> GetWallet(Guid walletId)
    {
        try
        {
            var wallet = await _infrastructureService.GetWalletAsync(walletId);
            
            // Ensure user can only access their own wallets
            var currentUserId = GetCurrentUserId();
            if (wallet.UserId != currentUserId)
            {
                return Forbid("You can only access your own wallets");
            }
            
            return Ok(wallet);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet {WalletId}", walletId);
            return StatusCode(500, "An error occurred while retrieving the wallet");
        }
    }

    [HttpGet("wallets/address/{address}")]
    public async Task<ActionResult<WalletResponse>> GetWalletByAddress(string address)
    {
        try
        {
            var wallet = await _infrastructureService.GetWalletByAddressAsync(address);
            
            // Ensure user can only access their own wallets
            var currentUserId = GetCurrentUserId();
            if (wallet.UserId != currentUserId)
            {
                return Forbid("You can only access your own wallets");
            }
            
            return Ok(wallet);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet by address {Address}", address);
            return StatusCode(500, "An error occurred while retrieving the wallet");
        }
    }

    [HttpGet("wallets")]
    public async Task<ActionResult<List<WalletResponse>>> GetUserWallets()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var wallets = await _infrastructureService.GetUserWalletsAsync(currentUserId);
            return Ok(wallets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallets for user");
            return StatusCode(500, "An error occurred while retrieving wallets");
        }
    }

    [HttpPut("wallets/{walletId}/status")]
    public async Task<ActionResult<WalletResponse>> UpdateWalletStatus(Guid walletId, [FromBody] UpdateWalletStatusRequest request)
    {
        try
        {
            // First check if wallet belongs to current user
            var wallet = await _infrastructureService.GetWalletAsync(walletId);
            var currentUserId = GetCurrentUserId();
            if (wallet.UserId != currentUserId)
            {
                return Forbid("You can only update your own wallets");
            }
            
            var updatedWallet = await _infrastructureService.UpdateWalletStatusAsync(walletId, request.Status);
            return Ok(updatedWallet);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating wallet status for wallet {WalletId}", walletId);
            return StatusCode(500, "An error occurred while updating the wallet status");
        }
    }

    [HttpDelete("wallets/{walletId}")]
    public async Task<ActionResult> DeleteWallet(Guid walletId)
    {
        try
        {
            // First check if wallet belongs to current user
            var wallet = await _infrastructureService.GetWalletAsync(walletId);
            var currentUserId = GetCurrentUserId();
            if (wallet.UserId != currentUserId)
            {
                return Forbid("You can only delete your own wallets");
            }
            
            var result = await _infrastructureService.DeleteWalletAsync(walletId);
            if (result)
            {
                return NoContent();
            }
            return NotFound("Wallet not found");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting wallet {WalletId}", walletId);
            return StatusCode(500, "An error occurred while deleting the wallet");
        }
    }

    // Wallet Balance Endpoints
    [HttpGet("wallets/{walletId}/balances")]
    public async Task<ActionResult<List<WalletBalanceResponse>>> GetWalletBalances(Guid walletId)
    {
        try
        {
            // First check if wallet belongs to current user
            var wallet = await _infrastructureService.GetWalletAsync(walletId);
            var currentUserId = GetCurrentUserId();
            if (wallet.UserId != currentUserId)
            {
                return Forbid("You can only access your own wallet balances");
            }
            
            var balances = await _infrastructureService.GetWalletBalancesAsync(walletId);
            return Ok(balances);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balances for wallet {WalletId}", walletId);
            return StatusCode(500, "An error occurred while retrieving wallet balances");
        }
    }

    [HttpGet("wallets/{walletId}/balances/{tokenSymbol}")]
    public async Task<ActionResult<WalletBalanceResponse>> GetWalletBalance(Guid walletId, string tokenSymbol)
    {
        try
        {
            // First check if wallet belongs to current user
            var wallet = await _infrastructureService.GetWalletAsync(walletId);
            var currentUserId = GetCurrentUserId();
            if (wallet.UserId != currentUserId)
            {
                return Forbid("You can only access your own wallet balances");
            }
            
            var balance = await _infrastructureService.GetWalletBalanceAsync(walletId, tokenSymbol);
            return Ok(balance);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {TokenSymbol} balance for wallet {WalletId}", tokenSymbol, walletId);
            return StatusCode(500, "An error occurred while retrieving wallet balance");
        }
    }

    [HttpPost("wallets/{walletId}/sync-balances")]
    public async Task<ActionResult> SyncWalletBalances(Guid walletId)
    {
        try
        {
            // First check if wallet belongs to current user
            var wallet = await _infrastructureService.GetWalletAsync(walletId);
            var currentUserId = GetCurrentUserId();
            if (wallet.UserId != currentUserId)
            {
                return Forbid("You can only sync your own wallet balances");
            }
            
            await _infrastructureService.SyncWalletBalancesAsync(walletId);
            return Ok(new { message = "Wallet balances synced successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing balances for wallet {WalletId}", walletId);
            return StatusCode(500, "An error occurred while syncing wallet balances");
        }
    }

    // Blockchain Utility Endpoints
    [HttpGet("blockchain/validate-address")]
    public async Task<ActionResult<bool>> ValidateAddress([FromQuery] string address, [FromQuery] string network = "Ethereum")
    {
        try
        {
            var isValid = await _infrastructureService.ValidateAddressAsync(address, network);
            return Ok(new { address, network, isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating address {Address} on network {Network}", address, network);
            return StatusCode(500, "An error occurred while validating the address");
        }
    }

    // Broadcast a signed raw transaction to the network.
    [HttpPost("broadcast")]
    public async Task<ActionResult<BroadcastResponse>> BroadcastSignedTransaction([FromBody] InfrastructureService.Models.Requests.BroadcastSignedTransactionRequest request)
    {
        try
        {
            // In production, you would validate caller permissions and transaction readiness.
            var result = await _infrastructureService.BroadcastSignedTransactionAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid broadcast request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting signed transaction");
            return StatusCode(500, "An error occurred while broadcasting the transaction");
        }
    }

    [HttpGet("blockchain/gas-price")]
    public async Task<ActionResult<decimal>> GetCurrentGasPrice([FromQuery] string network = "Ethereum")
    {
        try
        {
            var gasPrice = await _infrastructureService.GetCurrentGasPriceAsync(network);
            return Ok(new { network, gasPrice, unit = "Gwei" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gas price for network {Network}", network);
            return StatusCode(500, "An error occurred while retrieving gas price");
        }
    }

    // Statistics Endpoints
    [HttpGet("stats/wallets")]
    public async Task<ActionResult<WalletStatsResponse>> GetWalletStats()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var stats = await _infrastructureService.GetWalletStatsAsync(currentUserId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet stats for user");
            return StatusCode(500, "An error occurred while retrieving wallet statistics");
        }
    }

    [HttpGet("stats/networks")]
    public async Task<ActionResult<List<NetworkStatsResponse>>> GetNetworkStats()
    {
        try
        {
            var stats = await _infrastructureService.GetNetworkStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting network stats");
            return StatusCode(500, "An error occurred while retrieving network statistics");
        }
    }
}

// Additional request models
public class UpdateWalletStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
