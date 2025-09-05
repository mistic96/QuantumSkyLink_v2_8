using FeeService.Models.Requests;
using FeeService.Models.Responses;

namespace FeeService.Services.Interfaces;

public interface IFeeCalculationService
{
    /// <summary>
    /// Calculate fee for a transaction
    /// </summary>
    Task<FeeCalculationResponse> CalculateFeeAsync(CalculateFeeRequest request);

    /// <summary>
    /// Estimate fee for a transaction
    /// </summary>
    Task<FeeEstimationResponse> EstimateFeeAsync(EstimateFeeRequest request);

    /// <summary>
    /// Get fee configuration for a specific fee type
    /// </summary>
    Task<FeeConfigurationResponse> GetFeeConfigurationAsync(string feeType, string? entityType = null, string? entityId = null);

    /// <summary>
    /// Create or update fee configuration
    /// </summary>
    Task<FeeConfigurationResponse> CreateOrUpdateFeeConfigurationAsync(CreateFeeConfigurationRequest request);

    /// <summary>
    /// Get all active fee configurations
    /// </summary>
    Task<IEnumerable<FeeConfigurationResponse>> GetActiveFeeConfigurationsAsync();

    /// <summary>
    /// Validate fee calculation parameters
    /// </summary>
    Task<bool> ValidateFeeParametersAsync(string feeType, decimal amount, string currency);

    /// <summary>
    /// Apply discounts to calculated fee
    /// </summary>
    Task<decimal> ApplyDiscountsAsync(Guid userId, string feeType, decimal calculatedFee);

    /// <summary>
    /// Get fee calculation history for a user
    /// </summary>
    Task<IEnumerable<FeeCalculationResponse>> GetCalculationHistoryAsync(Guid userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// Calculate fiat rejection fees including wire fees, Square fees, and internal fees
    /// </summary>
    Task<FiatRejectionFeesResponse> CalculateFiatRejectionFeesAsync(FiatRejectionFeesRequest request);

    /// <summary>
    /// Calculate crypto rejection fees including network fees and internal fees
    /// </summary>
    Task<CryptoRejectionFeesResponse> CalculateCryptoRejectionFeesAsync(CryptoRejectionFeesRequest request);
}
