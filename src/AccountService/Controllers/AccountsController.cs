using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        IAccountService accountService,
        ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new account for the authenticated user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            request.UserId = userId; // Override with authenticated user ID

            var account = await _accountService.CreateAccountAsync(request);
            
            _logger.LogInformation("Account created successfully: {AccountId} for user {UserId}", account.Id, userId);
            
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid account creation request: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account for user {UserId}", request.UserId);
            return StatusCode(500, new { error = "An error occurred while creating the account" });
        }
    }

    /// <summary>
    /// Internal endpoint to create a new account for a specified user (service-to-service).
    /// Bypasses authentication for initial onboarding. Intended for internal use only.
    /// </summary>
    [HttpPost("internal")]
    [AllowAnonymous]
    public async Task<ActionResult<AccountResponse>> CreateAccountInternal([FromBody] CreateAccountRequest request)
    {
        try
        {
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "UserId is required" });
            }

            var account = await _accountService.CreateAccountAsync(request);
            
            _logger.LogInformation("Internal account created successfully: {AccountId} for user {UserId}", account.Id, request.UserId);
            
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid internal account creation request: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating internal account for user {UserId}", request.UserId);
            return StatusCode(500, new { error = "An error occurred while creating the account" });
        }
    }

    /// <summary>
    /// Get account details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AccountResponse>> GetAccount(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the account
            if (!await _accountService.UserOwnsAccountAsync(userId, id))
            {
                return Forbid("You do not have access to this account");
            }

            var account = await _accountService.GetAccountAsync(id);
            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Account not found: {AccountId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {AccountId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the account" });
        }
    }

    /// <summary>
    /// Get account by account number
    /// </summary>
    [HttpGet("number/{accountNumber}")]
    public async Task<ActionResult<AccountResponse>> GetAccountByNumber(string accountNumber)
    {
        try
        {
            var account = await _accountService.GetAccountByNumberAsync(accountNumber);
            if (account == null)
            {
                return NotFound(new { error = "Account not found" });
            }

            var userId = GetCurrentUserId();
            
            // Verify user owns the account
            if (account.UserId != userId)
            {
                return Forbid("You do not have access to this account");
            }

            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account by number {AccountNumber}", accountNumber);
            return StatusCode(500, new { error = "An error occurred while retrieving the account" });
        }
    }

    /// <summary>
    /// Get all accounts for the authenticated user
    /// </summary>
    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<AccountSummaryResponse>>> GetUserAccounts()
    {
        try
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountService.GetUserAccountsAsync(userId);
            
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { error = "An error occurred while retrieving accounts" });
        }
    }

    /// <summary>
    /// Get accounts by type for the authenticated user
    /// </summary>
    [HttpGet("user/type/{accountType}")]
    public async Task<ActionResult<IEnumerable<AccountSummaryResponse>>> GetAccountsByType(AccountType accountType)
    {
        try
        {
            var userId = GetCurrentUserId();
            var accounts = await _accountService.GetAccountsByTypeAsync(userId, accountType);
            
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {AccountType} accounts for user {UserId}", accountType, GetCurrentUserId());
            return StatusCode(500, new { error = "An error occurred while retrieving accounts" });
        }
    }

    /// <summary>
    /// Update account status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<AccountResponse>> UpdateAccountStatus(Guid id, [FromBody] UpdateAccountStatusRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the account
            if (!await _accountService.UserOwnsAccountAsync(userId, id))
            {
                return Forbid("You do not have access to this account");
            }

            var account = await _accountService.UpdateAccountStatusAsync(id, request);
            
            _logger.LogInformation("Account status updated: {AccountId} -> {Status}", id, request.Status);
            
            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Account not found for status update: {AccountId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account status for {AccountId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the account status" });
        }
    }

    /// <summary>
    /// Get account balance
    /// </summary>
    [HttpGet("{id}/balance")]
    public async Task<ActionResult<AccountBalanceResponse>> GetAccountBalance(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the account
            if (!await _accountService.UserOwnsAccountAsync(userId, id))
            {
                return Forbid("You do not have access to this account");
            }

            var balance = await _accountService.GetAccountBalanceAsync(id);
            return Ok(balance);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Account not found for balance query: {AccountId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for account {AccountId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the account balance" });
        }
    }

    /// <summary>
    /// Delete (close) an account
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAccount(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the account
            if (!await _accountService.UserOwnsAccountAsync(userId, id))
            {
                return Forbid("You do not have access to this account");
            }

            var success = await _accountService.DeleteAccountAsync(id);
            if (!success)
            {
                return NotFound(new { error = "Account not found" });
            }

            _logger.LogInformation("Account closed: {AccountId} by user {UserId}", id, userId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account {AccountId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the account" });
        }
    }

    /// <summary>
    /// Check if account exists
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> AccountExists(Guid id)
    {
        try
        {
            var exists = await _accountService.AccountExistsAsync(id);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if account exists: {AccountId}", id);
            return StatusCode(500, new { error = "An error occurred while checking account existence" });
        }
    }

    /// <summary>
    /// Check if account is active
    /// </summary>
    [HttpGet("{id}/active")]
    public async Task<ActionResult<bool>> IsAccountActive(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the account
            if (!await _accountService.UserOwnsAccountAsync(userId, id))
            {
                return Forbid("You do not have access to this account");
            }

            var isActive = await _accountService.IsAccountActiveAsync(id);
            return Ok(isActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if account is active: {AccountId}", id);
            return StatusCode(500, new { error = "An error occurred while checking account status" });
        }
    }

    /// <summary>
    /// Get user account statistics
    /// </summary>
    [HttpGet("user/stats")]
    public async Task<ActionResult<object>> GetUserAccountStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var accountCount = await _accountService.GetUserAccountCountAsync(userId);
            var totalBalance = await _accountService.GetTotalUserBalanceAsync(userId);
            
            var stats = new
            {
                AccountCount = accountCount,
                TotalBalance = totalBalance,
                Currency = "USD"
            };
            
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account statistics for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { error = "An error occurred while retrieving account statistics" });
        }
    }

    /// <summary>
    /// Validate a transaction (for internal use)
    /// </summary>
    [HttpPost("{id}/validate-transaction")]
    public async Task<ActionResult<bool>> ValidateTransaction(Guid id, [FromBody] ValidateTransactionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the account
            if (!await _accountService.UserOwnsAccountAsync(userId, id))
            {
                return Forbid("You do not have access to this account");
            }

            var isValid = await _accountService.ValidateTransactionAsync(id, request.Amount, request.TransactionType);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating transaction for account {AccountId}", id);
            return StatusCode(500, new { error = "An error occurred while validating the transaction" });
        }
    }

    // Helper methods
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
}

// Additional request models for controller endpoints
public class ValidateTransactionRequest
{
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
}
