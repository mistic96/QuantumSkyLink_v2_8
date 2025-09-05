using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuantumLedger.Hub.Models;

namespace QuantumLedger.Hub.Endpoints
{
    /// <summary>
    /// Balance and transaction history endpoints using Minimal APIs.
    /// Handles balance queries, transaction history, and pending transactions.
    /// </summary>
    public static class BalanceEndpoints
    {
        /// <summary>
        /// Maps all balance-related endpoints to the application.
        /// </summary>
        public static void MapBalanceEndpoints(this IEndpointRouteBuilder app)
        {
            var balanceGroup = app.MapGroup("/api/balances")
                .WithTags("Balances")
                .WithOpenApi();

            // Get account balance
            balanceGroup.MapGet("/{accountId:guid}", GetAccountBalanceAsync)
                .WithName("GetAccountBalanceByGuid")
                .WithSummary("Get account balance")
                .WithDescription("Retrieves the current balance for an account")
                .Produces<BalanceResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Get transaction history
            balanceGroup.MapGet("/{accountId:guid}/transactions", GetTransactionHistoryAsync)
                .WithName("GetTransactionHistory")
                .WithSummary("Get transaction history")
                .WithDescription("Retrieves transaction history for an account with pagination")
                .Produces<TransactionHistoryResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Get pending transactions
            balanceGroup.MapGet("/{accountId:guid}/pending", GetPendingTransactionsAsync)
                .WithName("GetPendingTransactions")
                .WithSummary("Get pending transactions")
                .WithDescription("Retrieves pending transactions for an account")
                .Produces<PendingTransactionsResponse>(200)
                .Produces<ErrorResponse>(404)
                .Produces<ErrorResponse>(500);

            // Balance adjustment (admin operation)
            balanceGroup.MapPost("/{accountId:guid}/adjust", AdjustBalanceAsync)
                .WithName("AdjustBalance")
                .WithSummary("Adjust account balance")
                .WithDescription("Performs administrative balance adjustments")
                .Produces<BalanceAdjustmentResponse>(200)
                .Produces<ErrorResponse>(400)
                .Produces<ErrorResponse>(401)
                .Produces<ErrorResponse>(403)
                .Produces<ErrorResponse>(500);
        }

