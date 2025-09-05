using AccountService.Data;
using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Services;

public class AccountService : IAccountService
{
    private readonly AccountDbContext _context;
    private readonly IUserServiceClient _userServiceClient;
    private readonly IInfrastructureServiceClient _infrastructureServiceClient;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        AccountDbContext context,
        IUserServiceClient userServiceClient,
        IInfrastructureServiceClient infrastructureServiceClient,
        ILogger<AccountService> logger)
    {
        _context = context;
        _userServiceClient = userServiceClient;
        _infrastructureServiceClient = infrastructureServiceClient;
        _logger = logger;
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating account for user {UserId} with type {AccountType}", request.UserId, request.AccountType);

            // Validate user exists via UserService
            var userResponse = await _userServiceClient.UserExistsAsync(request.UserId, cancellationToken);
            if (!userResponse.IsSuccessStatusCode || !userResponse.Content)
            {
                throw new InvalidOperationException($"User {request.UserId} does not exist or is not accessible");
            }

            // Generate unique account number
            var accountNumber = await GenerateAccountNumberAsync(cancellationToken);

            // Create account entity
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                AccountNumber = accountNumber,
                AccountType = request.AccountType,
                Status = AccountStatus.Pending,
                Description = request.Description,
                Currency = request.Currency,
                DailyLimit = request.DailyLimit ?? GetDefaultDailyLimit(request.AccountType),
                MonthlyLimit = request.MonthlyLimit ?? GetDefaultMonthlyLimit(request.AccountType),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add to database
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Account {AccountId} created successfully with number {AccountNumber}", account.Id, account.AccountNumber);

            // Create blockchain wallet via Infrastructure Service
            try
            {
                var walletRequest = new CreateWalletRequest
                {
                    AccountId = account.Id,
                    UserId = request.UserId,
                    WalletType = "MultiSig",
                    Metadata = request.Metadata
                };

                var walletResponse = await _infrastructureServiceClient.CreateWalletAsync(walletRequest, cancellationToken);
                if (walletResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Blockchain wallet created for account {AccountId}: {WalletAddress}", 
                        account.Id, walletResponse.Content?.WalletAddress);
                }
                else
                {
                    _logger.LogWarning("Failed to create blockchain wallet for account {AccountId}: {Error}", 
                        account.Id, walletResponse.Error?.Content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blockchain wallet for account {AccountId}", account.Id);
                // Continue - wallet creation failure shouldn't fail account creation
            }

            // Create default account limits
            await CreateDefaultAccountLimitsAsync(account.Id, cancellationToken);

            // Auto-verify if requested and user is verified
            if (request.AutoVerify)
            {
                await TryAutoVerifyAccountAsync(account.Id, request.UserId, cancellationToken);
            }

            return account.Adapt<AccountResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create account for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<AccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        return account.Adapt<AccountResponse>();
    }

    public async Task<AccountResponse?> GetAccountByNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

        return account?.Adapt<AccountResponse>();
    }

    public async Task<IEnumerable<AccountSummaryResponse>> GetUserAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return accounts.Adapt<IEnumerable<AccountSummaryResponse>>();
    }

    public async Task<AccountResponse> UpdateAccountStatusAsync(Guid accountId, UpdateAccountStatusRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        var oldStatus = account.Status;
        account.Status = request.Status;
        account.UpdatedAt = DateTime.UtcNow;

        // Log status change
        _logger.LogInformation("Account {AccountId} status changed from {OldStatus} to {NewStatus}. Reason: {Reason}", 
            accountId, oldStatus, request.Status, request.Reason);

        await _context.SaveChangesAsync(cancellationToken);

        return account.Adapt<AccountResponse>();
    }

    public async Task<bool> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            return false;
        }

        // Soft delete by setting status to Closed
        account.Status = AccountStatus.Closed;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account {AccountId} marked as closed", accountId);
        return true;
    }

    public async Task<AccountBalanceResponse> GetAccountBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account {accountId} not found");
        }

        // Get pending transactions
        var pendingAmount = await _context.AccountTransactions
            .Where(t => t.AccountId == accountId && t.Status == TransactionStatus.Pending)
            .SumAsync(t => t.Amount, cancellationToken);

        return new AccountBalanceResponse
        {
            AccountId = account.Id,
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            Currency = account.Currency,
            LastUpdated = account.UpdatedAt,
            AvailableBalance = account.Balance - pendingAmount,
            PendingBalance = pendingAmount
        };
    }

    public async Task<bool> UpdateBalanceAsync(Guid accountId, decimal amount, TransactionType transactionType, string description, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

            if (account == null)
            {
                return false;
            }

            // Validate transaction
            if (!await ValidateTransactionAsync(accountId, amount, transactionType, cancellationToken))
            {
                return false;
            }

            // Update balance
            var oldBalance = account.Balance;
            account.Balance += amount;
            account.LastTransactionAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var accountTransaction = new AccountTransaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                TransactionType = transactionType,
                Amount = amount,
                Description = description,
                Status = TransactionStatus.Completed,
                CorrelationId = correlationId,
                BalanceAfter = account.Balance,
                Timestamp = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            _context.AccountTransactions.Add(accountTransaction);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Balance updated for account {AccountId}: {OldBalance} -> {NewBalance} (Amount: {Amount}, Type: {TransactionType})", 
                accountId, oldBalance, account.Balance, amount, transactionType);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to update balance for account {AccountId}", accountId);
            throw;
        }
    }

    public async Task<bool> ValidateTransactionAsync(Guid accountId, decimal amount, TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null || account.Status != AccountStatus.Active)
        {
            return false;
        }

        // Check if withdrawal would result in negative balance
        if (transactionType == TransactionType.Withdrawal && account.Balance + amount < 0)
        {
            return false;
        }

        // Check daily limits
        var today = DateTime.UtcNow.Date;
        var dailyTotal = await _context.AccountTransactions
            .Where(t => t.AccountId == accountId && 
                       t.Timestamp >= today && 
                       t.TransactionType == transactionType &&
                       t.Status == TransactionStatus.Completed)
            .SumAsync(t => Math.Abs(t.Amount), cancellationToken);

        if (dailyTotal + Math.Abs(amount) > account.DailyLimit)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> AccountExistsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Id == accountId, cancellationToken);
    }

    public async Task<bool> UserOwnsAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsAccountActiveAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        return account?.Status == AccountStatus.Active;
    }

    public async Task<int> GetUserAccountCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .CountAsync(a => a.UserId == userId && a.Status != AccountStatus.Closed, cancellationToken);
    }

    public async Task<decimal> GetTotalUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId && a.Status == AccountStatus.Active)
            .SumAsync(a => a.Balance, cancellationToken);
    }

    public async Task<IEnumerable<AccountSummaryResponse>> GetAccountsByTypeAsync(Guid userId, AccountType accountType, CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.AccountType == accountType)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return accounts.Adapt<IEnumerable<AccountSummaryResponse>>();
    }

    // Private helper methods
    private async Task<string> GenerateAccountNumberAsync(CancellationToken cancellationToken)
    {
        // Use PostgreSQL sequence for account numbers
        var sequenceValue = await _context.Database
            .SqlQueryRaw<long>("SELECT nextval('\"AccountNumberSequence\"')")
            .FirstAsync(cancellationToken);

        return $"ACC{sequenceValue:D10}";
    }

    private static decimal GetDefaultDailyLimit(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Individual => 10000m,
            AccountType.Business => 50000m,
            AccountType.Institutional => 100000m,
            AccountType.Trading => 25000m,
            AccountType.Savings => 5000m,
            AccountType.Escrow => 1000000m,
            _ => 10000m
        };
    }

    private static decimal GetDefaultMonthlyLimit(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Individual => 100000m,
            AccountType.Business => 500000m,
            AccountType.Institutional => 1000000m,
            AccountType.Trading => 250000m,
            AccountType.Savings => 50000m,
            AccountType.Escrow => 10000000m,
            _ => 100000m
        };
    }

    private async Task CreateDefaultAccountLimitsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var defaultLimits = new[]
        {
            new AccountLimit { Id = Guid.NewGuid(), AccountId = accountId, LimitType = LimitType.DailyWithdrawal, LimitAmount = 10000m, Period = LimitPeriod.Daily },
            new AccountLimit { Id = Guid.NewGuid(), AccountId = accountId, LimitType = LimitType.DailyDeposit, LimitAmount = 50000m, Period = LimitPeriod.Daily },
            new AccountLimit { Id = Guid.NewGuid(), AccountId = accountId, LimitType = LimitType.MonthlyWithdrawal, LimitAmount = 100000m, Period = LimitPeriod.Monthly },
            new AccountLimit { Id = Guid.NewGuid(), AccountId = accountId, LimitType = LimitType.SingleTransaction, LimitAmount = 25000m, Period = LimitPeriod.PerTransaction }
        };

        _context.AccountLimits.AddRange(defaultLimits);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task TryAutoVerifyAccountAsync(Guid accountId, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var userVerificationResponse = await _userServiceClient.GetUserVerificationStatusAsync(userId, cancellationToken);
            if (userVerificationResponse.IsSuccessStatusCode && 
                userVerificationResponse.Content?.IsKycVerified == true)
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
                if (account != null)
                {
                    account.Status = AccountStatus.Active;
                    account.VerifiedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Account {AccountId} auto-verified based on user KYC status", accountId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to auto-verify account {AccountId}", accountId);
        }
    }
}
