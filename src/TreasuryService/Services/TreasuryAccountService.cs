using Microsoft.EntityFrameworkCore;
using Mapster;
using TreasuryService.Data;
using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;
using TreasuryService.Services.Interfaces;

namespace TreasuryService.Services;

public class TreasuryAccountService : ITreasuryAccountService
{
    private readonly TreasuryDbContext _context;
    private readonly IUserServiceClient _userServiceClient;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly ILogger<TreasuryAccountService> _logger;

    public TreasuryAccountService(
        TreasuryDbContext context,
        IUserServiceClient userServiceClient,
        INotificationServiceClient notificationServiceClient,
        ILogger<TreasuryAccountService> logger)
    {
        _context = context;
        _userServiceClient = userServiceClient;
        _notificationServiceClient = notificationServiceClient;
        _logger = logger;
    }

    #region Treasury Account CRUD Operations

    public async Task<TreasuryAccountResponse> CreateAccountAsync(CreateTreasuryAccountRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating treasury account for user {UserId}", request.CreatedBy);

        try
        {
            // Validate user exists
            var userExists = await _userServiceClient.UserExistsAsync(request.CreatedBy ?? Guid.Empty);
            if (!userExists)
            {
                throw new ArgumentException($"User {request.CreatedBy} does not exist");
            }

            // Generate unique account number
            var accountNumber = await GenerateAccountNumberAsync(request.AccountType, request.Currency);

            // Create treasury account entity
            var account = new TreasuryAccount
            {
                AccountNumber = accountNumber,
                AccountName = request.AccountName,
                AccountType = request.AccountType,
                Currency = request.Currency,
                Balance = 0,
                AvailableBalance = 0,
                ReservedBalance = 0,
                MinimumBalance = request.MinimumBalance,
                MaximumBalance = request.MaximumBalance ?? decimal.MaxValue,
                Status = "Active",
                Description = request.Description,
                ExternalAccountId = request.ExternalAccountId,
                BankName = request.BankName,
                RoutingNumber = request.RoutingNumber,
                SwiftCode = request.SwiftCode,
                IsDefault = request.IsDefault,
                RequiresApproval = request.RequiresApproval,
                InterestRate = request.InterestRate,
                CreatedBy = request.CreatedBy ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TreasuryAccounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);

            // Send notification
            try
            {
                await _notificationServiceClient.SendNotificationAsync(new NotificationRequest
                {
                    UserId = request.CreatedBy ?? Guid.Empty,
                    Type = "TreasuryAccountCreated",
                    Title = "Treasury Account Created",
                    Message = $"Treasury account '{request.AccountName}' has been created successfully.",
                    Data = new Dictionary<string, object>
                    {
                        ["accountId"] = account.Id,
                        ["accountNumber"] = account.AccountNumber,
                        ["accountType"] = account.AccountType,
                        ["currency"] = account.Currency
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification for treasury account creation {AccountId}", account.Id);
            }

            _logger.LogInformation("Treasury account {AccountId} created successfully", account.Id);

            return account.Adapt<TreasuryAccountResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating treasury account for user {UserId}", request.CreatedBy);
            throw;
        }
    }

    public async Task<TreasuryAccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<TreasuryAccountResponse> GetAccountByNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account with number {accountNumber} not found");
        }

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<IEnumerable<TreasuryAccountResponse>> GetAccountsAsync(GetTreasuryAccountsRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.TreasuryAccounts.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.AccountType))
        {
            query = query.Where(a => a.AccountType == request.AccountType);
        }

        if (!string.IsNullOrEmpty(request.Currency))
        {
            query = query.Where(a => a.Currency == request.Currency);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        if (request.CreatedBy.HasValue)
        {
            query = query.Where(a => a.CreatedBy == request.CreatedBy.Value);
        }

        if (request.IsDefault.HasValue)
        {
            query = query.Where(a => a.IsDefault == request.IsDefault.Value);
        }

        // Apply pagination
        var accounts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return accounts.Adapt<IEnumerable<TreasuryAccountResponse>>();
    }

    public async Task<TreasuryAccountResponse> UpdateAccountAsync(Guid accountId, UpdateTreasuryAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.AccountName))
            account.AccountName = request.AccountName;

