using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;

namespace AccountService.Services.Interfaces;

public interface IAccountTransactionService
{
    // Transaction Management
    Task<AccountTransactionResponse> CreateTransactionAsync(CreateAccountTransactionRequest request, CancellationToken cancellationToken = default);
    Task<AccountTransactionResponse> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountTransactionResponse>> GetAccountTransactionsAsync(Guid accountId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountTransactionResponse>> GetUserTransactionsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountTransactionResponse>> GetTransactionsByTypeAsync(Guid accountId, TransactionType transactionType, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountTransactionResponse>> GetTransactionsByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    // Transaction Status Management
    Task<AccountTransactionResponse> UpdateTransactionStatusAsync(Guid transactionId, TransactionStatus status, string? notes = null, CancellationToken cancellationToken = default);
    Task<bool> CancelTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);

    // Transaction Validation
    Task<bool> TransactionExistsAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<bool> UserOwnsTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);

    // Transaction Statistics
    Task<decimal> GetAccountTransactionSumAsync(Guid accountId, TransactionType? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<int> GetAccountTransactionCountAsync(Guid accountId, TransactionType? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<TransactionStatisticsResponse> GetTransactionStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Transaction Processing
    Task<AccountTransactionResponse> ProcessDepositAsync(Guid accountId, decimal amount, string description, string? correlationId = null, CancellationToken cancellationToken = default);
    Task<AccountTransactionResponse> ProcessWithdrawalAsync(Guid accountId, decimal amount, string description, string? correlationId = null, CancellationToken cancellationToken = default);
    Task<AccountTransactionResponse> ProcessTransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string description, string? correlationId = null, CancellationToken cancellationToken = default);
}
