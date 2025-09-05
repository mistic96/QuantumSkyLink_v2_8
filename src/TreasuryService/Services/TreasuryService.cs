using Microsoft.EntityFrameworkCore;
using TreasuryService.Data;
using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;
using TreasuryService.Services.Interfaces;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace TreasuryService.Services;

public class TreasuryService : ITreasuryService
{
    private readonly TreasuryDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TreasuryService> _logger;

    public TreasuryService(
        TreasuryDbContext context,
        IDistributedCache cache,
        ILogger<TreasuryService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    // Treasury Account Management
    public async Task<TreasuryAccountResponse> CreateAccountAsync(CreateTreasuryAccountRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating treasury account: {AccountName}", request.AccountName);

        var account = new TreasuryAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = await GenerateAccountNumberAsync(),
            AccountName = request.AccountName,
            AccountType = request.AccountType,
            Currency = request.Currency,
            Balance = request.InitialBalance,
            AvailableBalance = request.InitialBalance,
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
            CreatedBy = Guid.NewGuid() // TODO: Get from JWT claims
        };

        _context.TreasuryAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Treasury account created successfully: {AccountId}", account.Id);
        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<TreasuryAccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {accountId}");

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<TreasuryAccountResponse> GetAccountByNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {accountNumber}");

        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<IEnumerable<TreasuryAccountResponse>> GetAccountsAsync(GetTreasuryAccountsRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.TreasuryAccounts.AsQueryable();

        if (!string.IsNullOrEmpty(request.AccountType))
            query = query.Where(a => a.AccountType == request.AccountType);

        if (!string.IsNullOrEmpty(request.Currency))
            query = query.Where(a => a.Currency == request.Currency);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(a => a.Status == request.Status);

        if (request.IsDefault.HasValue)
            query = query.Where(a => a.IsDefault == request.IsDefault.Value);

        var accounts = await query
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
            throw new KeyNotFoundException($"Treasury account not found: {accountId}");

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.AccountName))
            account.AccountName = request.AccountName;

        if (request.Description != null)
            account.Description = request.Description;

        if (request.MinimumBalance.HasValue)
            account.MinimumBalance = request.MinimumBalance.Value;

        if (request.MaximumBalance.HasValue)
            account.MaximumBalance = request.MaximumBalance.Value;

        if (!string.IsNullOrEmpty(request.Status))
            account.Status = request.Status;

        account.UpdatedBy = Guid.NewGuid(); // TODO: Get from JWT claims

