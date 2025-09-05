using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TreasuryService.Services.Interfaces;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;
using System.Security.Claims;

namespace TreasuryService.Controllers;

/// <summary>
/// Controller for treasury account management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TreasuryAccountController : ControllerBase
{
    private readonly ITreasuryAccountService _accountService;
    private readonly ILogger<TreasuryAccountController> _logger;

    public TreasuryAccountController(
        ITreasuryAccountService accountService,
        ILogger<TreasuryAccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new treasury account
    /// </summary>
    /// <param name="request">Treasury account creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created treasury account details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TreasuryAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryAccountResponse>> CreateAccount(
        [FromBody] CreateTreasuryAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating treasury account: {AccountName}, Type: {AccountType}", 
                request.AccountName, request.AccountType);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.CreateAccountAsync(request, cancellationToken);
            
            _logger.LogInformation("Treasury account created successfully: {AccountId}", result.Id);
            return CreatedAtAction(nameof(GetAccount), new { accountId = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid treasury account creation request");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Treasury account creation failed");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating treasury account");
            return StatusCode(500, "An error occurred while creating the treasury account");
        }
    }

    /// <summary>
    /// Get treasury account by ID
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Treasury account details</returns>
    [HttpGet("{accountId:guid}")]
    [ProducesResponseType(typeof(TreasuryAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryAccountResponse>> GetAccount(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting treasury account: {AccountId}", accountId);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            var account = await _accountService.GetAccountAsync(accountId, cancellationToken);
            return Ok(account);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while retrieving the treasury account");
        }
    }

    /// <summary>
    /// Get treasury accounts with filtering and pagination
    /// </summary>
    /// <param name="accountType">Optional account type filter</param>
    /// <param name="currency">Optional currency filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of treasury accounts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TreasuryAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TreasuryAccountResponse>>> GetAccounts(
        [FromQuery] string? accountType = null,
        [FromQuery] string? currency = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting treasury accounts - Type: {AccountType}, Currency: {Currency}, Status: {Status}, Page: {Page}, Size: {PageSize}", 
                accountType, currency, status, page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var request = new GetTreasuryAccountsRequest
            {
                AccountType = accountType,
                Currency = currency,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var accounts = await _accountService.GetAccountsAsync(request, cancellationToken);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury accounts");
            return StatusCode(500, "An error occurred while retrieving treasury accounts");
        }
    }

    /// <summary>
    /// Update treasury account
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="request">Treasury account update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated treasury account details</returns>
    [HttpPut("{accountId:guid}")]
    [ProducesResponseType(typeof(TreasuryAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryAccountResponse>> UpdateAccount(
        Guid accountId,
        [FromBody] UpdateTreasuryAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating treasury account: {AccountId}", accountId);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.UpdateAccountAsync(accountId, request, cancellationToken);
            
            _logger.LogInformation("Treasury account updated successfully: {AccountId}", accountId);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while updating the treasury account");
        }
    }

    /// <summary>
    /// Deactivate treasury account
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{accountId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> DeactivateAccount(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating treasury account: {AccountId}", accountId);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            var result = await _accountService.DeactivateAccountAsync(accountId, cancellationToken);
            
            if (!result)
            {
                return NotFound($"Treasury account not found or already deactivated: {accountId}");
            }

            _logger.LogInformation("Treasury account deactivated successfully: {AccountId}", accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while deactivating the treasury account");
        }
    }

    /// <summary>
    /// Freeze treasury account
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="reason">Reason for freezing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{accountId:guid}/freeze")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> FreezeAccount(
        Guid accountId,
        [FromBody] string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Freezing treasury account: {AccountId}, Reason: {Reason}", accountId, reason);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Reason is required for freezing account");
            }

            var result = await _accountService.FreezeAccountAsync(accountId, reason, cancellationToken);
            
            if (!result)
            {
                return NotFound($"Treasury account not found or already frozen: {accountId}");
            }

            _logger.LogInformation("Treasury account frozen successfully: {AccountId}", accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error freezing treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while freezing the treasury account");
        }
    }

    /// <summary>
    /// Unfreeze treasury account
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{accountId:guid}/unfreeze")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> UnfreezeAccount(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unfreezing treasury account: {AccountId}", accountId);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            var result = await _accountService.UnfreezeAccountAsync(accountId, cancellationToken);
            
            if (!result)
            {
                return NotFound($"Treasury account not found or not frozen: {accountId}");
            }

            _logger.LogInformation("Treasury account unfrozen successfully: {AccountId}", accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfreezing treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while unfreezing the treasury account");
        }
    }

    /// <summary>
    /// Close treasury account
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="reason">Reason for closing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{accountId:guid}/close")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> CloseAccount(
        Guid accountId,
        [FromBody] string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Closing treasury account: {AccountId}, Reason: {Reason}", accountId, reason);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Reason is required for closing account");
            }

            var result = await _accountService.CloseAccountAsync(accountId, reason, cancellationToken);
            
            if (!result)
            {
                return NotFound($"Treasury account not found or already closed: {accountId}");
            }

            _logger.LogInformation("Treasury account closed successfully: {AccountId}", accountId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while closing the treasury account");
        }
    }

    /// <summary>
    /// Set minimum balance for treasury account
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="minimumBalance">Minimum balance amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated treasury account</returns>
    [HttpPut("{accountId:guid}/minimum-balance")]
    [ProducesResponseType(typeof(TreasuryAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryAccountResponse>> SetMinimumBalance(
        Guid accountId,
        [FromBody] decimal minimumBalance,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting minimum balance for treasury account: {AccountId}, Amount: {MinimumBalance}", 
                accountId, minimumBalance);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            if (minimumBalance < 0)
            {
                return BadRequest("Minimum balance cannot be negative");
            }

            var result = await _accountService.SetMinimumBalanceAsync(accountId, minimumBalance, cancellationToken);
            
            _logger.LogInformation("Minimum balance set successfully for treasury account: {AccountId}", accountId);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting minimum balance for treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while setting minimum balance");
        }
    }

    /// <summary>
    /// Validate treasury account
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpGet("{accountId:guid}/validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidateAccount(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating treasury account: {AccountId}", accountId);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            var result = await _accountService.ValidateAccountAsync(accountId, cancellationToken);
            
            _logger.LogInformation("Treasury account validation completed: {AccountId}, Valid: {IsValid}", accountId, result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating treasury account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while validating the treasury account");
        }
    }

    /// <summary>
    /// Get user treasury accounts
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user treasury accounts</returns>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TreasuryAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TreasuryAccountResponse>>> GetUserAccounts(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting treasury accounts for user: {UserId}", userId);

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty");
            }

            var accounts = await _accountService.GetUserAccountsAsync(userId, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} treasury accounts for user: {UserId}", accounts.Count(), userId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury accounts for user: {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user treasury accounts");
        }
    }
}
