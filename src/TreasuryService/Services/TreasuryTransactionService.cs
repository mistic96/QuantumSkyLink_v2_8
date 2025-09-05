using Microsoft.EntityFrameworkCore;
using Mapster;
using TreasuryService.Data;
using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;
using TreasuryService.Services.Interfaces;

namespace TreasuryService.Services;

public class TreasuryTransactionService : ITreasuryTransactionService
{
    private readonly TreasuryDbContext _context;
    private readonly ITreasuryBalanceService _balanceService;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly ILogger<TreasuryTransactionService> _logger;

    public TreasuryTransactionService(
        TreasuryDbContext context,
        ITreasuryBalanceService balanceService,
        INotificationServiceClient notificationServiceClient,
        ILogger<TreasuryTransactionService> logger)
    {
        _context = context;
        _balanceService = balanceService;
        _notificationServiceClient = notificationServiceClient;
        _logger = logger;
    }

    #region Transaction CRUD Operations

    public async Task<TreasuryTransactionResponse> CreateTransactionAsync(CreateTreasuryTransactionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating treasury transaction for account {AccountId}", request.AccountId);

        // Validate account exists
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
        {
            throw new ArgumentException($"Treasury account {request.AccountId} not found");
        }

        // Validate transaction
        var validation = await _balanceService.ValidateTransactionBalanceAsync(
            request.AccountId, request.Amount, request.TransactionType, cancellationToken);

        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Transaction validation failed: {validation.Message}");
        }

        // Create transaction
        var transaction = new TreasuryTransaction
        {
            AccountId = request.AccountId,
            Amount = request.Amount,
            TransactionType = request.TransactionType,
            Description = request.Description,
            Reference = request.Reference,
            Status = "Pending",
            CreatedBy = request.CreatedBy ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TreasuryTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Treasury transaction {TransactionId} created successfully", transaction.Id);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new ArgumentException($"Treasury transaction {transactionId} not found");
        }

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsAsync(GetTreasuryTransactionsRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.TreasuryTransactions.AsQueryable();

        // Apply filters
        if (request.AccountId.HasValue)
        {
            query = query.Where(t => t.AccountId == request.AccountId.Value);
        }

        if (!string.IsNullOrEmpty(request.TransactionType))
        {
            query = query.Where(t => t.TransactionType == request.TransactionType);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(t => t.Status == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.EndDate.Value);
        }

        // Apply pagination
        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<TreasuryTransactionResponse>>();
    }

    public async Task<IEnumerable<TreasuryTransactionResponse>> GetAccountTransactionsAsync(Guid accountId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.TreasuryTransactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<TreasuryTransactionResponse>>();
    }

    public async Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.TreasuryTransactions
            .Where(t => t.AccountId == accountId && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<TreasuryTransactionResponse>>();
    }

    public async Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsByTypeAsync(Guid accountId, string transactionType, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.TreasuryTransactions
            .Where(t => t.AccountId == accountId && t.TransactionType == transactionType)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<TreasuryTransactionResponse>>();
    }

    #endregion

    #region Transaction Processing