        await _context.SaveChangesAsync(cancellationToken);
        return account.Adapt<TreasuryAccountResponse>();
    }

    public async Task<bool> DeactivateAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
            return false;

        account.Status = "Inactive";
        account.UpdatedBy = Guid.NewGuid(); // TODO: Get from JWT claims

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Placeholder implementations for remaining methods
    public async Task<TreasuryBalanceResponse> GetCurrentBalanceAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting current balance for account: {AccountId}", accountId);

        // Try cache first
        var cacheKey = $"treasury_balance_{accountId}";
        var cachedBalance = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (!string.IsNullOrEmpty(cachedBalance))
        {
            _logger.LogDebug("Balance found in cache for account: {AccountId}", accountId);
            return System.Text.Json.JsonSerializer.Deserialize<TreasuryBalanceResponse>(cachedBalance)!;
        }

        // Get from database
        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {accountId}");

        // Get or create current balance record
        var balance = await _context.TreasuryBalances
            .Where(b => b.AccountId == accountId && b.BalanceType == "Current")
            .OrderByDescending(b => b.BalanceDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (balance == null)
        {
            // Create initial balance record
            balance = new TreasuryBalance
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Currency = account.Currency,
                BalanceType = "Current",
                Balance = account.Balance,
                AvailableBalance = account.AvailableBalance,
                ReservedBalance = account.ReservedBalance,
                PendingCredits = 0,
                PendingDebits = 0,
                DayChange = 0,
                DayChangePercentage = 0,
                BalanceDate = DateTime.UtcNow.Date,
                AsOfTime = DateTime.UtcNow,
                IsReconciled = true,
                ReconciledAt = DateTime.UtcNow
            };

            _context.TreasuryBalances.Add(balance);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var response = balance.Adapt<TreasuryBalanceResponse>();

        // Cache for 5 minutes
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(response), cacheOptions, cancellationToken);

        return response;
    }

    public async Task<IEnumerable<TreasuryBalanceResponse>> GetBalanceHistoryAsync(GetBalanceHistoryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting balance history for account: {AccountId}", request.AccountId);

        var query = _context.TreasuryBalances
            .Where(b => b.AccountId == request.AccountId);

        if (request.FromDate.HasValue)
            query = query.Where(b => b.BalanceDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(b => b.BalanceDate <= request.ToDate.Value);

        if (!string.IsNullOrEmpty(request.BalanceType))
            query = query.Where(b => b.BalanceType == request.BalanceType);

        var balances = await query
            .OrderByDescending(b => b.BalanceDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return balances.Adapt<IEnumerable<TreasuryBalanceResponse>>();
    }

    public async Task<TreasuryBalanceResponse> ReconcileBalanceAsync(ReconcileBalanceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reconciling balance for account: {AccountId}", request.AccountId);

        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {request.AccountId}");

        // Get current balance record
        var currentBalance = await _context.TreasuryBalances
            .Where(b => b.AccountId == request.AccountId && b.BalanceType == "Current")
            .OrderByDescending(b => b.BalanceDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentBalance == null)
        {
            throw new InvalidOperationException($"No current balance record found for account: {request.AccountId}");
        }

        // Calculate difference
        var difference = request.ExternalBalance - currentBalance.Balance;

        // Update account balance
        account.Balance = request.ExternalBalance;
        account.AvailableBalance = request.ExternalBalance - account.ReservedBalance;

        // Create reconciliation balance record
        var reconciledBalance = new TreasuryBalance
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            Currency = account.Currency,
            BalanceType = "Reconciled",
            Balance = request.ExternalBalance,
            AvailableBalance = account.AvailableBalance,
            ReservedBalance = account.ReservedBalance,
            PendingCredits = currentBalance.PendingCredits,
            PendingDebits = currentBalance.PendingDebits,
            DayChange = difference,
            DayChangePercentage = currentBalance.Balance != 0 ? (difference / currentBalance.Balance) * 100 : 0,
            BalanceDate = DateTime.UtcNow.Date,
            AsOfTime = DateTime.UtcNow,
            IsReconciled = true,
            ReconciledAt = DateTime.UtcNow,
            ReconciliationNotes = request.ReconciliationNotes
        };

        _context.TreasuryBalances.Add(reconciledBalance);

        // Update current balance record
        currentBalance.IsReconciled = true;
        currentBalance.ReconciledAt = DateTime.UtcNow;
        currentBalance.ReconciliationNotes = request.ReconciliationNotes;

        await _context.SaveChangesAsync(cancellationToken);

        // Clear cache
        var cacheKey = $"treasury_balance_{request.AccountId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("Balance reconciled successfully for account: {AccountId}, Difference: {Difference}", 
            request.AccountId, difference);

        return reconciledBalance.Adapt<TreasuryBalanceResponse>();
    }

    public async Task<TreasuryTransactionResponse> CreateTransactionAsync(CreateTreasuryTransactionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating treasury transaction for account: {AccountId}, Type: {TransactionType}, Amount: {Amount}", 
            request.AccountId, request.TransactionType, request.Amount);

        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {request.AccountId}");

        if (account.Status != "Active")
            throw new InvalidOperationException($"Account is not active: {account.Status}");

        // Generate transaction number
        var transactionNumber = await GenerateTransactionNumberAsync();

        // Calculate balance changes
        var balanceBefore = account.Balance;
        var balanceAfter = request.TransactionType.ToLower() switch
        {
            "credit" or "deposit" => balanceBefore + request.Amount,
            "debit" or "withdrawal" => balanceBefore - request.Amount,
            _ => balanceBefore
        };

        // Validate sufficient funds for debits
        if (request.TransactionType.ToLower() is "debit" or "withdrawal")
        {
            if (account.AvailableBalance < request.Amount)
                throw new InvalidOperationException($"Insufficient available balance. Available: {account.AvailableBalance}, Required: {request.Amount}");
        }

        // Create transaction
        var transaction = new TreasuryTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = transactionNumber,
            AccountId = request.AccountId,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            Status = account.RequiresApproval ? "Pending" : "Approved",
            Description = request.Description,
            Reference = request.Reference,
            TransactionDate = DateTime.UtcNow,
            RelatedAccountId = request.RelatedAccountId,
            RequiresApproval = account.RequiresApproval,
            CreatedBy = Guid.NewGuid() // TODO: Get from JWT claims
        };

        _context.TreasuryTransactions.Add(transaction);

        // Update account balance if auto-approved
        if (!account.RequiresApproval)
        {
            account.Balance = balanceAfter;
            
            if (request.TransactionType.ToLower() is "credit" or "deposit")
            {
                account.AvailableBalance += request.Amount;
            }
            else if (request.TransactionType.ToLower() is "debit" or "withdrawal")
            {
                account.AvailableBalance -= request.Amount;
            }

            transaction.ProcessedDate = DateTime.UtcNow;
            transaction.ApprovedBy = Guid.NewGuid(); // TODO: Get from JWT claims
            transaction.ApprovedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Clear balance cache
        var cacheKey = $"treasury_balance_{request.AccountId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("Treasury transaction created successfully: {TransactionId}, Status: {Status}", 
            transaction.Id, transaction.Status);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting treasury transaction: {TransactionId}", transactionId);

        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
            throw new KeyNotFoundException($"Treasury transaction not found: {transactionId}");

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsAsync(GetTreasuryTransactionsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting treasury transactions with filters");

        var query = _context.TreasuryTransactions.AsQueryable();

        if (request.AccountId.HasValue)
            query = query.Where(t => t.AccountId == request.AccountId.Value);

        if (!string.IsNullOrEmpty(request.TransactionType))
            query = query.Where(t => t.TransactionType == request.TransactionType);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(t => t.Status == request.Status);

        if (request.FromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(t => t.TransactionDate <= request.ToDate.Value);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return transactions.Adapt<IEnumerable<TreasuryTransactionResponse>>();
    }

    public async Task<TreasuryTransactionResponse> ApproveTransactionAsync(Guid transactionId, ApproveTransactionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving treasury transaction: {TransactionId}", transactionId);

        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
            throw new KeyNotFoundException($"Treasury transaction not found: {transactionId}");

        if (transaction.Status != "Pending")
            throw new InvalidOperationException($"Transaction cannot be approved. Current status: {transaction.Status}");

        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == transaction.AccountId, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {transaction.AccountId}");

        // Validate sufficient funds for debits at approval time
        if (transaction.TransactionType.ToLower() is "debit" or "withdrawal")
        {
            if (account.AvailableBalance < transaction.Amount)
                throw new InvalidOperationException($"Insufficient available balance for approval. Available: {account.AvailableBalance}, Required: {transaction.Amount}");
        }

        // Update transaction status
        transaction.Status = "Approved";
        transaction.ApprovedBy = Guid.NewGuid(); // TODO: Get from JWT claims
        transaction.ApprovedAt = DateTime.UtcNow;
        transaction.ProcessedDate = DateTime.UtcNow;
        transaction.ApprovalNotes = request.ApprovalNotes;

        // Update account balance
        if (transaction.TransactionType.ToLower() is "credit" or "deposit")
        {
            account.Balance += transaction.Amount;
            account.AvailableBalance += transaction.Amount;
        }
        else if (transaction.TransactionType.ToLower() is "debit" or "withdrawal")
        {
            account.Balance -= transaction.Amount;
            account.AvailableBalance -= transaction.Amount;
        }

        // Update transaction balance after with actual values
        transaction.BalanceAfter = account.Balance;

        await _context.SaveChangesAsync(cancellationToken);

        // Clear balance cache
        var cacheKey = $"treasury_balance_{transaction.AccountId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("Treasury transaction approved successfully: {TransactionId}", transactionId);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> CancelTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling treasury transaction: {TransactionId}, Reason: {Reason}", transactionId, reason);

        var transaction = await _context.TreasuryTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

        if (transaction == null)
            throw new KeyNotFoundException($"Treasury transaction not found: {transactionId}");

        if (transaction.Status is "Cancelled" or "Settled")
            throw new InvalidOperationException($"Transaction cannot be cancelled. Current status: {transaction.Status}");

        // If transaction was already approved and processed, we need to reverse the balance changes
        if (transaction.Status == "Approved" && transaction.ProcessedDate.HasValue)
        {
            var account = await _context.TreasuryAccounts
                .FirstOrDefaultAsync(a => a.Id == transaction.AccountId, cancellationToken);

            if (account != null)
            {
                // Reverse the balance changes
                if (transaction.TransactionType.ToLower() is "credit" or "deposit")
                {
                    account.Balance -= transaction.Amount;
                    account.AvailableBalance -= transaction.Amount;
                }
                else if (transaction.TransactionType.ToLower() is "debit" or "withdrawal")
                {
                    account.Balance += transaction.Amount;
                    account.AvailableBalance += transaction.Amount;
                }

                // Clear balance cache
                var cacheKey = $"treasury_balance_{transaction.AccountId}";
                await _cache.RemoveAsync(cacheKey, cancellationToken);
            }
        }

        // Update transaction status
        transaction.Status = "Cancelled";
        transaction.ApprovalNotes = reason;
        transaction.ProcessedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Treasury transaction cancelled successfully: {TransactionId}", transactionId);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> TransferFundsAsync(TransferFundsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transferring funds from {FromAccountId} to {ToAccountId}, Amount: {Amount}", 
            request.FromAccountId, request.ToAccountId, request.Amount);

        // Validate accounts exist and are active
        var fromAccount = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.FromAccountId, cancellationToken);

        if (fromAccount == null)
            throw new KeyNotFoundException($"Source treasury account not found: {request.FromAccountId}");

        var toAccount = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.ToAccountId, cancellationToken);

        if (toAccount == null)
            throw new KeyNotFoundException($"Destination treasury account not found: {request.ToAccountId}");

        if (fromAccount.Status != "Active")
            throw new InvalidOperationException($"Source account is not active: {fromAccount.Status}");

        if (toAccount.Status != "Active")
            throw new InvalidOperationException($"Destination account is not active: {toAccount.Status}");

        // Validate sufficient funds
        if (fromAccount.AvailableBalance < request.Amount)
            throw new InvalidOperationException($"Insufficient available balance. Available: {fromAccount.AvailableBalance}, Required: {request.Amount}");

        // Validate currency match
        if (fromAccount.Currency != request.Currency || toAccount.Currency != request.Currency)
            throw new InvalidOperationException($"Currency mismatch. From: {fromAccount.Currency}, To: {toAccount.Currency}, Requested: {request.Currency}");

        // Generate transaction number
        var transactionNumber = await GenerateTransactionNumberAsync();

        // Create debit transaction for source account
        var debitTransaction = new TreasuryTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = $"{transactionNumber}-OUT",
            AccountId = request.FromAccountId,
            TransactionType = "Transfer Out",
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceBefore = fromAccount.Balance,
            BalanceAfter = fromAccount.Balance - request.Amount,
            Status = fromAccount.RequiresApproval ? "Pending" : "Approved",
            Description = $"Transfer to {toAccount.AccountName}: {request.Description}",
            Reference = request.Reference,
            TransactionDate = DateTime.UtcNow,
            RelatedAccountId = request.ToAccountId,
            RequiresApproval = fromAccount.RequiresApproval,
            CreatedBy = Guid.NewGuid() // TODO: Get from JWT claims
        };

        // Create credit transaction for destination account
        var creditTransaction = new TreasuryTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = $"{transactionNumber}-IN",
            AccountId = request.ToAccountId,
            TransactionType = "Transfer In",
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceBefore = toAccount.Balance,
            BalanceAfter = toAccount.Balance + request.Amount,
            Status = toAccount.RequiresApproval ? "Pending" : "Approved",
            Description = $"Transfer from {fromAccount.AccountName}: {request.Description}",
            Reference = request.Reference,
            TransactionDate = DateTime.UtcNow,
            RelatedAccountId = request.FromAccountId,
            RequiresApproval = toAccount.RequiresApproval,
            CreatedBy = Guid.NewGuid() // TODO: Get from JWT claims
        };

        _context.TreasuryTransactions.Add(debitTransaction);
        _context.TreasuryTransactions.Add(creditTransaction);

        // Update account balances if auto-approved
        if (!fromAccount.RequiresApproval)
        {
            fromAccount.Balance -= request.Amount;
            fromAccount.AvailableBalance -= request.Amount;
            debitTransaction.ProcessedDate = DateTime.UtcNow;
            debitTransaction.ApprovedBy = Guid.NewGuid(); // TODO: Get from JWT claims
            debitTransaction.ApprovedAt = DateTime.UtcNow;
        }

        if (!toAccount.RequiresApproval)
        {
            toAccount.Balance += request.Amount;
            toAccount.AvailableBalance += request.Amount;
            creditTransaction.ProcessedDate = DateTime.UtcNow;
            creditTransaction.ApprovedBy = Guid.NewGuid(); // TODO: Get from JWT claims
            creditTransaction.ApprovedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Clear balance caches
        var fromCacheKey = $"treasury_balance_{request.FromAccountId}";
        var toCacheKey = $"treasury_balance_{request.ToAccountId}";
        await _cache.RemoveAsync(fromCacheKey, cancellationToken);
        await _cache.RemoveAsync(toCacheKey, cancellationToken);

        _logger.LogInformation("Fund transfer completed successfully. Debit: {DebitTransactionId}, Credit: {CreditTransactionId}", 
            debitTransaction.Id, creditTransaction.Id);

        // Return the debit transaction as the primary transaction
        return debitTransaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> DepositFundsAsync(DepositFundsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Depositing funds to account: {AccountId}, Amount: {Amount}", 
            request.AccountId, request.Amount);

        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {request.AccountId}");

        if (account.Status != "Active")
            throw new InvalidOperationException($"Account is not active: {account.Status}");

        // Validate currency match
        if (account.Currency != request.Currency)
            throw new InvalidOperationException($"Currency mismatch. Account: {account.Currency}, Requested: {request.Currency}");

        // Generate transaction number
        var transactionNumber = await GenerateTransactionNumberAsync();

        // Create deposit transaction
        var transaction = new TreasuryTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = transactionNumber,
            AccountId = request.AccountId,
            TransactionType = "Deposit",
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceBefore = account.Balance,
            BalanceAfter = account.Balance + request.Amount,
            Status = account.RequiresApproval ? "Pending" : "Approved",
            Description = request.Description,
            Reference = request.Reference,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod,
            RequiresApproval = account.RequiresApproval,
            CreatedBy = Guid.NewGuid() // TODO: Get from JWT claims
        };

        _context.TreasuryTransactions.Add(transaction);

        // Update account balance if auto-approved
        if (!account.RequiresApproval)
        {
            account.Balance += request.Amount;
            account.AvailableBalance += request.Amount;
            transaction.ProcessedDate = DateTime.UtcNow;
            transaction.ApprovedBy = Guid.NewGuid(); // TODO: Get from JWT claims
            transaction.ApprovedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Clear balance cache
        var cacheKey = $"treasury_balance_{request.AccountId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("Deposit transaction created successfully: {TransactionId}, Status: {Status}", 
            transaction.Id, transaction.Status);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryTransactionResponse> WithdrawFundsAsync(WithdrawFundsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Withdrawing funds from account: {AccountId}, Amount: {Amount}", 
            request.AccountId, request.Amount);

        var account = await _context.TreasuryAccounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
            throw new KeyNotFoundException($"Treasury account not found: {request.AccountId}");

        if (account.Status != "Active")
            throw new InvalidOperationException($"Account is not active: {account.Status}");

        // Validate currency match
        if (account.Currency != request.Currency)
            throw new InvalidOperationException($"Currency mismatch. Account: {account.Currency}, Requested: {request.Currency}");

        // Validate sufficient funds
        if (account.AvailableBalance < request.Amount)
            throw new InvalidOperationException($"Insufficient available balance. Available: {account.AvailableBalance}, Required: {request.Amount}");

        // Validate minimum balance requirement
        var balanceAfterWithdrawal = account.Balance - request.Amount;
        if (balanceAfterWithdrawal < account.MinimumBalance)
            throw new InvalidOperationException($"Withdrawal would violate minimum balance requirement. Minimum: {account.MinimumBalance}, Balance after withdrawal: {balanceAfterWithdrawal}");

        // Generate transaction number
        var transactionNumber = await GenerateTransactionNumberAsync();

        // Create withdrawal transaction
        var transaction = new TreasuryTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = transactionNumber,
            AccountId = request.AccountId,
            TransactionType = "Withdrawal",
            Amount = request.Amount,
            Currency = request.Currency,
            BalanceBefore = account.Balance,
            BalanceAfter = account.Balance - request.Amount,
            Status = account.RequiresApproval ? "Pending" : "Approved",
            Description = request.Description,
            Reference = request.Reference,
            TransactionDate = DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod,
            RequiresApproval = account.RequiresApproval,
            CreatedBy = Guid.NewGuid() // TODO: Get from JWT claims
        };

        _context.TreasuryTransactions.Add(transaction);

        // Update account balance if auto-approved
        if (!account.RequiresApproval)
        {
            account.Balance -= request.Amount;
            account.AvailableBalance -= request.Amount;
            transaction.ProcessedDate = DateTime.UtcNow;
            transaction.ApprovedBy = Guid.NewGuid(); // TODO: Get from JWT claims
            transaction.ApprovedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Clear balance cache
        var cacheKey = $"treasury_balance_{request.AccountId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("Withdrawal transaction created successfully: {TransactionId}, Status: {Status}", 
            transaction.Id, transaction.Status);

        return transaction.Adapt<TreasuryTransactionResponse>();
    }

    public async Task<TreasuryAnalyticsResponse> GetTreasuryAnalyticsAsync(GetTreasuryAnalyticsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting treasury analytics for date range: {FromDate} to {ToDate}", 
            request.FromDate, request.ToDate);

        var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Build account query
        var accountQuery = _context.TreasuryAccounts.AsQueryable();

        if (!string.IsNullOrEmpty(request.Currency))
            accountQuery = accountQuery.Where(a => a.Currency == request.Currency);

        if (!string.IsNullOrEmpty(request.AccountType))
            accountQuery = accountQuery.Where(a => a.AccountType == request.AccountType);

        var accounts = await accountQuery.ToListAsync(cancellationToken);

        // Calculate totals
        var totalBalance = accounts.Sum(a => a.Balance);
        var totalAvailableBalance = accounts.Sum(a => a.AvailableBalance);
        var totalReservedBalance = accounts.Sum(a => a.ReservedBalance);

        // Get transaction data for the period
        var transactionQuery = _context.TreasuryTransactions
            .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate);

        if (accounts.Any())
        {
            var accountIds = accounts.Select(a => a.Id).ToList();
            transactionQuery = transactionQuery.Where(t => accountIds.Contains(t.AccountId));
        }

        var transactions = await transactionQuery.ToListAsync(cancellationToken);

        // Calculate cash flows
        var inflows = transactions
            .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                       t.TransactionType.ToLower().Contains("deposit") || 
                       t.TransactionType.ToLower().Contains("transfer in"))
            .Sum(t => t.Amount);

        var outflows = transactions
            .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                       t.TransactionType.ToLower().Contains("withdrawal") || 
                       t.TransactionType.ToLower().Contains("transfer out"))
            .Sum(t => t.Amount);

        // Calculate balance statistics
        var balances = accounts.Select(a => a.Balance).ToList();
        var averageBalance = balances.Any() ? balances.Average() : 0;
        var highestBalance = balances.Any() ? balances.Max() : 0;
        var lowestBalance = balances.Any() ? balances.Min() : 0;

        // Group balances by account type
        var balancesByAccountType = accounts
            .GroupBy(a => a.AccountType)
            .Select(g => new AccountTypeBalance
            {
                AccountType = g.Key,
                Balance = g.Sum(a => a.Balance),
                AvailableBalance = g.Sum(a => a.AvailableBalance),
                AccountCount = g.Count()
            })
            .ToList();

        // Generate daily balances for the period
        var dailyBalances = new List<DailyBalance>();
        var currentDate = fromDate.Date;
        var previousBalance = totalBalance;

        while (currentDate <= toDate.Date)
        {
            var dayTransactions = transactions
                .Where(t => t.TransactionDate.Date == currentDate)
                .ToList();

            var dayInflows = dayTransactions
                .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                           t.TransactionType.ToLower().Contains("deposit") || 
                           t.TransactionType.ToLower().Contains("transfer in"))
                .Sum(t => t.Amount);

            var dayOutflows = dayTransactions
                .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                           t.TransactionType.ToLower().Contains("withdrawal") || 
                           t.TransactionType.ToLower().Contains("transfer out"))
                .Sum(t => t.Amount);

            var netChange = dayInflows - dayOutflows;
            var changePercentage = previousBalance != 0 ? (netChange / previousBalance) * 100 : 0;

            dailyBalances.Add(new DailyBalance
            {
                Date = currentDate,
                Balance = previousBalance + netChange,
                Change = netChange,
                ChangePercentage = changePercentage
            });

            previousBalance += netChange;
            currentDate = currentDate.AddDays(1);
        }

        var currency = !string.IsNullOrEmpty(request.Currency) ? request.Currency : 
                      accounts.FirstOrDefault()?.Currency ?? "USD";

        var response = new TreasuryAnalyticsResponse
        {
            TotalBalance = totalBalance,
            TotalAvailableBalance = totalAvailableBalance,
            TotalReservedBalance = totalReservedBalance,
            Currency = currency,
            AsOfDate = DateTime.UtcNow,
            AccountCount = accounts.Count,
            TransactionCount = transactions.Count,
            TotalInflows = inflows,
            TotalOutflows = outflows,
            NetCashFlow = inflows - outflows,
            AverageBalance = averageBalance,
            HighestBalance = highestBalance,
            LowestBalance = lowestBalance,
            BalancesByAccountType = balancesByAccountType,
            DailyBalances = dailyBalances
        };

        return response;
    }

    public async Task<IEnumerable<TreasuryAccountSummaryResponse>> GetAccountSummariesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting treasury account summaries");

        var accounts = await _context.TreasuryAccounts
            .Where(a => a.Status == "Active")
            .ToListAsync(cancellationToken);

        var summaries = new List<TreasuryAccountSummaryResponse>();

        foreach (var account in accounts)
        {
            // Get last transaction date and count
            var lastTransaction = await _context.TreasuryTransactions
                .Where(t => t.AccountId == account.Id)
                .OrderByDescending(t => t.TransactionDate)
                .FirstOrDefaultAsync(cancellationToken);

            var transactionCount = await _context.TreasuryTransactions
                .CountAsync(t => t.AccountId == account.Id, cancellationToken);

            // Calculate day change
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var yesterdayTransactions = await _context.TreasuryTransactions
                .Where(t => t.AccountId == account.Id && t.TransactionDate.Date == yesterday)
                .ToListAsync(cancellationToken);

            var dayInflows = yesterdayTransactions
                .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                           t.TransactionType.ToLower().Contains("deposit") || 
                           t.TransactionType.ToLower().Contains("transfer in"))
                .Sum(t => t.Amount);

            var dayOutflows = yesterdayTransactions
                .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                           t.TransactionType.ToLower().Contains("withdrawal") || 
                           t.TransactionType.ToLower().Contains("transfer out"))
                .Sum(t => t.Amount);

            var dayChange = dayInflows - dayOutflows;
            var dayChangePercentage = account.Balance != 0 ? (dayChange / account.Balance) * 100 : 0;

            summaries.Add(new TreasuryAccountSummaryResponse
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                Currency = account.Currency,
                Balance = account.Balance,
                AvailableBalance = account.AvailableBalance,
                Status = account.Status,
                IsDefault = account.IsDefault,
                LastTransactionDate = lastTransaction?.TransactionDate ?? account.CreatedAt,
                TransactionCount = transactionCount,
                DayChange = dayChange,
                DayChangePercentage = dayChangePercentage
            });
        }

        return summaries.OrderByDescending(s => s.Balance);
    }

    public async Task<TreasuryCashFlowResponse> GetCashFlowAnalysisAsync(GetCashFlowAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting cash flow analysis for date range: {FromDate} to {ToDate}", 
            request.FromDate, request.ToDate);

        var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Build transaction query
        var transactionQuery = _context.TreasuryTransactions
            .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate);

        if (!string.IsNullOrEmpty(request.Currency))
            transactionQuery = transactionQuery.Where(t => t.Currency == request.Currency);

        if (request.AccountId.HasValue)
            transactionQuery = transactionQuery.Where(t => t.AccountId == request.AccountId.Value);

        var transactions = await transactionQuery.ToListAsync(cancellationToken);

        // Calculate opening and closing balances
        var openingBalance = 0m;
        var closingBalance = 0m;

        if (request.AccountId.HasValue)
        {
            var account = await _context.TreasuryAccounts
                .FirstOrDefaultAsync(a => a.Id == request.AccountId.Value, cancellationToken);
            
            if (account != null)
            {
                closingBalance = account.Balance;
                
                // Calculate opening balance by subtracting period transactions
                var periodInflows = transactions
                    .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                               t.TransactionType.ToLower().Contains("deposit") || 
                               t.TransactionType.ToLower().Contains("transfer in"))
                    .Sum(t => t.Amount);

                var periodOutflows = transactions
                    .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                               t.TransactionType.ToLower().Contains("withdrawal") || 
                               t.TransactionType.ToLower().Contains("transfer out"))
                    .Sum(t => t.Amount);

                openingBalance = closingBalance - (periodInflows - periodOutflows);
            }
        }
        else
        {
            // For all accounts, get current total balance
            var accountQuery = _context.TreasuryAccounts.AsQueryable();
            
            if (!string.IsNullOrEmpty(request.Currency))
                accountQuery = accountQuery.Where(a => a.Currency == request.Currency);

            var accounts = await accountQuery.ToListAsync(cancellationToken);
            closingBalance = accounts.Sum(a => a.Balance);

            // Calculate opening balance
            var periodInflows = transactions
                .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                           t.TransactionType.ToLower().Contains("deposit") || 
                           t.TransactionType.ToLower().Contains("transfer in"))
                .Sum(t => t.Amount);

            var periodOutflows = transactions
                .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                           t.TransactionType.ToLower().Contains("withdrawal") || 
                           t.TransactionType.ToLower().Contains("transfer out"))
                .Sum(t => t.Amount);

            openingBalance = closingBalance - (periodInflows - periodOutflows);
        }

        // Calculate total inflows and outflows
        var totalInflows = transactions
            .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                       t.TransactionType.ToLower().Contains("deposit") || 
                       t.TransactionType.ToLower().Contains("transfer in"))
            .Sum(t => t.Amount);

        var totalOutflows = transactions
            .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                       t.TransactionType.ToLower().Contains("withdrawal") || 
                       t.TransactionType.ToLower().Contains("transfer out"))
            .Sum(t => t.Amount);

        // Group inflows by transaction type
        var inflowsByType = transactions
            .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                       t.TransactionType.ToLower().Contains("deposit") || 
                       t.TransactionType.ToLower().Contains("transfer in"))
            .GroupBy(t => t.TransactionType)
            .Select(g => new CashFlowByType
            {
                TransactionType = g.Key,
                Amount = g.Sum(t => t.Amount),
                TransactionCount = g.Count(),
                Percentage = totalInflows > 0 ? (g.Sum(t => t.Amount) / totalInflows) * 100 : 0
            })
            .ToList();

        // Group outflows by transaction type
        var outflowsByType = transactions
            .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                       t.TransactionType.ToLower().Contains("withdrawal") || 
                       t.TransactionType.ToLower().Contains("transfer out"))
            .GroupBy(t => t.TransactionType)
            .Select(g => new CashFlowByType
            {
                TransactionType = g.Key,
                Amount = g.Sum(t => t.Amount),
                TransactionCount = g.Count(),
                Percentage = totalOutflows > 0 ? (g.Sum(t => t.Amount) / totalOutflows) * 100 : 0
            })
            .ToList();

        // Generate daily cash flows
        var dailyCashFlows = new List<DailyCashFlow>();
        var currentDate = fromDate.Date;
        var runningBalance = openingBalance;

        while (currentDate <= toDate.Date)
        {
            var dayTransactions = transactions
                .Where(t => t.TransactionDate.Date == currentDate)
                .ToList();

            var dayInflows = dayTransactions
                .Where(t => t.TransactionType.ToLower().Contains("credit") || 
                           t.TransactionType.ToLower().Contains("deposit") || 
                           t.TransactionType.ToLower().Contains("transfer in"))
                .Sum(t => t.Amount);

            var dayOutflows = dayTransactions
                .Where(t => t.TransactionType.ToLower().Contains("debit") || 
                           t.TransactionType.ToLower().Contains("withdrawal") || 
                           t.TransactionType.ToLower().Contains("transfer out"))
                .Sum(t => t.Amount);

            var netFlow = dayInflows - dayOutflows;
            runningBalance += netFlow;

            dailyCashFlows.Add(new DailyCashFlow
            {
                Date = currentDate,
                Inflows = dayInflows,
                Outflows = dayOutflows,
                NetFlow = netFlow,
                Balance = runningBalance
            });

            currentDate = currentDate.AddDays(1);
        }

        // Calculate average daily balance
        var averageDailyBalance = dailyCashFlows.Any() ? dailyCashFlows.Average(d => d.Balance) : 0;

        var currency = !string.IsNullOrEmpty(request.Currency) ? request.Currency : "USD";

        var response = new TreasuryCashFlowResponse
        {
            FromDate = fromDate,
            ToDate = toDate,
            Currency = currency,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            TotalInflows = totalInflows,
            TotalOutflows = totalOutflows,
            NetCashFlow = totalInflows - totalOutflows,
            AverageDailyBalance = averageDailyBalance,
            InflowsByType = inflowsByType,
            OutflowsByType = outflowsByType,
            DailyCashFlows = dailyCashFlows
        };

        return response;
    }

    private Task<string> GenerateAccountNumberAsync()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(1000, 9999);
        return Task.FromResult($"TR{timestamp}{random}");
    }

    private Task<string> GenerateTransactionNumberAsync()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = new Random().Next(10000, 99999);
        return Task.FromResult($"TXN{timestamp}{random}");
    }
}
