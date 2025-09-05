using AccountService.Data;
using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace AccountService.Services;

public class AccountTransactionService : IAccountTransactionService
{
    private readonly AccountDbContext _context;
    private readonly ILogger<AccountTransactionService> _logger;
    private readonly IAccountService _accountService;

    public AccountTransactionService(
        AccountDbContext context,
        ILogger<AccountTransactionService> logger,
        IAccountService accountService)
    {
        _context = context;
        _logger = logger;
        _accountService = accountService;
    }

    public async Task<AccountTransactionResponse> CreateTransactionAsync(CreateAccountTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = new AccountTransaction
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            TransactionType = Enum.Parse<TransactionType>(request.TransactionType),
            Amount = request.Amount,
            Description = request.Description,
            Reference = request.Reference,
            ExternalTransactionId = request.ExternalTransactionId,
            Status = TransactionStatus.Pending,
            CorrelationId = request.CorrelationId,
            Fee = request.Fee,
            BalanceAfter = 0, // Will be calculated
            ProcessedBy = request.ProcessedBy?.ToString(),
            Metadata = request.Metadata,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccountTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction created: {TransactionId} for account {AccountId}", transaction.Id, request.AccountId);

        return transaction.Adapt<AccountTransactionResponse>();
    }

    public async Task<AccountTransactionResponse> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.AccountTransactions
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new InvalidOperationException($"Transaction with ID {transactionId} not found");
        }

        return transaction.Adapt<AccountTransactionResponse>();
    }

    public async Task<IEnumerable<AccountTransactionResponse>> GetAccountTransactionsAsync(Guid accountId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.AccountTransactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<AccountTransactionResponse>>();
    }

    public async Task<IEnumerable<AccountTransactionResponse>> GetUserTransactionsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.AccountTransactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<AccountTransactionResponse>>();
    }

    public async Task<IEnumerable<AccountTransactionResponse>> GetTransactionsByTypeAsync(Guid accountId, TransactionType transactionType, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.AccountTransactions
            .Where(t => t.AccountId == accountId && t.TransactionType == transactionType)
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<AccountTransactionResponse>>();
    }

    public async Task<IEnumerable<AccountTransactionResponse>> GetTransactionsByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.AccountTransactions
            .Where(t => t.AccountId == accountId && t.Timestamp >= startDate && t.Timestamp <= endDate)
            .OrderByDescending(t => t.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<AccountTransactionResponse>>();
    }

    public async Task<AccountTransactionResponse> UpdateTransactionStatusAsync(Guid transactionId, TransactionStatus status, string? notes = null, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.AccountTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new InvalidOperationException($"Transaction with ID {transactionId} not found");
        }

        transaction.Status = status;
        if (status == TransactionStatus.Completed || status == TransactionStatus.Failed)
        {
            transaction.ProcessedAt = DateTime.UtcNow;
        }

        if (!string.IsNullOrEmpty(notes))
        {
            transaction.Metadata = notes;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction status updated: {TransactionId} -> {Status}", transactionId, status);

        return transaction.Adapt<AccountTransactionResponse>();
    }

    public async Task<bool> CancelTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.AccountTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            return false;
        }

        if (transaction.Status != TransactionStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot cancel transaction with status {transaction.Status}");
        }

        transaction.Status = TransactionStatus.Cancelled;
        transaction.ProcessedAt = DateTime.UtcNow;
        transaction.Metadata = reason;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Transaction cancelled: {TransactionId}, Reason: {Reason}", transactionId, reason);

        return true;
    }

    public async Task<bool> TransactionExistsAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountTransactions
            .AnyAsync(t => t.Id == transactionId, cancellationToken);
    }

    public async Task<bool> UserOwnsTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountTransactions
            .Include(t => t.Account)
            .AnyAsync(t => t.Id == transactionId && t.Account.UserId == userId, cancellationToken);
    }

    public async Task<decimal> GetAccountTransactionSumAsync(Guid accountId, TransactionType? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AccountTransactions
            .Where(t => t.AccountId == accountId && t.Status == TransactionStatus.Completed);

        if (transactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == transactionType.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Timestamp <= endDate.Value);
        }

        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<int> GetAccountTransactionCountAsync(Guid accountId, TransactionType? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AccountTransactions
            .Where(t => t.AccountId == accountId);

        if (transactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == transactionType.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Timestamp <= endDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<TransactionStatisticsResponse> GetTransactionStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.AccountTransactions
            .Where(t => t.AccountId == accountId);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Timestamp <= endDate.Value);
        }

        var transactions = await query.ToListAsync(cancellationToken);

        return new TransactionStatisticsResponse
        {
            TotalTransactions = transactions.Count,
            TotalDeposits = transactions.Where(t => t.TransactionType == TransactionType.Deposit && t.Status == TransactionStatus.Completed).Sum(t => t.Amount),
            TotalWithdrawals = transactions.Where(t => t.TransactionType == TransactionType.Withdrawal && t.Status == TransactionStatus.Completed).Sum(t => t.Amount),
            TotalFees = transactions.Where(t => t.TransactionType == TransactionType.Fee && t.Status == TransactionStatus.Completed).Sum(t => t.Amount),
            PendingTransactions = transactions.Count(t => t.Status == TransactionStatus.Pending),
            FailedTransactions = transactions.Count(t => t.Status == TransactionStatus.Failed)
        };
    }

    public async Task<AccountTransactionResponse> ProcessDepositAsync(Guid accountId, decimal amount, string description, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var request = new CreateAccountTransactionRequest
        {
            AccountId = accountId,
            TransactionType = TransactionType.Deposit.ToString(),
            Amount = amount,
            Description = description,
            CorrelationId = correlationId
        };

        var transaction = await CreateTransactionAsync(request, cancellationToken);

        // Update account balance
        await _accountService.UpdateBalanceAsync(accountId, amount, TransactionType.Deposit, description, correlationId, cancellationToken);

        // Update transaction status to completed
        return await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.Completed, null, cancellationToken);
    }

    public async Task<AccountTransactionResponse> ProcessWithdrawalAsync(Guid accountId, decimal amount, string description, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var request = new CreateAccountTransactionRequest
        {
            AccountId = accountId,
            TransactionType = TransactionType.Withdrawal.ToString(),
            Amount = amount,
            Description = description,
            CorrelationId = correlationId
        };

        var transaction = await CreateTransactionAsync(request, cancellationToken);

        // Update account balance
        await _accountService.UpdateBalanceAsync(accountId, -amount, TransactionType.Withdrawal, description, correlationId, cancellationToken);

        // Update transaction status to completed
        return await UpdateTransactionStatusAsync(transaction.Id, TransactionStatus.Completed, null, cancellationToken);
    }

    public async Task<AccountTransactionResponse> ProcessTransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string description, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create withdrawal transaction
            var withdrawalRequest = new CreateAccountTransactionRequest
            {
                AccountId = fromAccountId,
                TransactionType = TransactionType.Transfer.ToString(),
                Amount = amount,
                Description = $"Transfer to account: {description}",
                CorrelationId = correlationId
            };

            var withdrawalTransaction = await CreateTransactionAsync(withdrawalRequest, cancellationToken);

            // Create deposit transaction
            var depositRequest = new CreateAccountTransactionRequest
            {
                AccountId = toAccountId,
                TransactionType = TransactionType.Transfer.ToString(),
                Amount = amount,
                Description = $"Transfer from account: {description}",
                CorrelationId = correlationId
            };

            var depositTransaction = await CreateTransactionAsync(depositRequest, cancellationToken);

            // Update balances
            await _accountService.UpdateBalanceAsync(fromAccountId, -amount, TransactionType.Transfer, description, correlationId, cancellationToken);
            await _accountService.UpdateBalanceAsync(toAccountId, amount, TransactionType.Transfer, description, correlationId, cancellationToken);

            // Update transaction statuses
            await UpdateTransactionStatusAsync(withdrawalTransaction.Id, TransactionStatus.Completed, null, cancellationToken);
            await UpdateTransactionStatusAsync(depositTransaction.Id, TransactionStatus.Completed, null, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Transfer completed: {Amount} from {FromAccount} to {ToAccount}", amount, fromAccountId, toAccountId);

            return withdrawalTransaction;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transfer failed: {Amount} from {FromAccount} to {ToAccount}", amount, fromAccountId, toAccountId);
            throw;
        }
    }
}
