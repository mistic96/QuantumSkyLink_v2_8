using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;

namespace AccountService.Services.Interfaces;

public interface IAccountService
{
    // Account Management
    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);
    Task<AccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<AccountResponse?> GetAccountByNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountSummaryResponse>> GetUserAccountsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AccountResponse> UpdateAccountStatusAsync(Guid accountId, UpdateAccountStatusRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Balance Operations
    Task<AccountBalanceResponse> GetAccountBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBalanceAsync(Guid accountId, decimal amount, TransactionType transactionType, string description, string? correlationId = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateTransactionAsync(Guid accountId, decimal amount, TransactionType transactionType, CancellationToken cancellationToken = default);

    // Account Validation
    Task<bool> AccountExistsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> UserOwnsAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> IsAccountActiveAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Account Statistics
    Task<int> GetUserAccountCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalUserBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountSummaryResponse>> GetAccountsByTypeAsync(Guid userId, AccountType accountType, CancellationToken cancellationToken = default);
}
