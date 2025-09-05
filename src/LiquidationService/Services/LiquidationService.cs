using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using LiquidationService.Data;
using LiquidationService.Data.Entities;
using LiquidationService.Models.Requests;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using Mapster;

namespace LiquidationService.Services;

/// <summary>
/// Main liquidation service for processing liquidation requests
/// Follows the PaymentGatewayService pattern - returns response models directly and throws exceptions for errors
/// </summary>
public class LiquidationService : ILiquidationService
{
    private readonly LiquidationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<LiquidationService> _logger;
    private readonly IComplianceService _complianceService;
    private readonly IAssetEligibilityService _assetEligibilityService;
    private readonly IMarketPricingService _marketPricingService;
    private readonly ILiquidityProviderService _liquidityProviderService;

    public LiquidationService(
        LiquidationDbContext context,
        IDistributedCache cache,
        ILogger<LiquidationService> logger,
        IComplianceService complianceService,
        IAssetEligibilityService assetEligibilityService,
        IMarketPricingService marketPricingService,
        ILiquidityProviderService liquidityProviderService)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _complianceService = complianceService;
        _assetEligibilityService = assetEligibilityService;
        _marketPricingService = marketPricingService;
        _liquidityProviderService = liquidityProviderService;
    }

    /// <summary>
    /// Create a new liquidation request
    /// </summary>
    public async Task<LiquidationRequestResponse> CreateLiquidationRequestAsync(
        Guid userId, 
        CreateLiquidationRequestModel request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating liquidation request for user {UserId}, asset {AssetSymbol}, amount {AssetAmount}",
            userId, request.AssetSymbol, request.AssetAmount);

        try
        {
            // Validate asset eligibility
            var isEligible = await _assetEligibilityService.IsAssetEligibleForLiquidationAsync(
                request.AssetSymbol, request.AssetAmount, userId, null, cancellationToken);

            if (!isEligible)
                throw new InvalidOperationException($"Asset {request.AssetSymbol} is not eligible for liquidation");

            // Get asset eligibility details for risk level
            var assetEligibility = await _assetEligibilityService.GetAssetEligibilityAsync(
                request.AssetSymbol, cancellationToken);

            // Get current market price for estimation
            var priceSnapshot = await _marketPricingService.GetCurrentPriceAsync(
                request.AssetSymbol, request.OutputSymbol, cancellationToken);

            var estimatedOutputAmount = request.AssetAmount * priceSnapshot.Price;

            // Create liquidation request entity
            var liquidationRequest = new LiquidationRequest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AssetSymbol = request.AssetSymbol,
                AssetAmount = request.AssetAmount,
                OutputType = request.OutputType,
                OutputSymbol = request.OutputSymbol,
                DestinationType = request.DestinationType,
                DestinationAddress = request.DestinationAddress,
                DestinationDetails = request.DestinationDetails,
                Status = LiquidationRequestStatus.Pending,
                MarketPriceAtRequest = priceSnapshot.Price,
                EstimatedOutputAmount = estimatedOutputAmount,
                AssetEligibilityVerified = true,
                RiskLevel = assetEligibility?.RiskLevel ?? RiskLevel.Medium,
                RequiresMultiSignature = assetEligibility?.RequiresMultiSignature ?? false,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // 24-hour expiration
            };

            _context.LiquidationRequests.Add(liquidationRequest);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidation request {RequestId} created successfully for user {UserId}",
                liquidationRequest.Id, userId);

            return liquidationRequest.Adapt<LiquidationRequestResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating liquidation request for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get liquidation request by ID
    /// </summary>
    public async Task<LiquidationRequestResponse?> GetLiquidationRequestAsync(
        Guid requestId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting liquidation request {RequestId} for user {UserId}", requestId, userId);

        try
        {
            var liquidationRequest = await _context.LiquidationRequests
                .Include(lr => lr.LiquidityProvider)
                .FirstOrDefaultAsync(lr => lr.Id == requestId && lr.UserId == userId, cancellationToken);

            if (liquidationRequest == null)
            {
                _logger.LogWarning("Liquidation request {RequestId} not found for user {UserId}", requestId, userId);
                return null;
            }

            var response = liquidationRequest.Adapt<LiquidationRequestResponse>();

            // Include liquidity provider summary if assigned
            if (liquidationRequest.LiquidityProvider != null)
            {
                response.LiquidityProvider = liquidationRequest.LiquidityProvider.Adapt<LiquidityProviderSummaryResponse>();
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting liquidation request {RequestId} for user {UserId}", requestId, userId);
            throw;
        }
    }

    /// <summary>
    /// Get liquidation requests for a user with filtering
    /// </summary>
    public async Task<PaginatedResponse<LiquidationRequestResponse>> GetLiquidationRequestsAsync(
        LiquidationRequestFilterModel filter, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting liquidation requests with filters - UserId: {UserId}, Status: {Status}, Page: {Page}",
            filter.UserId, filter.Status, filter.Page);

        try
        {
            var query = _context.LiquidationRequests
                .Include(lr => lr.LiquidityProvider)
                .AsQueryable();

            // Apply filters
            if (filter.UserId.HasValue)
                query = query.Where(lr => lr.UserId == filter.UserId.Value);

            if (!string.IsNullOrEmpty(filter.AssetSymbol))
                query = query.Where(lr => lr.AssetSymbol == filter.AssetSymbol);

            if (!string.IsNullOrEmpty(filter.OutputSymbol))
                query = query.Where(lr => lr.OutputSymbol == filter.OutputSymbol);

            if (filter.Status.HasValue)
                query = query.Where(lr => lr.Status == filter.Status.Value);

            if (filter.LiquidityProviderId.HasValue)
                query = query.Where(lr => lr.LiquidityProviderId == filter.LiquidityProviderId.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(lr => lr.AssetAmount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(lr => lr.AssetAmount <= filter.MaxAmount.Value);

            if (filter.CreatedFrom.HasValue)
                query = query.Where(lr => lr.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                query = query.Where(lr => lr.CreatedAt <= filter.CreatedTo.Value);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                var isDescending = filter.SortDirection?.ToLower() == "desc";
                query = filter.SortBy.ToLower() switch
                {
                    "createdat" => isDescending ? query.OrderByDescending(lr => lr.CreatedAt) : query.OrderBy(lr => lr.CreatedAt),
                    "updatedat" => isDescending ? query.OrderByDescending(lr => lr.UpdatedAt) : query.OrderBy(lr => lr.UpdatedAt),
                    "assetamount" => isDescending ? query.OrderByDescending(lr => lr.AssetAmount) : query.OrderBy(lr => lr.AssetAmount),
                    "status" => isDescending ? query.OrderByDescending(lr => lr.Status) : query.OrderBy(lr => lr.Status),
                    _ => query.OrderByDescending(lr => lr.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(lr => lr.CreatedAt);
            }

            // Apply pagination
            var liquidationRequests = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var responses = liquidationRequests.Select(lr =>
            {
                var response = lr.Adapt<LiquidationRequestResponse>();
                if (lr.LiquidityProvider != null)
                {
                    response.LiquidityProvider = lr.LiquidityProvider.Adapt<LiquidityProviderSummaryResponse>();
                }
                return response;
            }).ToList();

            return new PaginatedResponse<LiquidationRequestResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasNextPage = filter.Page < (int)Math.Ceiling((double)totalCount / filter.PageSize),
                HasPreviousPage = filter.Page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting liquidation requests");
            throw;
        }
    }

    /// <summary>
    /// Update liquidation request status
    /// </summary>
    public async Task<LiquidationRequestResponse> UpdateLiquidationStatusAsync(
        Guid requestId, 
        UpdateLiquidationStatusModel request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating liquidation request {RequestId} status to {Status}", requestId, request.Status);

        try
        {
            var liquidationRequest = await _context.LiquidationRequests
                .Include(lr => lr.LiquidityProvider)
                .FirstOrDefaultAsync(lr => lr.Id == requestId, cancellationToken);

            if (liquidationRequest == null)
                throw new InvalidOperationException($"Liquidation request {requestId} not found");

            // Validate status transition
            if (!IsValidStatusTransition(liquidationRequest.Status, request.Status))
                throw new InvalidOperationException($"Invalid status transition from {liquidationRequest.Status} to {request.Status}");

            liquidationRequest.Status = request.Status;
            liquidationRequest.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.Reason))
            {
                liquidationRequest.Notes = $"{liquidationRequest.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Status changed to {request.Status}: {request.Reason}";
            }

            if (!string.IsNullOrEmpty(request.Notes))
            {
                liquidationRequest.Notes = $"{liquidationRequest.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {request.Notes}";
            }

            // Set completion time for final statuses
            if (request.Status == LiquidationRequestStatus.Completed || 
                request.Status == LiquidationRequestStatus.Failed ||
                request.Status == LiquidationRequestStatus.Cancelled)
            {
                liquidationRequest.CompletedAt = DateTime.UtcNow;
            }

            // Set rejection reason for failed status
            if (request.Status == LiquidationRequestStatus.Failed && !string.IsNullOrEmpty(request.Reason))
            {
                liquidationRequest.RejectionReason = request.Reason;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidation request {RequestId} status updated to {Status}", requestId, request.Status);

            var response = liquidationRequest.Adapt<LiquidationRequestResponse>();
            if (liquidationRequest.LiquidityProvider != null)
            {
                response.LiquidityProvider = liquidationRequest.LiquidityProvider.Adapt<LiquidityProviderSummaryResponse>();
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating liquidation request {RequestId} status", requestId);
            throw;
        }
    }

    /// <summary>
    /// Cancel a pending liquidation request
    /// </summary>
    public async Task<LiquidationRequestResponse> CancelLiquidationRequestAsync(
        Guid requestId, 
        Guid userId, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling liquidation request {RequestId} for user {UserId}, reason: {Reason}",
            requestId, userId, reason);

        try
        {
            var liquidationRequest = await _context.LiquidationRequests
                .Include(lr => lr.LiquidityProvider)
                .FirstOrDefaultAsync(lr => lr.Id == requestId && lr.UserId == userId, cancellationToken);

            if (liquidationRequest == null)
                throw new InvalidOperationException($"Liquidation request {requestId} not found for user {userId}");

            if (liquidationRequest.Status != LiquidationRequestStatus.Pending &&
                liquidationRequest.Status != LiquidationRequestStatus.Executing)
                throw new InvalidOperationException($"Cannot cancel liquidation request with status {liquidationRequest.Status}");

            liquidationRequest.Status = LiquidationRequestStatus.Cancelled;
            liquidationRequest.RejectionReason = reason;
            liquidationRequest.UpdatedAt = DateTime.UtcNow;
            liquidationRequest.CompletedAt = DateTime.UtcNow;
            liquidationRequest.Notes = $"{liquidationRequest.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Cancelled by user: {reason}";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidation request {RequestId} cancelled successfully", requestId);

            var response = liquidationRequest.Adapt<LiquidationRequestResponse>();
            if (liquidationRequest.LiquidityProvider != null)
            {
                response.LiquidityProvider = liquidationRequest.LiquidityProvider.Adapt<LiquidityProviderSummaryResponse>();
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling liquidation request {RequestId}", requestId);
            throw;
        }
    }

    /// <summary>
    /// Process a liquidation request through the complete workflow
    /// </summary>
    public async Task<LiquidationRequestResponse> ProcessLiquidationAsync(
        Guid requestId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing liquidation request {RequestId}", requestId);

        try
        {
            var liquidationRequest = await _context.LiquidationRequests
                .FirstOrDefaultAsync(lr => lr.Id == requestId, cancellationToken);

            if (liquidationRequest == null)
                throw new InvalidOperationException($"Liquidation request {requestId} not found");

            if (liquidationRequest.Status != LiquidationRequestStatus.Pending)
                throw new InvalidOperationException($"Cannot process liquidation request with status {liquidationRequest.Status}");

            // Update status to executing
            liquidationRequest.Status = LiquidationRequestStatus.Executing;
            liquidationRequest.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            try
            {
                // Step 1: Compliance checks
                _logger.LogInformation("Running compliance checks for liquidation request {RequestId}", requestId);
                var complianceChecks = await _complianceService.PerformComprehensiveComplianceCheckAsync(
                    liquidationRequest.Id, liquidationRequest.UserId, liquidationRequest.AssetSymbol, liquidationRequest.AssetAmount, cancellationToken);

                var complianceApproved = await _complianceService.IsComplianceApprovedAsync(
                    liquidationRequest.Id, cancellationToken);

                liquidationRequest.ComplianceApproved = complianceApproved;

                if (!complianceApproved)
                {
                    liquidationRequest.Status = LiquidationRequestStatus.Failed;
                    liquidationRequest.RejectionReason = "Compliance check failed";
                    liquidationRequest.CompletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                    throw new InvalidOperationException("Compliance check failed");
                }

                // Step 2: Get current market pricing
                _logger.LogInformation("Getting current market price for liquidation request {RequestId}", requestId);
                var currentPrice = await _marketPricingService.GetCurrentPriceAsync(
                    liquidationRequest.AssetSymbol, liquidationRequest.OutputSymbol, cancellationToken);

                // Step 3: Find best liquidity provider
                _logger.LogInformation("Finding best liquidity provider for liquidation request {RequestId}", requestId);
                var bestProvider = await _liquidityProviderService.FindBestLiquidityProviderAsync(
                    liquidationRequest.AssetSymbol, liquidationRequest.OutputSymbol, liquidationRequest.AssetAmount, cancellationToken);

                liquidationRequest.LiquidityProviderId = bestProvider.Id;

                // Step 4: Calculate final amounts and fees
                var feePercentage = bestProvider.FeePercentage ?? 0.5m; // Default 0.5% fee
                var grossOutputAmount = liquidationRequest.AssetAmount * currentPrice.Price;
                var fees = grossOutputAmount * (feePercentage / 100);
                var netOutputAmount = grossOutputAmount - fees;

                liquidationRequest.ActualOutputAmount = netOutputAmount;
                liquidationRequest.Fees = fees;
                liquidationRequest.ExchangeRate = currentPrice.Price;

                // Step 5: Execute liquidation (simulated)
                _logger.LogInformation("Executing liquidation for request {RequestId}", requestId);
                
                // In a real implementation, this would:
                // - Transfer assets from user's wallet
                // - Execute the liquidation through the liquidity provider
                // - Transfer output currency to destination
                // - Record blockchain transactions
                
                // For now, we'll simulate successful execution
                liquidationRequest.TransactionHash = $"0x{Guid.NewGuid():N}"; // Simulated transaction hash
                liquidationRequest.PaymentTransactionId = Guid.NewGuid().ToString(); // Simulated payment ID
                liquidationRequest.Status = LiquidationRequestStatus.Completed;
                liquidationRequest.CompletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Liquidation request {RequestId} processed successfully", requestId);

                // Load the updated request with liquidity provider
                var updatedRequest = await _context.LiquidationRequests
                    .Include(lr => lr.LiquidityProvider)
                    .FirstOrDefaultAsync(lr => lr.Id == requestId, cancellationToken);

                var response = updatedRequest!.Adapt<LiquidationRequestResponse>();
                if (updatedRequest.LiquidityProvider != null)
                {
                    response.LiquidityProvider = updatedRequest.LiquidityProvider.Adapt<LiquidityProviderSummaryResponse>();
                }

                return response;
            }
            catch (Exception processingEx)
            {
                // Update status to failed if processing fails
                liquidationRequest.Status = LiquidationRequestStatus.Failed;
                liquidationRequest.RejectionReason = processingEx.Message;
                liquidationRequest.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogError(processingEx, "Failed to process liquidation request {RequestId}", requestId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing liquidation request {RequestId}", requestId);
            throw;
        }
    }

    /// <summary>
    /// Estimate liquidation output amount and fees
    /// </summary>
    public async Task<object> EstimateLiquidationAsync(
        string assetSymbol,
        decimal assetAmount,
        string outputSymbol,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Estimating liquidation for {AssetSymbol} -> {OutputSymbol}, amount: {AssetAmount}",
            assetSymbol, outputSymbol, assetAmount);

        try
        {
            // Get current market price
            var priceSnapshot = await _marketPricingService.GetCurrentPriceAsync(
                assetSymbol, outputSymbol, cancellationToken);

            // Find best liquidity provider for fee estimation
            var bestProvider = await _liquidityProviderService.FindBestLiquidityProviderAsync(
                assetSymbol, outputSymbol, assetAmount, cancellationToken);

            // Calculate estimates
            var grossOutputAmount = assetAmount * priceSnapshot.Price;
            var feePercentage = bestProvider.FeePercentage ?? 0.5m;
            var estimatedFees = grossOutputAmount * (feePercentage / 100);
            var estimatedNetAmount = grossOutputAmount - estimatedFees;

            // Get price with slippage estimation
            var priceWithSlippage = await _marketPricingService.GetPriceWithSlippageAsync(
                assetSymbol, outputSymbol, assetAmount, cancellationToken);

            return new
            {
                AssetSymbol = assetSymbol,
                AssetAmount = assetAmount,
                OutputSymbol = outputSymbol,
                CurrentPrice = priceSnapshot.Price,
                GrossOutputAmount = grossOutputAmount,
                EstimatedFees = estimatedFees,
                FeePercentage = feePercentage,
                EstimatedNetAmount = estimatedNetAmount,
                EstimatedSlippage = priceWithSlippage.EstimatedSlippage,
                SlippageAdjustedAmount = estimatedNetAmount * (1 - (priceWithSlippage.EstimatedSlippage ?? 0) / 100),
                LiquidityProvider = new
                {
                    bestProvider.Id,
                    bestProvider.Name,
                    bestProvider.FeePercentage,
                    bestProvider.Rating,
                    bestProvider.AverageResponseTimeMinutes
                },
                PriceValidUntil = priceSnapshot.ExpiresAt,
                EstimatedProcessingTime = bestProvider.AverageResponseTimeMinutes ?? 30,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating liquidation for {AssetSymbol} -> {OutputSymbol}",
                assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Retry a failed liquidation request
    /// </summary>
    public async Task<LiquidationRequestResponse> RetryLiquidationAsync(
        Guid requestId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrying liquidation request {RequestId}", requestId);

        try
        {
            var liquidationRequest = await _context.LiquidationRequests
                .FirstOrDefaultAsync(lr => lr.Id == requestId, cancellationToken);

            if (liquidationRequest == null)
                throw new InvalidOperationException($"Liquidation request {requestId} not found");

            if (liquidationRequest.Status != LiquidationRequestStatus.Failed)
                throw new InvalidOperationException($"Cannot retry liquidation request with status {liquidationRequest.Status}");

            // Reset request for retry
            liquidationRequest.Status = LiquidationRequestStatus.Pending;
            liquidationRequest.RejectionReason = null;
            liquidationRequest.CompletedAt = null;
            liquidationRequest.UpdatedAt = DateTime.UtcNow;
            liquidationRequest.Notes = $"{liquidationRequest.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Retry initiated";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidation request {RequestId} reset for retry", requestId);

            // Process the liquidation
            return await ProcessLiquidationAsync(requestId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying liquidation request {RequestId}", requestId);
            throw;
        }
    }

    /// <summary>
    /// Get liquidation statistics for a user
    /// </summary>
    public async Task<object> GetLiquidationStatisticsAsync(
        Guid userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting liquidation statistics for user {UserId}", userId);

        try
        {
            var query = _context.LiquidationRequests.Where(lr => lr.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(lr => lr.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(lr => lr.CreatedAt <= toDate.Value);

            var requests = await query.ToListAsync(cancellationToken);

            var totalRequests = requests.Count;
            var completedRequests = requests.Count(r => r.Status == LiquidationRequestStatus.Completed);
            var failedRequests = requests.Count(r => r.Status == LiquidationRequestStatus.Failed);
            var pendingRequests = requests.Count(r => r.Status == LiquidationRequestStatus.Pending || r.Status == LiquidationRequestStatus.Executing);

            var totalAssetValue = requests.Where(r => r.Status == LiquidationRequestStatus.Completed)
                .Sum(r => r.AssetAmount * (r.MarketPriceAtRequest ?? 0));

            var totalOutputValue = requests.Where(r => r.Status == LiquidationRequestStatus.Completed)
                .Sum(r => r.ActualOutputAmount ?? 0);

            var totalFees = requests.Where(r => r.Status == LiquidationRequestStatus.Completed)
                .Sum(r => r.Fees ?? 0);

            var assetBreakdown = requests.GroupBy(r => r.AssetSymbol)
                .Select(g => new
                {
                    AssetSymbol = g.Key,
                    TotalRequests = g.Count(),
                    CompletedRequests = g.Count(r => r.Status == LiquidationRequestStatus.Completed),
                    TotalAmount = g.Sum(r => r.AssetAmount),
                    TotalValue = g.Where(r => r.Status == LiquidationRequestStatus.Completed)
                        .Sum(r => r.ActualOutputAmount ?? 0)
                })
                .ToList();

            return new
            {
                UserId = userId,
                Period = new
                {
                    FromDate = fromDate,
                    ToDate = toDate
                },
                Summary = new
                {
                    TotalRequests = totalRequests,
                    CompletedRequests = completedRequests,
                    FailedRequests = failedRequests,
                    PendingRequests = pendingRequests,
                    SuccessRate = totalRequests > 0 ? (decimal)completedRequests / totalRequests * 100 : 0,
                    TotalAssetValue = totalAssetValue,
                    TotalOutputValue = totalOutputValue,
                    TotalFees = totalFees,
                    AverageFeePercentage = completedRequests > 0 && totalOutputValue > 0 ? totalFees / totalOutputValue * 100 : 0
                },
                AssetBreakdown = assetBreakdown,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting liquidation statistics for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Validate if a status transition is allowed
    /// </summary>
    private static bool IsValidStatusTransition(LiquidationRequestStatus currentStatus, LiquidationRequestStatus newStatus)
    {
        return currentStatus switch
        {
            LiquidationRequestStatus.Pending => newStatus is LiquidationRequestStatus.Executing or LiquidationRequestStatus.Cancelled or LiquidationRequestStatus.Failed,
            LiquidationRequestStatus.Executing => newStatus is LiquidationRequestStatus.Completed or LiquidationRequestStatus.Failed or LiquidationRequestStatus.Cancelled,
            LiquidationRequestStatus.Completed => false, // No transitions from completed
            LiquidationRequestStatus.Failed => newStatus is LiquidationRequestStatus.Pending, // Allow retry
            LiquidationRequestStatus.Cancelled => false, // No transitions from cancelled
            _ => false
        };
    }
}
