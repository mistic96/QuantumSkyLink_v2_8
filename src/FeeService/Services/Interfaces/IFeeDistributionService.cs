using FeeService.Models.Requests;
using FeeService.Models.Responses;

namespace FeeService.Services.Interfaces;

public interface IFeeDistributionService
{
    /// <summary>
    /// Distribute fees according to distribution rules
    /// </summary>
    Task<IEnumerable<FeeDistributionResponse>> DistributeFeesAsync(DistributeFeesRequest request);

    /// <summary>
    /// Create or update distribution rule
    /// </summary>
    Task<DistributionRuleResponse> CreateOrUpdateDistributionRuleAsync(CreateDistributionRuleRequest request);

    /// <summary>
    /// Get distribution rules for a fee type
    /// </summary>
    Task<IEnumerable<DistributionRuleResponse>> GetDistributionRulesAsync(string feeType, bool activeOnly = true);

    /// <summary>
    /// Get distribution history for a transaction
    /// </summary>
    Task<IEnumerable<FeeDistributionResponse>> GetDistributionHistoryAsync(Guid transactionId);

    /// <summary>
    /// Process settlement for distributions
    /// </summary>
    Task<SettlementResponse> ProcessSettlementAsync(ProcessSettlementRequest request);

    /// <summary>
    /// Get pending distributions
    /// </summary>
    Task<IEnumerable<FeeDistributionResponse>> GetPendingDistributionsAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Update distribution status
    /// </summary>
    Task<FeeDistributionResponse> UpdateDistributionStatusAsync(Guid distributionId, string status, string? reason = null);

    /// <summary>
    /// Validate distribution rules
    /// </summary>
    Task<bool> ValidateDistributionRulesAsync(string feeType);

    /// <summary>
    /// Get distribution statistics
    /// </summary>
    Task<DistributionStatisticsResponse> GetDistributionStatisticsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        string? feeType = null);
}
