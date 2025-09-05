using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;

namespace AccountService.Services.Interfaces;

public interface IAccountLimitService
{
    // Limit Management
    Task<AccountLimitResponse> CreateLimitAsync(CreateAccountLimitRequest request, CancellationToken cancellationToken = default);
    Task<AccountLimitResponse> GetLimitAsync(Guid limitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AccountLimitResponse>> GetAccountLimitsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<AccountLimitResponse?> GetLimitByTypeAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default);
    Task<AccountLimitResponse> UpdateLimitAsync(Guid limitId, UpdateAccountLimitRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteLimitAsync(Guid limitId, CancellationToken cancellationToken = default);

    // Limit Validation
    Task<bool> LimitExistsAsync(Guid limitId, CancellationToken cancellationToken = default);
    Task<bool> UserOwnsLimitAsync(Guid userId, Guid limitId, CancellationToken cancellationToken = default);
    Task<bool> ValidateTransactionAgainstLimitsAsync(Guid accountId, decimal amount, TransactionType transactionType, CancellationToken cancellationToken = default);
    Task<LimitValidationResponse> CheckTransactionLimitsAsync(Guid accountId, decimal amount, TransactionType transactionType, CancellationToken cancellationToken = default);

    // Limit Usage Tracking
    Task<decimal> GetCurrentUsageAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default);
    Task<decimal> GetRemainingLimitAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default);
    Task<LimitUsageResponse> GetLimitUsageAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default);
    Task<IEnumerable<LimitUsageResponse>> GetAllLimitUsageAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Limit Reset and Management
    Task<bool> ResetLimitUsageAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default);
    Task<bool> ResetAllLimitUsageAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<AccountLimitResponse> IncreaseLimitAsync(Guid limitId, decimal newAmount, string reason, CancellationToken cancellationToken = default);
    Task<AccountLimitResponse> DecreaseLimitAsync(Guid limitId, decimal newAmount, string reason, CancellationToken cancellationToken = default);

    // Limit Monitoring
    Task<IEnumerable<LimitAlertResponse>> GetLimitAlertsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> IsLimitExceededAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default);
    Task<bool> IsLimitNearExceededAsync(Guid accountId, LimitType limitType, decimal threshold = 0.8m, CancellationToken cancellationToken = default);

    // Default Limits
    Task<IEnumerable<AccountLimitResponse>> CreateDefaultLimitsAsync(Guid accountId, AccountType accountType, CancellationToken cancellationToken = default);
    Task<IEnumerable<DefaultLimitResponse>> GetDefaultLimitsForAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
    Task<bool> ApplyDefaultLimitsAsync(Guid accountId, CancellationToken cancellationToken = default);

    // Limit History
    Task<IEnumerable<LimitHistoryResponse>> GetLimitHistoryAsync(Guid accountId, LimitType? limitType = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<LimitStatisticsResponse> GetLimitStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Additional methods expected by controllers
    Task<IEnumerable<AccountLimitResponse>> GetUserLimitsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LimitValidationResponse> ValidateLimitAsync(Guid accountId, decimal amount, LimitType limitType, CancellationToken cancellationToken = default);
    Task<ComplianceCheckResponse> CheckLimitAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default);
    Task<IEnumerable<LimitUsageResponse>> GetRemainingLimitsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUsageAsync(Guid accountId, UpdateUsageRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<DefaultLimitResponse>> GetDefaultLimitRequirementsAsync(AccountType accountType, CancellationToken cancellationToken = default);
}