        if (!string.IsNullOrEmpty(request.Description))
            account.Description = request.Description;

        if (request.MinimumBalance.HasValue)
            account.MinimumBalance = request.MinimumBalance.Value;

        if (request.MaximumBalance.HasValue)
            account.MaximumBalance = request.MaximumBalance.Value;

        if (request.InterestRate.HasValue)
            account.InterestRate = request.InterestRate.Value;

        if (request.RequiresApproval.HasValue)
            account.RequiresApproval = request.RequiresApproval.Value;

        if (!string.IsNullOrEmpty(request.ExternalAccountId))
            account.ExternalAccountId = request.ExternalAccountId;

        if (!string.IsNullOrEmpty(request.BankName))
            account.BankName = request.BankName;

        if (!string.IsNullOrEmpty(request.RoutingNumber))
            account.RoutingNumber = request.RoutingNumber;

        if (!string.IsNullOrEmpty(request.SwiftCode))
            account.SwiftCode = request.SwiftCode;

        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = request.UpdatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Treasury account {AccountId} updated successfully", accountId);

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<bool> DeactivateAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await UpdateAccountStatusAsync(accountId, "Inactive", cancellationToken);
    }

    public async Task<bool> ActivateAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await UpdateAccountStatusAsync(accountId, "Active", cancellationToken);
    }

    #endregion

    #region Account Status Management

    public async Task<bool> FreezeAccountAsync(Guid accountId, string reason, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        account.Status = "Frozen";
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Send notification
        try
        {
            await _notificationServiceClient.SendNotificationAsync(new NotificationRequest
            {
                UserId = account.CreatedBy,
                Type = "TreasuryAccountFrozen",
                Title = "Treasury Account Frozen",
                Message = $"Treasury account '{account.AccountName}' has been frozen. Reason: {reason}",
                Priority = "High",
                Data = new Dictionary<string, object>
                {
                    ["accountId"] = account.Id,
                    ["reason"] = reason
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send freeze notification for account {AccountId}", accountId);
        }

        _logger.LogInformation("Treasury account {AccountId} frozen. Reason: {Reason}", accountId, reason);

        return true;
    }

    public async Task<bool> UnfreezeAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await UpdateAccountStatusAsync(accountId, "Active", cancellationToken);
    }

    public async Task<bool> CloseAccountAsync(Guid accountId, string reason, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        // Check if account has balance
        if (account.Balance > 0)
        {
            throw new InvalidOperationException($"Cannot close account {accountId} with non-zero balance: {account.Balance}");
        }

        account.Status = "Closed";
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Treasury account {AccountId} closed. Reason: {Reason}", accountId, reason);

        return true;
    }

    #endregion

    #region Account Configuration

    public async Task<TreasuryAccountResponse> SetMinimumBalanceAsync(Guid accountId, decimal minimumBalance, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        account.MinimumBalance = minimumBalance;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<TreasuryAccountResponse> SetMaximumBalanceAsync(Guid accountId, decimal maximumBalance, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        account.MaximumBalance = maximumBalance;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<TreasuryAccountResponse> SetInterestRateAsync(Guid accountId, decimal interestRate, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        account.InterestRate = interestRate;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<TreasuryAccountResponse> SetDefaultAccountAsync(Guid accountId, string currency, CancellationToken cancellationToken = default)
    {
        // First, unset any existing default account for this currency
        var existingDefault = await _context.TreasuryAccounts
            .Where(a => a.Currency == currency && a.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var account in existingDefault)
        {
            account.IsDefault = false;
            account.UpdatedAt = DateTime.UtcNow;
        }

        // Set the new default account
        var newDefaultAccount = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (newDefaultAccount == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        if (newDefaultAccount.Currency != currency)
        {
            throw new ArgumentException($"Account currency {newDefaultAccount.Currency} does not match requested currency {currency}");
        }

        newDefaultAccount.IsDefault = true;
        newDefaultAccount.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return newDefaultAccount.Adapt<TreasuryAccountResponse>();
    }

    #endregion

    #region Account Validation

    public async Task<bool> ValidateAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        return account != null && account.Status == "Active";
    }

    public async Task<bool> UserOwnsAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.CreatedBy == userId, cancellationToken);

        return account != null;
    }

    public async Task<IEnumerable<TreasuryAccountResponse>> GetUserAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _context.TreasuryAccounts
            .Where(a => a.CreatedBy == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return accounts.Adapt<IEnumerable<TreasuryAccountResponse>>();
    }

    #endregion

    #region Account Statistics

    public async Task<AccountStatisticsResponse> GetAccountStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        var transactionQuery = _context.TreasuryTransactions
            .Where(t => t.AccountId == accountId);

        if (startDate.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedAt <= endDate.Value);
        }

        var transactions = await transactionQuery.ToListAsync(cancellationToken);

        var statistics = new AccountStatisticsResponse
        {
            AccountId = accountId,
            TotalTransactions = transactions.Count,
            TotalDeposits = transactions.Where(t => t.TransactionType == "Deposit").Count(),
            TotalWithdrawals = transactions.Where(t => t.TransactionType == "Withdrawal").Count(),
            TotalTransfers = transactions.Where(t => t.TransactionType == "Transfer").Count(),
            CurrentBalance = account.Balance,
            AvailableBalance = account.AvailableBalance,
            ReservedBalance = account.ReservedBalance,
            AverageTransactionAmount = transactions.Any() ? transactions.Average(t => t.Amount) : 0,
            LargestTransaction = transactions.Any() ? transactions.Max(t => t.Amount) : 0,
            SmallestTransaction = transactions.Any() ? transactions.Min(t => t.Amount) : 0,
            LastTransactionDate = transactions.Any() ? transactions.Max(t => t.CreatedAt) : DateTime.MinValue,
            PeriodStart = startDate ?? DateTime.MinValue,
            PeriodEnd = endDate ?? DateTime.MinValue
        };

        return statistics;
    }

    public async Task<IEnumerable<TreasuryAccountSummaryResponse>> GetAccountSummariesAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _context.TreasuryAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync(cancellationToken);

        var summaries = new List<TreasuryAccountSummaryResponse>();

        foreach (var account in accounts)
        {
            var transactionCount = await _context.TreasuryTransactions
                .CountAsync(t => t.AccountId == account.Id, cancellationToken);

            var lastTransaction = await _context.TreasuryTransactions
                .Where(t => t.AccountId == account.Id)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            summaries.Add(new TreasuryAccountSummaryResponse
            {
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                Currency = account.Currency,
                Balance = account.Balance,
                AvailableBalance = account.AvailableBalance,
                ReservedBalance = account.ReservedBalance,
                Status = account.Status,
                TransactionCount = transactionCount,
                LastTransactionDate = lastTransaction?.CreatedAt ?? DateTime.MinValue,
                CreatedAt = account.CreatedAt
            });
        }

        return summaries.OrderByDescending(s => s.Balance);
    }

    #endregion

    #region Helper Methods

    private async Task<bool> UpdateAccountStatusAsync(Guid accountId, string status, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        account.Status = status;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Treasury account {AccountId} status updated to {Status}", accountId, status);

        return true;
    }

    private async Task<string> GenerateAccountNumberAsync(string accountType, string currency)
    {
        // Generate unique account number based on type and currency
        var prefix = accountType.Substring(0, Math.Min(3, accountType.Length)).ToUpper();
        var currencyCode = currency.ToUpper();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);

        var accountNumber = $"{prefix}-{currencyCode}-{timestamp}-{random}";

        // Ensure uniqueness
        var exists = await _context.TreasuryAccounts
            .AnyAsync(a => a.AccountNumber == accountNumber);

        if (exists)
        {
            // If collision, add additional random suffix
            accountNumber += $"-{new Random().Next(100, 999)}";
        }

        return accountNumber;
    }

    #endregion
}
