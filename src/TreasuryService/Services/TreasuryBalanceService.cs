using Microsoft.EntityFrameworkCore;
using Mapster;
using TreasuryService.Data;
using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;
using TreasuryService.Services.Interfaces;

namespace TreasuryService.Services;

public class TreasuryBalanceService : ITreasuryBalanceService
{
    private readonly TreasuryDbContext _context;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly ILogger<TreasuryBalanceService> _logger;

    public TreasuryBalanceService(
        TreasuryDbContext context,
        INotificationServiceClient notificationServiceClient,
        ILogger<TreasuryBalanceService> logger)
    {
        _context = context;
        _notificationServiceClient = notificationServiceClient;
        _logger = logger;
    }

    #region Balance Management

    public async Task<TreasuryBalanceResponse> GetCurrentBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        return new TreasuryBalanceResponse
        {
            AccountId = accountId,
            Balance = account.Balance,
            AvailableBalance = account.AvailableBalance,
            ReservedBalance = account.ReservedBalance,
            Currency = account.Currency,
            LastUpdated = account.UpdatedAt,
            MinimumBalance = account.MinimumBalance,
            MaximumBalance = account.MaximumBalance
        };
    }

    public async Task<IEnumerable<TreasuryBalanceResponse>> GetBalanceHistoryAsync(GetBalanceHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.TreasuryBalances
            .Where(b => b.AccountId == request.AccountId);

        if (request.StartDate.HasValue)
        {
            query = query.Where(b => b.BalanceDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(b => b.BalanceDate <= request.EndDate.Value);
        }

        var balanceHistory = await query
            .OrderByDescending(b => b.BalanceDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return balanceHistory.Select(b => new TreasuryBalanceResponse
        {
            AccountId = b.AccountId,
            Balance = b.Balance,
            AvailableBalance = b.AvailableBalance,
            ReservedBalance = b.ReservedBalance,
            Currency = b.Currency,
            LastUpdated = b.BalanceDate,
            ChangeAmount = b.ChangeAmount ?? 0,
            ChangeReason = b.ChangeReason,
            TransactionId = b.TransactionId
        });
    }

    public async Task<TreasuryBalanceResponse> ReconcileBalanceAsync(ReconcileBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {request.AccountId} not found");
        }

        var previousBalance = account.Balance;
        var difference = request.ActualBalance - previousBalance;

        // Update account balance
        account.Balance = request.ActualBalance;
        account.AvailableBalance = request.ActualBalance - account.ReservedBalance;
        account.UpdatedAt = DateTime.UtcNow;

        // Create balance history record
        var balanceHistory = new TreasuryBalance
        {
            AccountId = request.AccountId,
            Balance = request.ActualBalance,
            AvailableBalance = account.AvailableBalance,
            ReservedBalance = account.ReservedBalance,
            Currency = account.Currency,
            ChangeAmount = difference,
            ChangeReason = $"Reconciliation: {request.Reason}",
            BalanceDate = DateTime.UtcNow,
            ReconciliationReference = request.ReconciliationReference
        };

        _context.TreasuryBalances.Add(balanceHistory);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Balance reconciled for account {AccountId}. Previous: {PreviousBalance}, New: {NewBalance}, Difference: {Difference}",
            request.AccountId, previousBalance, request.ActualBalance, difference);

        return account.Adapt<TreasuryBalanceResponse>();
    }

    public async Task<TreasuryBalanceResponse> UpdateBalanceAsync(Guid accountId, decimal amount, string transactionType, Guid? transactionId = null, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {accountId} not found");
        }

        var previousBalance = account.Balance;

        // Update balance based on transaction type
        switch (transactionType.ToLower())
        {
            case "deposit":
            case "credit":
                account.Balance += amount;
                break;
            case "withdrawal":
            case "debit":
                account.Balance -= amount;
                break;
            case "transfer_in":
                account.Balance += amount;
                break;
            case "transfer_out":
                account.Balance -= amount;
                break;
            default:
                throw new ArgumentException($"Unknown transaction type: {transactionType}");
        }

        // Update available balance
        account.AvailableBalance = account.Balance - account.ReservedBalance;
        account.UpdatedAt = DateTime.UtcNow;

        // Create balance history record
        var balanceHistory = new TreasuryBalance
        {
            AccountId = accountId,
            Balance = account.Balance,
            AvailableBalance = account.AvailableBalance,
            ReservedBalance = account.ReservedBalance,
            Currency = account.Currency,
            ChangeAmount = account.Balance - previousBalance,
            ChangeReason = $"Transaction: {transactionType}",
            BalanceDate = DateTime.UtcNow,
            TransactionId = transactionId
        };

        _context.TreasuryBalances.Add(balanceHistory);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Balance updated for account {AccountId}. Amount: {Amount}, Type: {TransactionType}, New Balance: {NewBalance}",
            accountId, amount, transactionType, account.Balance);

        return account.Adapt<TreasuryBalanceResponse>();
    }

    #endregion

    #region Balance Validation

    public async Task<bool> ValidateBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            return false;
        }

        // Check if account is active
        if (account.Status != "Active")
        {
            return false;
        }

        // Check minimum balance constraint
        if (account.Balance - amount < account.MinimumBalance)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> HasSufficientBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        return account != null && account.AvailableBalance >= amount;
    }

    public async Task<BalanceValidationResponse> ValidateTransactionBalanceAsync(Guid accountId, decimal amount, string transactionType, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            return new BalanceValidationResponse
            {
                IsValid = false,
                Message = "Account not found",
                CurrentBalance = 0,
                AvailableBalance = 0
            };
        }

        var response = new BalanceValidationResponse
        {
            CurrentBalance = account.Balance,
            AvailableBalance = account.AvailableBalance,
            ReservedBalance = account.ReservedBalance
        };

        // Validate based on transaction type
        switch (transactionType.ToLower())
        {
            case "withdrawal":
            case "debit":
            case "transfer_out":
                if (account.AvailableBalance < amount)
                {
                    response.IsValid = false;
                    response.Message = $"Insufficient available balance. Required: {amount:C}, Available: {account.AvailableBalance:C}";
                }
                else if (account.Balance - amount < account.MinimumBalance)
                {
                    response.IsValid = false;
                    response.Message = $"Transaction would violate minimum balance requirement of {account.MinimumBalance:C}";
                }
                else
                {
                    response.IsValid = true;
                    response.Message = "Transaction is valid";
                }
                break;

            case "deposit":
            case "credit":
            case "transfer_in":
                if (account.Balance + amount > account.MaximumBalance)
                {
                    response.IsValid = false;
                    response.Message = $"Transaction would exceed maximum balance limit of {account.MaximumBalance:C}";
                }
                else
                {
                    response.IsValid = true;
                    response.Message = "Transaction is valid";
                }
                break;

            default:
                response.IsValid = false;
                response.Message = $"Unknown transaction type: {transactionType}";
                break;
        }

        return response;
    }

    #endregion

    #region Not Yet Implemented - Will be moved to specialized services

    public Task<bool> ReserveBalanceAsync(Guid accountId, decimal amount, Guid transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceReservationService");
    }

    public Task<bool> ReleaseReservedBalanceAsync(Guid accountId, decimal amount, Guid transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceReservationService");
    }

    public Task<bool> CommitReservedBalanceAsync(Guid accountId, decimal amount, Guid transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceReservationService");
    }

    public Task<decimal> GetReservedBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceReservationService");
    }

    public Task<decimal> GetAvailableBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceReservationService");
    }

    public Task<IEnumerable<BalanceAlertResponse>> GetBalanceAlertsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceMonitoringService");
    }

    public Task<bool> CheckMinimumBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceMonitoringService");
    }

    public Task<bool> CheckMaximumBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceMonitoringService");
    }

    public Task<IEnumerable<TreasuryAccountResponse>> GetAccountsNearLimitsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceMonitoringService");
    }

    public Task<BalanceSnapshotResponse> CreateBalanceSnapshotAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<IEnumerable<BalanceSnapshotResponse>> GetBalanceSnapshotsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<BalanceComparisonResponse> CompareBalanceSnapshotsAsync(Guid accountId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<IEnumerable<CurrencyBalanceResponse>> GetCurrencyBalancesAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<CurrencyBalanceResponse> GetCurrencyBalanceAsync(Guid accountId, string currency, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<decimal> ConvertBalanceAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<BalanceAnalyticsResponse> GetBalanceAnalyticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<IEnumerable<BalanceTrendResponse>> GetBalanceTrendsAsync(Guid accountId, string period = "daily", int periods = 30, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<BalanceProjectionResponse> ProjectBalanceAsync(Guid accountId, int days = 30, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<bool> SyncBalanceWithExternalSourceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<BalanceSyncResponse> GetBalanceSyncStatusAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    public Task<bool> ForceBalanceSyncAsync(Guid accountId, decimal externalBalance, string source, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in TreasuryBalanceAnalyticsService");
    }

    #endregion
}