        /// <summary>
        /// Gets account balance for all currencies or a specific currency.
        /// </summary>
        private static async Task<IResult> GetAccountBalanceAsync(
            Guid accountId,
            ILogger<string> logger,
            string? currency = null)
        {
            try
            {
                logger.LogDebug("Retrieving balance for account {AccountId}", accountId);

                // Mock implementation
                await Task.Delay(15);

                var balances = new List<CurrencyBalance>
                {
                    new CurrencyBalance { Currency = "QLT", Available = 1250.75m, Pending = 50.25m, Total = 1301.00m },
                    new CurrencyBalance { Currency = "BTC", Available = 0.05m, Pending = 0.001m, Total = 0.051m },
                    new CurrencyBalance { Currency = "ETH", Available = 2.5m, Pending = 0.1m, Total = 2.6m }
                };

                // Filter by currency if specified
                if (!string.IsNullOrEmpty(currency))
                {
                    balances = balances.Where(b => b.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    if (!balances.Any())
                    {
                        return Results.NotFound(new ErrorResponse
                        {
                            Error = "currency_not_found",
                            ErrorDescription = $"No balance found for currency '{currency}'",
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }

                var response = new BalanceResponse
                {
                    AccountId = accountId,
                    Balances = balances,
                    LastUpdated = DateTime.UtcNow.AddMinutes(-5),
                    TotalValueUSD = 45250.30m // Mock USD value
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving balance for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Balance Retrieval Error",
                    detail: "An error occurred while retrieving the account balance",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets transaction history for an account with pagination and filtering.
        /// </summary>
        private static async Task<IResult> GetTransactionHistoryAsync(
            Guid accountId,
            ILogger<string> logger,
            int page = 1,
            int pageSize = 20,
            string? type = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                logger.LogDebug("Retrieving transaction history for account {AccountId}", accountId);

                // Validate pagination
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Validate date range
                if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_date_range",
                        ErrorDescription = "From date cannot be later than to date",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Mock implementation
                await Task.Delay(25);

                var transactions = Enumerable.Range(1, pageSize).Select(i => new TransactionSummary
                {
                    TransactionId = Guid.NewGuid(),
                    Type = type ?? (i % 2 == 0 ? "Transfer" : "Deposit"),
                    Amount = 100m + (i * 10),
                    Currency = "QLT",
                    Status = status ?? "Completed",
                    FromAccountId = i % 2 == 0 ? accountId : Guid.NewGuid(),
                    ToAccountId = i % 2 == 0 ? Guid.NewGuid() : accountId,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    ProcessedAt = DateTime.UtcNow.AddDays(-i).AddMinutes(5),
                    BlockHash = $"0x{Guid.NewGuid():N}",
                    TransactionFee = (100m + (i * 10)) * 0.001m
                }).ToList();

                // Apply date filtering if specified
                if (fromDate.HasValue)
                {
                    transactions = transactions.Where(t => t.CreatedAt >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    transactions = transactions.Where(t => t.CreatedAt <= toDate.Value).ToList();
                }

                var response = new TransactionHistoryResponse
                {
                    AccountId = accountId,
                    Transactions = transactions,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = 500, // Mock total
                    TotalPages = (int)Math.Ceiling(500.0 / pageSize),
                    TotalVolume = transactions.Sum(t => t.Amount),
                    TotalFees = transactions.Sum(t => t.TransactionFee)
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving transaction history for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Transaction History Error",
                    detail: "An error occurred while retrieving transaction history",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Gets pending transactions for an account.
        /// </summary>
        private static async Task<IResult> GetPendingTransactionsAsync(
            Guid accountId,
            ILogger<string> logger)
        {
            try
            {
                logger.LogDebug("Retrieving pending transactions for account {AccountId}", accountId);

                // Mock implementation
                await Task.Delay(15);

                var pendingTransactions = new List<TransactionSummary>
                {
                    new TransactionSummary
                    {
                        TransactionId = Guid.NewGuid(),
                        Type = "Transfer",
                        Amount = 250.00m,
                        Currency = "QLT",
                        Status = "Pending",
                        FromAccountId = accountId,
                        ToAccountId = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                        TransactionFee = 0.25m
                    },
                    new TransactionSummary
                    {
                        TransactionId = Guid.NewGuid(),
                        Type = "Withdrawal",
                        Amount = 100.00m,
                        Currency = "QLT",
                        Status = "Processing",
                        FromAccountId = accountId,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                        TransactionFee = 0.10m
                    }
                };

                var response = new PendingTransactionsResponse
                {
                    AccountId = accountId,
                    PendingTransactions = pendingTransactions,
                    TotalPending = pendingTransactions.Count,
                    TotalPendingAmount = pendingTransactions.Sum(t => t.Amount),
                    OldestPendingAt = pendingTransactions.Min(t => t.CreatedAt)
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving pending transactions for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Pending Transactions Error",
                    detail: "An error occurred while retrieving pending transactions",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Performs administrative balance adjustments.
        /// </summary>
        private static async Task<IResult> AdjustBalanceAsync(
            Guid accountId,
            BalanceAdjustmentRequest request,
            ILogger<string> logger)
        {
            try
            {
                logger.LogInformation("Processing balance adjustment for account {AccountId}: {Type} {Amount} {Currency}", 
                    accountId, request.AdjustmentType, request.Amount, request.Currency);

                // Validate adjustment type
                var validTypes = new[] { "Credit", "Debit", "Correction" };
                if (!validTypes.Contains(request.AdjustmentType))
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_adjustment_type",
                        ErrorDescription = $"Adjustment type must be one of: {string.Join(", ", validTypes)}",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Validate amount
                if (request.Amount == 0)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "invalid_amount",
                        ErrorDescription = "Adjustment amount cannot be zero",
                        Timestamp = DateTime.UtcNow
                    });
                }

                // For debit operations, ensure amount is negative
                var adjustmentAmount = request.AdjustmentType == "Debit" ? -Math.Abs(request.Amount) : Math.Abs(request.Amount);

                // Mock implementation
                await Task.Delay(50);

                var previousBalance = 1000.00m; // Mock current balance
                var newBalance = previousBalance + adjustmentAmount;

                // Validate sufficient balance for debits
                if (request.AdjustmentType == "Debit" && newBalance < 0)
                {
                    return Results.BadRequest(new ErrorResponse
                    {
                        Error = "insufficient_balance",
                        ErrorDescription = "Adjustment would result in negative balance",
                        Timestamp = DateTime.UtcNow
                    });
                }

                var adjustmentId = Guid.NewGuid();

                var response = new BalanceAdjustmentResponse
                {
                    AdjustmentId = adjustmentId,
                    AccountId = accountId,
                    Currency = request.Currency,
                    Amount = adjustmentAmount,
                    AdjustmentType = request.AdjustmentType,
                    PreviousBalance = previousBalance,
                    NewBalance = newBalance,
                    Reason = request.Reason,
                    Reference = request.Reference,
                    AdminUserId = request.AdminUserId,
                    ProcessedAt = DateTime.UtcNow,
                    Status = "Completed"
                };

                logger.LogInformation("Successfully processed balance adjustment {AdjustmentId} for account {AccountId}", 
                    adjustmentId, accountId);

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing balance adjustment for account {AccountId}", accountId);
                return Results.Problem(
                    title: "Balance Adjustment Error",
                    detail: "An error occurred while processing the balance adjustment",
                    statusCode: 500);
            }
        }
    }
}
