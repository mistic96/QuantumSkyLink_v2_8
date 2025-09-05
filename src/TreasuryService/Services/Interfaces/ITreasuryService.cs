using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Services.Interfaces;

public interface ITreasuryService
{
    // Treasury Account Management
    Task<TreasuryAccountResponse> CreateAccountAsync(CreateTreasuryAccountRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> GetAccountByNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryAccountResponse>> GetAccountsAsync(GetTreasuryAccountsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryAccountResponse> UpdateAccountAsync(Guid accountId, UpdateTreasuryAccountRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Balance Management
    Task<TreasuryBalanceResponse> GetCurrentBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryBalanceResponse>> GetBalanceHistoryAsync(GetBalanceHistoryRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryBalanceResponse> ReconcileBalanceAsync(ReconcileBalanceRequest request, CancellationToken cancellationToken = default);

    // Transaction Management
    Task<TreasuryTransactionResponse> CreateTransactionAsync(CreateTreasuryTransactionRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsAsync(GetTreasuryTransactionsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> ApproveTransactionAsync(Guid transactionId, ApproveTransactionRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> CancelTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);

    // Transfer Operations
    Task<TreasuryTransactionResponse> TransferFundsAsync(TransferFundsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> DepositFundsAsync(DepositFundsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> WithdrawFundsAsync(WithdrawFundsRequest request, CancellationToken cancellationToken = default);

    // Treasury Analytics
    Task<TreasuryAnalyticsResponse> GetTreasuryAnalyticsAsync(GetTreasuryAnalyticsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryAccountSummaryResponse>> GetAccountSummariesAsync(CancellationToken cancellationToken = default);
    Task<TreasuryCashFlowResponse> GetCashFlowAnalysisAsync(GetCashFlowAnalysisRequest request, CancellationToken cancellationToken = default);
}
