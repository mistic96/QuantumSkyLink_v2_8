using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketplaceService.Services.Interfaces;
using MarketplaceService.Data.Entities;
using MarketplaceService.Models.Requests;
using MarketplaceService.Models.Responses;
using MarketplaceService.Models.Shared;
using System.Security.Claims;
using Mapster;

namespace MarketplaceService.Controllers;

/// <summary>
/// Secondary Market Controller - Handles peer-to-peer trading of existing tokens and crypto assets
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecondaryMarketController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly IOrderService _orderService;
    private readonly IPricingService _pricingService;
    private readonly IMarketAnalyticsService _analyticsService;
    private readonly IMarketplaceIntegrationService _integrationService;
    private readonly ILogger<SecondaryMarketController> _logger;

    public SecondaryMarketController(
        IListingService listingService,
        IOrderService orderService,
        IPricingService pricingService,
        IMarketAnalyticsService analyticsService,
        IMarketplaceIntegrationService integrationService,
        ILogger<SecondaryMarketController> logger)
    {
        _listingService = listingService;
        _orderService = orderService;
        _pricingService = pricingService;
        _analyticsService = analyticsService;
        _integrationService = integrationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new secondary market listing for existing tokens or crypto assets
    /// </summary>
    [HttpPost("listings")]
    public async Task<ActionResult<SecondaryMarketListingResponse>> CreateListingAsync(
        [FromBody] CreateSecondaryMarketListingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating secondary market listing for user {UserId}, asset {AssetIdentifier}", 
                userId, request.AssetIdentifier);

            // Verify asset ownership based on asset type
            if (request.AssetType == AssetType.PlatformToken)
            {
                // For platform tokens, verify ownership through TokenService
                if (Guid.TryParse(request.AssetIdentifier, out var tokenId))
                {
                    var tokenValidation = await _integrationService.ValidateTokenOwnershipAsync(
                        userId, tokenId, request.TotalQuantity, cancellationToken);
                    
                    if (!tokenValidation.IsValid)
                    {
                        _logger.LogWarning("Token {TokenId} failed validation: {Reason}", 
                            tokenId, tokenValidation.ErrorMessage);
                        return BadRequest($"Token validation failed: {tokenValidation.ErrorMessage}");
                    }

                    if (!tokenValidation.HasSufficientBalance)
                    {
                        _logger.LogWarning("User {UserId} has insufficient balance for token {TokenId}", 
                            userId, tokenId);
                        return BadRequest("Insufficient token balance for listing");
                    }
                }
                else
                {
                    return BadRequest("Invalid token ID format for platform token");
                }
            }
            else
            {
                // For external crypto assets, verify ownership through external wallet integration
                // This would typically involve checking wallet balances through external APIs
                _logger.LogInformation("External crypto asset listing for {AssetIdentifier} - ownership verification required", 
                    request.AssetIdentifier);
                // TODO: Implement external crypto asset ownership verification
            }

            // Create the listing request
            var listingRequest = new CreateListingRequest
            {
                TokenId = request.AssetType == AssetType.PlatformToken && Guid.TryParse(request.AssetIdentifier, out var tid) ? tid : null,
                AssetSymbol = request.AssetType != AssetType.PlatformToken ? request.AssetIdentifier : null,
                AssetType = request.AssetType,
                MarketType = MarketType.Secondary,
                PricingStrategy = request.PricingStrategy,
                Title = request.Title,
                Description = request.Description,
                TotalQuantity = request.TotalQuantity,
                MinimumPurchaseQuantity = request.MinimumPurchaseQuantity ?? 1,
                MaximumPurchaseQuantity = request.MaximumPurchaseQuantity,
                BasePrice = request.BasePrice,
                Currency = request.Currency,
                PricingConfiguration = request.PricingConfiguration ?? "{}",
                Tags = request.Tags != null ? string.Join(",", request.Tags) : null,
                ExpiresAt = request.ExpiresAt,
                ContactInfo = request.ContactInfo
            };

            var createdListing = await _listingService.CreateListingAsync(listingRequest, userId, cancellationToken);

            _logger.LogInformation("Secondary market listing {ListingId} created successfully for asset {AssetIdentifier}", 
                createdListing.Id, request.AssetIdentifier);

            return Ok(createdListing.Adapt<SecondaryMarketListingResponse>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create secondary market listing for asset {AssetIdentifier}", request.AssetIdentifier);
            throw;
        }
    }

    /// <summary>
    /// Get all secondary market listings with filtering and pagination
    /// </summary>
    [HttpGet("listings")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<SecondaryMarketListingResponse>>> GetListingsAsync(
        [FromQuery] GetSecondaryMarketListingsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving secondary market listings with filters: {Filters}", request);

            // TODO: Implement proper listing filtering in the service layer
            // For now, return empty response
            var response = new PagedResponse<SecondaryMarketListingResponse>
            {
                Data = new List<SecondaryMarketListingResponse>(),
                TotalCount = 0,
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? 20,
                TotalPages = 0
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secondary market listings");
            throw;
        }
    }

    /// <summary>
    /// Get a specific secondary market listing by ID
    /// </summary>
    [HttpGet("listings/{listingId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<SecondaryMarketListingResponse>> GetListingAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving secondary market listing {ListingId}", listingId);

            var listing = await _listingService.GetListingAsync(listingId, null, cancellationToken);
            if (listing == null || listing.MarketType != MarketType.Secondary)
            {
                return NotFound($"Secondary market listing {listingId} not found");
            }

            // Increment view count
            await _listingService.IncrementViewCountAsync(listingId, cancellationToken);

            return Ok(listing.Adapt<SecondaryMarketListingResponse>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secondary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Update a secondary market listing (only by the seller)
    /// </summary>
    [HttpPut("listings/{listingId:guid}")]
    public async Task<ActionResult<SecondaryMarketListingResponse>> UpdateListingAsync(
        Guid listingId,
        [FromBody] UpdateSecondaryMarketListingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating secondary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            var listing = await _listingService.GetListingAsync(listingId, userId, cancellationToken);
            if (listing == null || listing.MarketType != MarketType.Secondary)
            {
                return NotFound($"Secondary market listing {listingId} not found");
            }

            if (listing.SellerId != userId)
            {
                return Forbid("Only the listing owner can update this listing");
            }

            // Create update request
            var updateRequest = new UpdateListingRequest
            {
                Title = request.Title,
                Description = request.Description,
                BasePrice = request.BasePrice,
                PricingConfiguration = request.PricingConfiguration,
                ExpiresAt = request.ExpiresAt,
                Tags = request.Tags != null ? string.Join(",", request.Tags) : null,
                ContactInfo = request.ContactInfo
            };

            var updatedListing = await _listingService.UpdateListingAsync(listingId, updateRequest, userId, cancellationToken);

            _logger.LogInformation("Secondary market listing {ListingId} updated successfully", listingId);

            return Ok(updatedListing.Adapt<SecondaryMarketListingResponse>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update secondary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Activate a secondary market listing (make it live)
    /// </summary>
    [HttpPost("listings/{listingId:guid}/activate")]
    public async Task<ActionResult> ActivateListingAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Activating secondary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            var result = await _listingService.ActivateListingAsync(listingId, userId, cancellationToken);

            _logger.LogInformation("Secondary market listing {ListingId} activated successfully", listingId);

            return Ok(new { Message = "Listing activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate secondary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Deactivate a secondary market listing
    /// </summary>
    [HttpPost("listings/{listingId:guid}/deactivate")]
    public async Task<ActionResult> DeactivateListingAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Deactivating secondary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            var result = await _listingService.PauseListingAsync(listingId, userId, "User requested deactivation", cancellationToken);

            _logger.LogInformation("Secondary market listing {ListingId} deactivated successfully", listingId);

            return Ok(new { Message = "Listing deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate secondary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Create a purchase order for a secondary market listing
    /// </summary>
    [HttpPost("listings/{listingId:guid}/orders")]
    public async Task<ActionResult<MarketplaceOrderResponse>> CreateOrderAsync(
        Guid listingId,
        [FromBody] CreateSecondaryMarketOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating order for secondary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            // Get the listing
            var listing = await _listingService.GetListingAsync(listingId, userId, cancellationToken);
            if (listing == null || listing.MarketType != MarketType.Secondary)
            {
                return NotFound($"Secondary market listing {listingId} not found");
            }

            if (listing.Status != ListingStatus.Active)
            {
                return BadRequest("Listing is not active");
            }

            // Prevent self-trading
            if (listing.SellerId == userId)
            {
                return BadRequest("Cannot purchase your own listing");
            }

            // Calculate pricing
            var pricingResult = await _pricingService.CalculatePriceAsync(
                listingId, request.Quantity, cancellationToken);
            
            if (!pricingResult.IsValid)
            {
                return BadRequest($"Pricing calculation failed: {pricingResult.ErrorMessage}");
            }

            // Create the order request
            var orderRequest = new CreateOrderRequest
            {
                ListingId = listingId,
                Quantity = request.Quantity,
                BuyerNotes = request.Metadata
            };

            var createdOrder = await _orderService.CreateOrderAsync(orderRequest, userId, cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully for listing {ListingId}", 
                createdOrder.Id, listingId);

            return Ok(createdOrder.Adapt<MarketplaceOrderResponse>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for secondary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Get current pricing for a secondary market listing
    /// </summary>
    [HttpGet("listings/{listingId:guid}/pricing")]
    [AllowAnonymous]
    public async Task<ActionResult<PricingResponse>> GetPricingAsync(
        Guid listingId,
        [FromQuery] decimal quantity = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting pricing for secondary market listing {ListingId}, quantity {Quantity}", 
                listingId, quantity);

            var pricingResult = await _pricingService.CalculatePriceAsync(listingId, quantity, cancellationToken);
            if (!pricingResult.IsValid)
            {
                return BadRequest($"Pricing calculation failed: {pricingResult.ErrorMessage}");
            }

            return Ok(pricingResult.Adapt<PricingResponse>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pricing for secondary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Get trading pairs available in the secondary market
    /// </summary>
    [HttpGet("trading-pairs")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TradingPairResponse>>> GetTradingPairsAsync(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving trading pairs, activeOnly: {ActiveOnly}", activeOnly);

            // TODO: Implement trading pairs service
            var tradingPairs = new List<TradingPairResponse>();

            return Ok(tradingPairs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve trading pairs");
            throw;
        }
    }

    /// <summary>
    /// Create a new trading pair (admin only)
    /// </summary>
    [HttpPost("trading-pairs")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TradingPairResponse>> CreateTradingPairAsync(
        [FromBody] CreateTradingPairRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating trading pair {BaseAsset}/{QuoteAsset} by admin {UserId}", 
                request.BaseAsset, request.QuoteAsset, userId);

            // TODO: Implement trading pair creation
            var tradingPair = new TradingPairResponse
            {
                Id = Guid.NewGuid(),
                BaseAsset = request.BaseAsset,
                QuoteAsset = request.QuoteAsset,
                Symbol = $"{request.BaseAsset}/{request.QuoteAsset}",
                MinTradeAmount = request.MinTradeAmount,
                MaxTradeAmount = request.MaxTradeAmount,
                TradingFeePercentage = request.TradingFeePercentage,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Trading pair {Symbol} created successfully", tradingPair.Symbol);

            return Ok(tradingPair);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create trading pair {BaseAsset}/{QuoteAsset}", 
                request.BaseAsset, request.QuoteAsset);
            throw;
        }
    }

    /// <summary>
    /// Get market analytics for secondary market assets
    /// </summary>
    [HttpGet("analytics")]
    [AllowAnonymous]
    public async Task<ActionResult<SecondaryMarketAnalyticsResponse>> GetAnalyticsAsync(
        [FromQuery] string? assetIdentifier = null,
        [FromQuery] AssetType? assetType = null,
        [FromQuery] string? tradingPair = null,
        [FromQuery] string timePeriod = "24h",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting secondary market analytics for asset {AssetIdentifier}, period {TimePeriod}", 
                assetIdentifier, timePeriod);

            // TODO: Implement analytics service
            var analytics = new SecondaryMarketAnalyticsResponse
            {
                AssetIdentifier = assetIdentifier ?? "ALL",
                AssetType = assetType ?? AssetType.PlatformToken,
                TradingPair = tradingPair,
                CurrentPrice = 0,
                Volume24h = 0,
                ActiveListings = 0,
                TotalOrders24h = 0,
                TotalVolume24h = 0,
                AverageOrderSize = 0,
                LastUpdated = DateTime.UtcNow
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get secondary market analytics");
            throw;
        }
    }

    /// <summary>
    /// Get market depth (order book) for a specific asset or trading pair
    /// </summary>
    [HttpGet("market-depth")]
    [AllowAnonymous]
    public async Task<ActionResult<MarketDepthResponse>> GetMarketDepthAsync(
        [FromQuery] string assetIdentifier,
        [FromQuery] string? tradingPair = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting market depth for asset {AssetIdentifier}, trading pair {TradingPair}", 
                assetIdentifier, tradingPair);

            // TODO: Implement market depth calculation
            var marketDepth = new MarketDepthResponse
            {
                AssetIdentifier = assetIdentifier,
                TradingPair = tradingPair,
                Bids = new List<Models.Responses.OrderBookEntry>(),
                Asks = new List<Models.Responses.OrderBookEntry>(),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(marketDepth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get market depth for asset {AssetIdentifier}", assetIdentifier);
            throw;
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}