    public async Task<TreasuryTransactionResponse> ProcessTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new ArgumentException($"Treasury transaction {transactionId} not found");
        }

        if (transaction.Status != "Pending")
        {
            throw new InvalidOperationException($"Transaction {transactionId} is not in pending status");
        }

        try
        {
            // Update balance
            await _balanceService.UpdateBalanceAsync(
                transaction.AccountId, 
                transaction.Amount, 
                transaction.TransactionType, 
                transaction.Id, 
                cancellationToken);

            // Update transaction status
            transaction.Status = "Completed";
            transaction.ProcessedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Treasury transaction {TransactionId} processed successfully", transactionId);

            return transaction.Adapt<TreasuryTransactionResponse>();
        }
        catch (Exception ex)
        {
            transaction.Status = "Failed";
            transaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex, "Failed to process treasury transaction {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<TreasuryTransactionResponse> ApproveTransactionAsync(Guid transactionId, ApproveTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new ArgumentException($"Treasury transaction {transactionId} not found");
        }

        transaction.Status = "Approved";
        transaction.ApprovedBy = request.ApprovedBy;
        transaction.ApprovedAt = DateTime.UtcNow;
        transaction.ApprovalNotes = request.Notes;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Auto-process if configured
        if (request.AutoProcess == true)
        {
            return await ProcessTransactionAsync(transactionId, cancellationToken);
        }

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> RejectTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new ArgumentException($"Treasury transaction {transactionId} not found");
        }

        transaction.Status = "Rejected";
        transaction.RejectionReason = reason;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> CancelTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
        {
            throw new ArgumentException($"Treasury transaction {transactionId} not found");
        }

        if (transaction.Status == "Completed")
        {
            throw new InvalidOperationException("Cannot cancel a completed transaction");
        }

        transaction.Status = "Cancelled";
        transaction.CancellationReason = reason;
        transaction.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    #endregion

    #region Fund Transfer Operations

    public async Task<TreasuryTransactionResponse> TransferFundsAsync(TransferFundsRequest request, CancellationToken cancellationToken = default)
    {
        // Create withdrawal transaction
        var withdrawalTransaction = await CreateTransactionAsync(new CreateTreasuryTransactionRequest
        {
            AccountId = request.FromAccountId,
            Amount = request.Amount,
            TransactionType = "Transfer_Out",
            Description = $"Transfer to account {request.ToAccountId}: {request.Description}",
            Reference = request.Reference,
            CreatedBy = request.InitiatedBy ?? Guid.Empty
        }, cancellationToken);

        // Create deposit transaction
        var depositTransaction = await CreateTransactionAsync(new CreateTreasuryTransactionRequest
        {
            AccountId = request.ToAccountId,
            Amount = request.Amount,
            TransactionType = "Transfer_In",
            Description = $"Transfer from account {request.FromAccountId}: {request.Description}",
            Reference = request.Reference,
            CreatedBy = request.InitiatedBy
        }, cancellationToken);

        // Process both transactions
        await ProcessTransactionAsync(withdrawalTransaction.Id, cancellationToken);
        await ProcessTransactionAsync(depositTransaction.Id, cancellationToken);

        return withdrawalTransaction;
    }

    public async Task<TreasuryTransactionResponse> DepositFundsAsync(DepositFundsRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await CreateTransactionAsync(new CreateTreasuryTransactionRequest
        {
            AccountId = request.AccountId,
            Amount = request.Amount,
            TransactionType = "Deposit",
            Description = request.Description,
            Reference = request.Reference,
            CreatedBy = request.InitiatedBy
        }, cancellationToken);

        if (request.AutoProcess == true)
        {
            return await ProcessTransactionAsync(transaction.Id, cancellationToken);
        }

        return transaction;
    }

    public async Task<TreasuryTransactionResponse> WithdrawFundsAsync(WithdrawFundsRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await CreateTransactionAsync(new CreateTreasuryTransactionRequest
        {
            AccountId = request.AccountId,
            Amount = request.Amount,
            TransactionType = "Withdrawal",
            Description = request.Description,
            Reference = request.Reference,
            CreatedBy = request.InitiatedBy
        }, cancellationToken);

        if (request.AutoProcess == true)
        {
            return await ProcessTransactionAsync(transaction.Id, cancellationToken);
        }

        return transaction;
    }

    public async Task<TreasuryTransactionResponse> InternalTransferAsync(InternalTransferRequest request, CancellationToken cancellationToken = default)
    {
        return await TransferFundsAsync(new TransferFundsRequest
        {
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            Description = $"Internal transfer: {request.Description}",
            Reference = request.Reference,
            InitiatedBy = request.InitiatedBy
        }, cancellationToken);
    }

    public async Task<TreasuryTransactionResponse> ExternalTransferAsync(ExternalTransferRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await CreateTransactionAsync(new CreateTreasuryTransactionRequest
        {
            AccountId = request.FromAccountId,
            Amount = request.Amount,
            TransactionType = "External_Transfer",
            Description = $"External transfer to {request.ExternalAccountId}: {request.Description}",
            Reference = request.Reference,
            CreatedBy = request.InitiatedBy
        }, cancellationToken);

        // External transfers typically require approval
        return transaction;
    }

    #endregion

    #region Not Yet Implemented - Placeholder methods

    public Task<bool> ValidateTransactionAsync(CreateTreasuryTransactionRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<bool> UserOwnsTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<bool> CanProcessTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<TransactionValidationResponse> ValidateTransferAsync(TransferFundsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<TreasuryTransactionResponse> UpdateTransactionStatusAsync(Guid transactionId, string status, string? notes = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<TreasuryTransactionResponse>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<TreasuryTransactionResponse>> GetFailedTransactionsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<bool> RetryFailedTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<TransactionStatisticsResponse> GetTransactionStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<TransactionSummaryResponse>> GetTransactionSummaryAsync(GetTransactionSummaryRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<decimal> GetTransactionVolumeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<ReconciliationResponse> ReconcileTransactionsAsync(ReconcileTransactionsRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<IEnumerable<TreasuryTransactionResponse>> GetUnreconciledTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    public Task<bool> MarkTransactionReconciledAsync(Guid transactionId, string reconciliationReference, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in future iteration");
    }

    #endregion
}
