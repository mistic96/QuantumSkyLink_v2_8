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
/// Primary Market Controller - Handles new token sales directly from issuers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrimaryMarketController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly IOrderService _orderService;
    private readonly IPricingService _pricingService;
    private readonly IMarketplaceIntegrationService _integrationService;
    private readonly ILogger<PrimaryMarketController> _logger;

    public PrimaryMarketController(
        IListingService listingService,
        IOrderService orderService,
        IPricingService pricingService,
        IMarketplaceIntegrationService integrationService,
        ILogger<PrimaryMarketController> logger)
    {
        _listingService = listingService;
        _orderService = orderService;
        _pricingService = pricingService;
        _integrationService = integrationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new primary market listing for token issuers
    /// </summary>
    [HttpPost("listings")]
    public async Task<ActionResult<PrimaryMarketListingResponse>> CreateListingAsync(
        [FromBody] CreatePrimaryMarketListingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating primary market listing for user {UserId}, token {TokenId}", 
                userId, request.TokenId);

            // Verify user marketplace permissions (seller role / permission check)
            var permissions = await _integrationService.GetMarketplacePermissionsAsync(userId, cancellationToken);
            if (permissions == null || !permissions.CanCreateListings || !permissions.CanSellTokens)
            {
                _logger.LogWarning("User {UserId} does not have marketplace seller permissions", userId);
                return Forbid("User is not authorized to create marketplace listings");
            }

            // If TokenId provided, verify token lifecycle (ListingReady) before ownership validation
            if (request.TokenId != Guid.Empty)
            {
                var lifecycle = await _integrationService.GetTokenLifecycleStatusAsync(request.TokenId, cancellationToken);
                if (lifecycle == null || !lifecycle.CanBeListedInMarketplace)
                {
                    _logger.LogWarning("Token {TokenId} cannot be listed: {Reason}", request.TokenId, lifecycle?.BlockingReason);
                    return Conflict(new { error = "TokenNotListingReady", message = lifecycle?.BlockingReason ?? "Token cannot be listed in marketplace" });
                }

                // Verify token ownership with TokenService
                var tokenValidation = await _integrationService.ValidateTokenOwnershipAsync(
                    userId, request.TokenId, request.TotalQuantity, cancellationToken);
                
                if (!tokenValidation.IsValid)
                {
                    _logger.LogWarning("Token {TokenId} failed validation: {Reason}", 
                        request.TokenId, tokenValidation.ErrorMessage);
                    return BadRequest($"Token validation failed: {tokenValidation.ErrorMessage}");
                }

                // Verify user has sufficient token balance
                if (!tokenValidation.HasSufficientBalance)
                {
                    _logger.LogWarning("User {UserId} has insufficient balance for token {TokenId}", 
                        userId, request.TokenId);
                    return BadRequest("Insufficient token balance for listing");
                }
            }
            else
            {
                // No TokenId - currently primary market requires TokenId. Guard in case of future crypto symbol support.
                _logger.LogWarning("CreateListing called without TokenId - primary market requires TokenId");
                return BadRequest("TokenId is required for primary market listings");
            }

            // Create the listing request
            var listingRequest = new CreateListingRequest
            {
                TokenId = request.TokenId,
                AssetType = AssetType.PlatformToken,
                MarketType = MarketType.Primary,
                PricingStrategy = request.PricingStrategy,
                Title = request.Title,
                Description = request.Description,
                TotalQuantity = request.TotalQuantity,
                MinimumPurchaseQuantity = request.MinimumPurchaseQuantity ?? 1,
                MaximumPurchaseQuantity = request.MaximumPurchaseQuantity,
                BasePrice = request.BasePrice,
                Currency = request.Currency ?? "USD",
                PricingConfiguration = request.PricingConfiguration ?? "{}",
                Tags = request.Tags,
                ExpiresAt = request.ExpiresAt,
                ContactInfo = request.ContactInfo,
                DocumentationUrl = request.DocumentationUrl,
                RoadmapUrl = request.RoadmapUrl,
                WhitepaperUrl = request.WhitepaperUrl,
                WebsiteUrl = request.WebsiteUrl,
                SocialLinks = request.SocialLinks
            };

            var createdListing = await _listingService.CreateListingAsync(listingRequest, userId, cancellationToken);

            _logger.LogInformation("Primary market listing {ListingId} created successfully for token {TokenId}", 
                createdListing.Id, request.TokenId);

            var resp = createdListing.Adapt<PrimaryMarketListingResponse>();
            // Created by current user, mark as own listing
            resp.IsOwnListing = true;
            return Ok(resp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create primary market listing for token {TokenId}", request.TokenId);
            throw;
        }
    }

    /// <summary>
    /// Get all primary market listings with filtering and pagination
    /// </summary>
    [HttpGet("listings")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<PrimaryMarketListingResponse>>> GetListingsAsync(
        [FromQuery] GetPrimaryMarketListingsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving primary market listings with filters: {Filters}", request);

            // For now, return empty response - this needs proper implementation
            // TODO: Implement proper listing filtering in the service layer
            var response = new PagedResponse<PrimaryMarketListingResponse>
            {
                Data = new List<PrimaryMarketListingResponse>(),
                TotalCount = 0,
                Page = request.Page ?? 1,
                PageSize = request.PageSize ?? 20,
                TotalPages = 0
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve primary market listings");
            throw;
        }
    }

    /// <summary>
    /// Get a specific primary market listing by ID
    /// </summary>
    [HttpGet("listings/{listingId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PrimaryMarketListingResponse>> GetListingAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving primary market listing {ListingId}", listingId);

            var listing = await _listingService.GetListingAsync(listingId, null, cancellationToken);
            if (listing == null || listing.MarketType != MarketType.Primary)
            {
                return NotFound($"Primary market listing {listingId} not found");
            }

            // Increment view count
            await _listingService.IncrementViewCountAsync(listingId, cancellationToken);

            var currentUser = GetCurrentUserIdOrNull();
            var resp = listing.Adapt<PrimaryMarketListingResponse>();
            resp.IsOwnListing = currentUser.HasValue && listing.SellerId == currentUser.Value;
            return Ok(resp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve primary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Update a primary market listing (only by the issuer)
    /// </summary>
    [HttpPut("listings/{listingId:guid}")]
    public async Task<ActionResult<PrimaryMarketListingResponse>> UpdateListingAsync(
        Guid listingId,
        [FromBody] UpdatePrimaryMarketListingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating primary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            var listing = await _listingService.GetListingAsync(listingId, userId, cancellationToken);
            if (listing == null || listing.MarketType != MarketType.Primary)
            {
                return NotFound($"Primary market listing {listingId} not found");
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
                Tags = request.Tags,
                ContactInfo = request.ContactInfo,
                DocumentationUrl = request.DocumentationUrl,
                RoadmapUrl = request.RoadmapUrl,
                WhitepaperUrl = request.WhitepaperUrl,
                WebsiteUrl = request.WebsiteUrl,
                SocialLinks = request.SocialLinks
            };

            var updatedListing = await _listingService.UpdateListingAsync(listingId, updateRequest, userId, cancellationToken);

            _logger.LogInformation("Primary market listing {ListingId} updated successfully", listingId);

            return Ok(updatedListing.Adapt<PrimaryMarketListingResponse>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update primary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Activate a primary market listing (make it live)
    /// </summary>
    [HttpPost("listings/{listingId:guid}/activate")]
    public async Task<ActionResult> ActivateListingAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Activating primary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            var result = await _listingService.ActivateListingAsync(listingId, userId, cancellationToken);
            // ActivateListingAsync returns MarketListingDetailDto, not a result object
            // For now, assume success if no exception is thrown

            _logger.LogInformation("Primary market listing {ListingId} activated successfully", listingId);

            return Ok(new { Message = "Listing activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate primary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Deactivate a primary market listing
    /// </summary>
    [HttpPost("listings/{listingId:guid}/deactivate")]
    public async Task<ActionResult> DeactivateListingAsync(
        Guid listingId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Deactivating primary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            // Use PauseListingAsync instead of DeactivateListingAsync
            var result = await _listingService.PauseListingAsync(listingId, userId, "User requested deactivation", cancellationToken);
            // PauseListingAsync returns MarketListingDetailDto, not a result object
            // For now, assume success if no exception is thrown

            _logger.LogInformation("Primary market listing {ListingId} deactivated successfully", listingId);

            return Ok(new { Message = "Listing deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate primary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Create a purchase order for a primary market listing
    /// </summary>
    [HttpPost("listings/{listingId:guid}/orders")]
    public async Task<ActionResult<MarketplaceOrderResponse>> CreateOrderAsync(
        Guid listingId,
        [FromBody] CreatePrimaryMarketOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating order for primary market listing {ListingId} by user {UserId}", 
                listingId, userId);

            // Get the listing
            var listing = await _listingService.GetListingAsync(listingId, userId, cancellationToken);
            if (listing == null || listing.MarketType != MarketType.Primary)
            {
                return NotFound($"Primary market listing {listingId} not found");
            }

            if (listing.Status != ListingStatus.Active)
            {
                return BadRequest("Listing is not active");
            }

            // Prevent self-purchase by the seller
            if (listing.SellerId == userId)
            {
                _logger.LogWarning("User {UserId} attempted to purchase own listing {ListingId}", userId, listingId);
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
            _logger.LogError(ex, "Failed to create order for primary market listing {ListingId}", listingId);
            throw;
        }
    }

    /// <summary>
    /// Get current pricing for a primary market listing
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
            _logger.LogInformation("Getting pricing for primary market listing {ListingId}, quantity {Quantity}", 
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
            _logger.LogError(ex, "Failed to get pricing for primary market listing {ListingId}", listingId);
            throw;
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }

    /// <summary>
    /// Returns the current authenticated user ID, or null if the request is anonymous or the claim is invalid.
    /// </summary>
    private Guid? GetCurrentUserIdOrNull()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}
