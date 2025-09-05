using LiquidationService.Models.Requests;
using LiquidationService.Models.Responses;

namespace LiquidationService.Services.Interfaces;

/// <summary>
/// Main liquidation service interface for processing liquidation requests
/// </summary>
public interface ILiquidationService
{
    /// <summary>
    /// Create a new liquidation request
    /// </summary>
    /// <param name="userId">User ID making the request</param>
    /// <param name="request">Liquidation request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created liquidation request response</returns>
    Task<LiquidationRequestResponse> CreateLiquidationRequestAsync(
        Guid userId, 
        CreateLiquidationRequestModel request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get liquidation request by ID
    /// </summary>
    /// <param name="requestId">Liquidation request ID</param>
    /// <param name="userId">User ID for authorization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidation request response</returns>
    Task<LiquidationRequestResponse?> GetLiquidationRequestAsync(
        Guid requestId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get liquidation requests for a user with filtering
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated liquidation requests</returns>
    Task<PaginatedResponse<LiquidationRequestResponse>> GetLiquidationRequestsAsync(
        LiquidationRequestFilterModel filter, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update liquidation request status
    /// </summary>
    /// <param name="requestId">Liquidation request ID</param>
    /// <param name="request">Status update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidation request response</returns>
    Task<LiquidationRequestResponse> UpdateLiquidationStatusAsync(
        Guid requestId, 
        UpdateLiquidationStatusModel request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a pending liquidation request
    /// </summary>
    /// <param name="requestId">Liquidation request ID</param>
    /// <param name="userId">User ID for authorization</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidation request response</returns>
    Task<LiquidationRequestResponse> CancelLiquidationRequestAsync(
        Guid requestId, 
        Guid userId, 
        string reason, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a liquidation request through the complete workflow
    /// </summary>
    /// <param name="requestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed liquidation request response</returns>
    Task<LiquidationRequestResponse> ProcessLiquidationAsync(
        Guid requestId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimate liquidation output amount and fees
    /// </summary>
    /// <param name="assetSymbol">Asset to liquidate</param>
    /// <param name="assetAmount">Amount to liquidate</param>
    /// <param name="outputSymbol">Desired output currency</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidation estimation</returns>
    Task<object> EstimateLiquidationAsync(
        string assetSymbol,
        decimal assetAmount,
        string outputSymbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retry a failed liquidation request
    /// </summary>
    /// <param name="requestId">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidation request response</returns>
    Task<LiquidationRequestResponse> RetryLiquidationAsync(
        Guid requestId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get liquidation statistics for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="fromDate">Start date for statistics</param>
    /// <param name="toDate">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidation statistics</returns>
    Task<object> GetLiquidationStatisticsAsync(
        Guid userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
