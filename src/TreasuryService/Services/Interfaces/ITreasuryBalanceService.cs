using TreasuryService.Data.Entities;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Services.Interfaces;

public interface ITreasuryBalanceService
{
    // Balance Management
    Task<TreasuryBalanceResponse> GetCurrentBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryBalanceResponse>> GetBalanceHistoryAsync(GetBalanceHistoryRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryBalanceResponse> ReconcileBalanceAsync(ReconcileBalanceRequest request, CancellationToken cancellationToken = default);
    Task<TreasuryBalanceResponse> UpdateBalanceAsync(Guid accountId, decimal amount, string transactionType, Guid? transactionId = null, CancellationToken cancellationToken = default);
    
    // Balance Validation
    Task<bool> ValidateBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
    Task<bool> HasSufficientBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
    Task<BalanceValidationResponse> ValidateTransactionBalanceAsync(Guid accountId, decimal amount, string transactionType, CancellationToken cancellationToken = default);
    
    // Balance Reservations
    Task<bool> ReserveBalanceAsync(Guid accountId, decimal amount, Guid transactionId, CancellationToken cancellationToken = default);
    Task<bool> ReleaseReservedBalanceAsync(Guid accountId, decimal amount, Guid transactionId, CancellationToken cancellationToken = default);
    Task<bool> CommitReservedBalanceAsync(Guid accountId, decimal amount, Guid transactionId, CancellationToken cancellationToken = default);
    Task<decimal> GetReservedBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<decimal> GetAvailableBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    
    // Balance Monitoring
    Task<IEnumerable<BalanceAlertResponse>> GetBalanceAlertsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> CheckMinimumBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> CheckMaximumBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TreasuryAccountResponse>> GetAccountsNearLimitsAsync(CancellationToken cancellationToken = default);
    
    // Balance Snapshots
    Task<BalanceSnapshotResponse> CreateBalanceSnapshotAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BalanceSnapshotResponse>> GetBalanceSnapshotsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<BalanceComparisonResponse> CompareBalanceSnapshotsAsync(Guid accountId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    
    // Multi-Currency Balance Management
    Task<IEnumerable<CurrencyBalanceResponse>> GetCurrencyBalancesAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<CurrencyBalanceResponse> GetCurrencyBalanceAsync(Guid accountId, string currency, CancellationToken cancellationToken = default);
    Task<decimal> ConvertBalanceAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    
    // Balance Analytics
    Task<BalanceAnalyticsResponse> GetBalanceAnalyticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<BalanceTrendResponse>> GetBalanceTrendsAsync(Guid accountId, string period = "daily", int periods = 30, CancellationToken cancellationToken = default);
    Task<BalanceProjectionResponse> ProjectBalanceAsync(Guid accountId, int days = 30, CancellationToken cancellationToken = default);
    
    // Balance Synchronization
    Task<bool> SyncBalanceWithExternalSourceAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<BalanceSyncResponse> GetBalanceSyncStatusAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> ForceBalanceSyncAsync(Guid accountId, decimal externalBalance, string source, CancellationToken cancellationToken = default);
}
