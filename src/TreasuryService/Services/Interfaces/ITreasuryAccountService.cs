using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Services.Interfaces;

public interface ITreasuryAccountService
{
    // Treasury Account CRUD Operations
    Task<TreasuryAccountResponse> CreateAccountAsync(CreateTreasuryAccountRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> GetAccountByNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryAccountResponse>> GetAccountsAsync(GetTreasuryAccountsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> UpdateAccountAsync(Guid accountId, UpdateTreasuryAccountRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> ActivateAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    
    // Account Status Management
    Task<bool> FreezeAccountAsync(Guid accountId, string reason, CancellationToken cancellationToken = default);
    Task<bool> UnfreezeAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> CloseAccountAsync(Guid accountId, string reason, CancellationToken cancellationToken = default);
    
    // Account Configuration
    Task<TreasuryAccountResponse> SetMinimumBalanceAsync(Guid accountId, decimal minimumBalance, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> SetMaximumBalanceAsync(Guid accountId, decimal maximumBalance, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> SetInterestRateAsync(Guid accountId, decimal interestRate, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> SetDefaultAccountAsync(Guid accountId, string currency, CancellationToken cancellationToken = default);
    
    // Account Validation
    Task<bool> ValidateAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> UserOwnsAccountAsync(Guid userId, Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryAccountResponse>> GetUserAccountsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Account Statistics
    Task<AccountStatisticsResponse> GetAccountStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryAccountSummaryResponse>> GetAccountSummariesAsync(CancellationToken cancellationToken = default);
}
