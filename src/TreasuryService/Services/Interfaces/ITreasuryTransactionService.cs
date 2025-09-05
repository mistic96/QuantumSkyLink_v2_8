using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Services.Interfaces;

public interface ITreasuryTransactionService
{
    // Transaction CRUD Operations
    Task<TreasuryTransactionResponse> CreateTransactionAsync(CreateTreasuryTransactionRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsAsync(GetTreasuryTransactionsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetAccountTransactionsAsync(Guid accountId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetTransactionsByTypeAsync(Guid accountId, string transactionType, CancellationToken cancellationToken = default);
    
    // Transaction Processing
    Task<TreasuryTransactionResponse> ProcessTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> ApproveTransactionAsync(Guid transactionId, ApproveTransactionRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> RejectTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> CancelTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);
    
    // Fund Transfer Operations
    Task<TreasuryTransactionResponse> TransferFundsAsync(TransferFundsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> DepositFundsAsync(DepositFundsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> WithdrawFundsAsync(WithdrawFundsRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> InternalTransferAsync(InternalTransferRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryTransactionResponse> ExternalTransferAsync(ExternalTransferRequest request, CancellationToken cancellationToken = default);
    
    // Transaction Validation
    Task<bool> ValidateTransactionAsync(CreateTreasuryTransactionRequest request, CancellationToken cancellationToken = default);
    Task<bool> UserOwnsTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);
    Task<bool> CanProcessTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<TransactionValidationResponse> ValidateTransferAsync(TransferFundsRequest request, CancellationToken cancellationToken = default);
    
    // Transaction Status Management
    Task<TreasuryTransactionResponse> UpdateTransactionStatusAsync(Guid transactionId, string status, string? notes = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetFailedTransactionsAsync(CancellationToken cancellationToken = default);
    Task<bool> RetryFailedTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    
    // Transaction Analytics
    Task<TransactionStatisticsResponse> GetTransactionStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TransactionSummaryResponse>> GetTransactionSummaryAsync(GetTransactionSummaryRequest request, CancellationToken cancellationToken = default);
    Task<decimal> GetTransactionVolumeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    // Reconciliation
    Task<ReconciliationResponse> ReconcileTransactionsAsync(ReconcileTransactionsRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryTransactionResponse>> GetUnreconciledTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> MarkTransactionReconciledAsync(Guid transactionId, string reconciliationReference, CancellationToken cancellationToken = default);
}
