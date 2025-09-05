using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;
using MarketplaceService.Models.Shared;

namespace MarketplaceService.Services;

public class ListingService : IListingService
{
    private readonly MarketplaceDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ListingService> _logger;
    private readonly IPricingService _pricingService;
    private readonly IMarketplaceIntegrationService _integrationService;

    public ListingService(
        MarketplaceDbContext context,
        IDistributedCache cache,
        ILogger<ListingService> logger,
        IPricingService pricingService,
        IMarketplaceIntegrationService integrationService)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _pricingService = pricingService;
        _integrationService = integrationService;
    }

    public async Task<MarketListingDetailDto> CreateListingAsync(CreateListingRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new listing for seller {SellerId}", userId);

        var listing = new MarketListing
        {
            Id = Guid.NewGuid(),
            SellerId = userId,
            TokenId = request.TokenId,
            AssetSymbol = request.AssetSymbol,
            AssetType = request.AssetType,
            MarketType = request.MarketType,
            PricingStrategy = request.PricingStrategy,
            Title = request.Title,
            Description = request.Description,
            TotalQuantity = request.TotalQuantity,
            RemainingQuantity = request.TotalQuantity,
            MinimumPurchaseQuantity = request.MinimumPurchaseQuantity,
            MaximumPurchaseQuantity = request.MaximumPurchaseQuantity,
            BasePrice = request.BasePrice,
            Currency = request.Currency,
            PricingConfiguration = request.PricingConfiguration,
            Tags = request.Tags,
            ExpiresAt = request.ExpiresAt,
            ContactInfo = request.ContactInfo,
            DocumentationUrl = request.DocumentationUrl,
            RoadmapUrl = request.RoadmapUrl,
            WhitepaperUrl = request.WhitepaperUrl,
            WebsiteUrl = request.WebsiteUrl,
            SocialLinks = request.SocialLinks,
            Status = ListingStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MarketListings.Add(listing);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created listing {ListingId} for seller {SellerId}", listing.Id, userId);

        return MapToDetailDto(listing);
    }

    public async Task<MarketListingDetailDto?> GetListingAsync(Guid listingId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            return null;
        }

        // Increment view count if not the owner
        if (userId.HasValue && userId.Value != listing.SellerId)
        {
            await IncrementViewCountAsync(listingId, cancellationToken);
        }

        return MapToDetailDto(listing);
    }

    public async Task<MarketListingDetailDto> UpdateListingAsync(Guid listingId, UpdateListingRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating listing {ListingId}", listingId);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerId == userId, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {listingId} not found or access denied");
        }

        if (listing.Status == ListingStatus.SoldOut || listing.Status == ListingStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot update listing in {listing.Status} status");
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.Title))
            listing.Title = request.Title;
        
        if (!string.IsNullOrEmpty(request.Description))
            listing.Description = request.Description;
        
        if (request.BasePrice.HasValue)
            listing.BasePrice = request.BasePrice;

        if (!string.IsNullOrEmpty(request.PricingConfiguration))
            listing.PricingConfiguration = request.PricingConfiguration;

        if (!string.IsNullOrEmpty(request.Tags))
            listing.Tags = request.Tags;

        if (request.ExpiresAt.HasValue)
            listing.ExpiresAt = request.ExpiresAt;

        if (!string.IsNullOrEmpty(request.ContactInfo))
            listing.ContactInfo = request.ContactInfo;

        if (!string.IsNullOrEmpty(request.DocumentationUrl))
            listing.DocumentationUrl = request.DocumentationUrl;

        if (!string.IsNullOrEmpty(request.RoadmapUrl))
            listing.RoadmapUrl = request.RoadmapUrl;

        if (!string.IsNullOrEmpty(request.WhitepaperUrl))
            listing.WhitepaperUrl = request.WhitepaperUrl;

        if (!string.IsNullOrEmpty(request.WebsiteUrl))
            listing.WebsiteUrl = request.WebsiteUrl;

        if (!string.IsNullOrEmpty(request.SocialLinks))
            listing.SocialLinks = request.SocialLinks;

        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated listing {ListingId}", listingId);

        return MapToDetailDto(listing);
    }

    public async Task<bool> DeleteListingAsync(Guid listingId, Guid userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting listing {ListingId} with reason: {Reason}", listingId, reason);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerId == userId, cancellationToken);

        if (listing == null)
        {
            return false;
        }

        // Check if listing has active orders
        var hasActiveOrders = await _context.MarketplaceOrders
            .AnyAsync(o => o.ListingId == listingId && 
                          o.Status != OrderStatus.Cancelled && 
                          o.Status != OrderStatus.Completed, cancellationToken);

        if (hasActiveOrders)
        {
            throw new InvalidOperationException("Cannot delete listing with active orders");
        }

        listing.Status = ListingStatus.Cancelled;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted listing {ListingId}", listingId);

        return true;
    }

    public async Task<PaginatedListingResponse> GetUserListingsAsync(Guid userId, ListingStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.MarketListings
            .Where(l => l.SellerId == userId);

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginatedListingResponse
        {
            Listings = listings.Select(MapToListingDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<MarketListingDetailDto> ActivateListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating listing {ListingId}", listingId);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerId == userId, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {listingId} not found or access denied");
        }

        if (listing.Status != ListingStatus.Draft)
        {
            throw new InvalidOperationException($"Can only activate draft listings. Current status: {listing.Status}");
        }

        // Verify token lifecycle status with TokenService integration before activating
        if (listing.TokenId.HasValue)
        {
            var lifecycle = await _integrationService.GetTokenLifecycleStatusAsync(listing.TokenId.Value, cancellationToken);
            if (!lifecycle.CanBeListedInMarketplace)
            {
                var reason = string.IsNullOrEmpty(lifecycle.BlockingReason) ? "Token lifecycle incomplete" : lifecycle.BlockingReason;
                throw new InvalidOperationException($"Token {listing.TokenId} cannot be listed: {reason}");
            }
        }

        listing.Status = ListingStatus.Active;
        listing.ActivatedAt = DateTime.UtcNow;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated listing {ListingId}", listingId);

        return MapToDetailDto(listing);
    }

    public async Task<MarketListingDetailDto> PauseListingAsync(Guid listingId, Guid userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pausing listing {ListingId} with reason: {Reason}", listingId, reason);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerId == userId, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {listingId} not found or access denied");
        }

        if (listing.Status != ListingStatus.Active)
        {
            throw new InvalidOperationException($"Can only pause active listings. Current status: {listing.Status}");
        }

        listing.Status = ListingStatus.Paused;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Paused listing {ListingId}", listingId);

        return MapToDetailDto(listing);
    }

    public async Task<MarketListingDetailDto> ResumeListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resuming listing {ListingId}", listingId);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.SellerId == userId, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {listingId} not found or access denied");
        }

        if (listing.Status != ListingStatus.Paused)
        {
            throw new InvalidOperationException($"Can only resume paused listings. Current status: {listing.Status}");
        }

        listing.Status = ListingStatus.Active;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Resumed listing {ListingId}", listingId);

        return MapToDetailDto(listing);
    }

    public async Task<MarketListingDetailDto> UpdateListingQuantityAsync(Guid listingId, decimal quantitySold, CancellationToken cancellationToken = default)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {listingId} not found");
        }

        if (listing.RemainingQuantity < quantitySold)
        {
            throw new InvalidOperationException("Insufficient quantity available");
        }

        listing.RemainingQuantity -= quantitySold;
        listing.VolumeSold += quantitySold;
        listing.UpdatedAt = DateTime.UtcNow;

        // Update status if sold out
        if (listing.RemainingQuantity == 0)
        {
            listing.Status = ListingStatus.SoldOut;
            listing.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDetailDto(listing);
    }

    public async Task<IEnumerable<PriceHistoryDto>> GetListingPriceHistoryAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {listingId} not found");
        }

        // For now, return a single price history entry based on the current listing
        // In a full implementation, you would have a separate PriceHistory table
        return new List<PriceHistoryDto>
        {
            new PriceHistoryDto
            {
                Id = Guid.NewGuid(),
                PricingStrategy = listing.PricingStrategy,
                PricePerToken = listing.BasePrice ?? 0,
                Currency = listing.Currency,
                ChangeReason = "Initial listing price",
                IsAutomaticUpdate = false,
                CreatedAt = listing.CreatedAt
            }
        };
    }

    public async Task<bool> IncrementViewCountAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId, cancellationToken);

        if (listing == null)
        {
            return false;
        }

        listing.ViewCount++;
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static MarketListingDetailDto MapToDetailDto(MarketListing listing)
    {
        return new MarketListingDetailDto
        {
            Id = listing.Id,
            SellerId = listing.SellerId,
            TokenId = listing.TokenId,
            AssetSymbol = listing.AssetSymbol,
            AssetType = listing.AssetType,
            MarketType = listing.MarketType,
            Status = listing.Status,
            PricingStrategy = listing.PricingStrategy,
            Title = listing.Title,
            Description = listing.Description,
            TotalQuantity = listing.TotalQuantity,
            RemainingQuantity = listing.RemainingQuantity,
            MinimumPurchaseQuantity = listing.MinimumPurchaseQuantity,
            MaximumPurchaseQuantity = listing.MaximumPurchaseQuantity,
            BasePrice = listing.BasePrice,
            Currency = listing.Currency,
            PricingConfiguration = listing.PricingConfiguration ?? "{}",
            ListingFee = listing.ListingFee,
            CommissionPercentage = listing.CommissionPercentage,
            IsFeatured = listing.IsFeatured,
            IsVerified = listing.IsVerified,
            ExpiresAt = listing.ExpiresAt,
            ActivatedAt = listing.ActivatedAt,
            CompletedAt = listing.CompletedAt,
            ViewCount = listing.ViewCount,
            OrderCount = listing.OrderCount,
            VolumeSold = listing.VolumeSold,
            TotalRevenue = listing.TotalRevenue,
            Tags = listing.Tags,
            ContactInfo = listing.ContactInfo,
            DocumentationUrl = listing.DocumentationUrl,
            RoadmapUrl = listing.RoadmapUrl,
            WhitepaperUrl = listing.WhitepaperUrl,
            WebsiteUrl = listing.WebsiteUrl,
            SocialLinks = listing.SocialLinks,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt
        };
    }

    private static MarketListingDto MapToListingDto(MarketListing listing)
    {
        return new MarketListingDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            MarketType = listing.MarketType,
            AssetType = listing.AssetType,
            AssetSymbol = listing.AssetSymbol,
            TotalQuantity = listing.TotalQuantity,
            RemainingQuantity = listing.RemainingQuantity,
            BasePrice = listing.BasePrice,
            Currency = listing.Currency,
            PricingStrategy = listing.PricingStrategy,
            IsFeatured = listing.IsFeatured,
            IsVerified = listing.IsVerified,
            ViewCount = listing.ViewCount,
            OrderCount = listing.OrderCount,
            CreatedAt = listing.CreatedAt,
            Tags = listing.Tags
        };
    }
}
