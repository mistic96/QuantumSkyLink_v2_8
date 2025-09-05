using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services;

public class MarketplaceService : IMarketplaceService
{
    private readonly MarketplaceDbContext _context;
    private readonly ILogger<MarketplaceService> _logger;

    public MarketplaceService(
        MarketplaceDbContext context,
        ILogger<MarketplaceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MarketplaceOverviewDto> GetMarketplaceOverviewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace overview");

        var now = DateTime.UtcNow;
        var last24Hours = now.AddDays(-1);

        var totalActiveListings = await _context.MarketListings
            .CountAsync(l => l.Status == ListingStatus.Active, cancellationToken);

        var primaryMarketListings = await _context.MarketListings
            .CountAsync(l => l.MarketType == MarketType.Primary && l.Status == ListingStatus.Active, cancellationToken);

        var secondaryMarketListings = await _context.MarketListings
            .CountAsync(l => l.MarketType == MarketType.Secondary && l.Status == ListingStatus.Active, cancellationToken);

        var orders24h = await _context.MarketplaceOrders
            .CountAsync(o => o.CreatedAt >= last24Hours, cancellationToken);

        var volume24h = await _context.MarketplaceOrders
            .Where(o => o.CreatedAt >= last24Hours && o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var totalVolumeTraded = await _context.MarketplaceOrders
            .Where(o => o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        // Get unique users from orders and listings
        var orderUsers = await _context.MarketplaceOrders
            .Select(o => new { o.BuyerId, o.SellerId })
            .ToListAsync(cancellationToken);

        var listingUsers = await _context.MarketListings
            .Select(l => l.SellerId)
            .ToListAsync(cancellationToken);

        var allUserIds = orderUsers.SelectMany(u => new[] { u.BuyerId, u.SellerId })
            .Union(listingUsers)
            .Distinct()
            .Count();

        return new MarketplaceOverviewDto
        {
            TotalActiveListings = totalActiveListings,
            TotalUsers = allUserIds,
            TotalVolumeTraded = totalVolumeTraded,
            Volume24h = volume24h,
            Orders24h = orders24h,
            PrimaryMarketListings = primaryMarketListings,
            SecondaryMarketListings = secondaryMarketListings
        };
    }

    public async Task<IEnumerable<MarketListingDto>> GetFeaturedListingsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting featured listings with limit {Limit}", limit);

        var listings = await _context.MarketListings
            .Where(l => l.Status == ListingStatus.Active && l.IsFeatured)
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return listings.Select(MapToMarketListingDto);
    }

    public async Task<PaginatedListingResponse> SearchListingsAsync(SearchListingsRequest searchRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching listings with term '{SearchTerm}'", searchRequest.SearchTerm);

        var query = _context.MarketListings
            .Where(l => l.Status == ListingStatus.Active);

        // Apply filters
        if (!string.IsNullOrEmpty(searchRequest.SearchTerm))
        {
            query = query.Where(l => l.Title.Contains(searchRequest.SearchTerm) || 
                                   (l.Description != null && l.Description.Contains(searchRequest.SearchTerm)));
        }

        if (searchRequest.MarketType.HasValue)
        {
            query = query.Where(l => l.MarketType == searchRequest.MarketType.Value);
        }

        if (searchRequest.AssetType.HasValue)
        {
            query = query.Where(l => l.AssetType == searchRequest.AssetType.Value);
        }

        if (searchRequest.MinPrice.HasValue && searchRequest.MaxPrice.HasValue)
        {
            query = query.Where(l => l.BasePrice >= searchRequest.MinPrice.Value && 
                                   l.BasePrice <= searchRequest.MaxPrice.Value);
        }
        else if (searchRequest.MinPrice.HasValue)
        {
            query = query.Where(l => l.BasePrice >= searchRequest.MinPrice.Value);
        }
        else if (searchRequest.MaxPrice.HasValue)
        {
            query = query.Where(l => l.BasePrice <= searchRequest.MaxPrice.Value);
        }

        if (searchRequest.PricingStrategy.HasValue)
        {
            query = query.Where(l => l.PricingStrategy == searchRequest.PricingStrategy.Value);
        }

        if (searchRequest.IsFeatured.HasValue)
        {
            query = query.Where(l => l.IsFeatured == searchRequest.IsFeatured.Value);
        }

        if (searchRequest.IsVerified.HasValue)
        {
            query = query.Where(l => l.IsVerified == searchRequest.IsVerified.Value);
        }

        if (!string.IsNullOrEmpty(searchRequest.Tags))
        {
            query = query.Where(l => l.Tags != null && l.Tags.Contains(searchRequest.Tags));
        }

        // Apply sorting
        query = searchRequest.SortBy?.ToLower() switch
        {
            "price" => searchRequest.SortDirection?.ToUpper() == "ASC" 
                ? query.OrderBy(l => l.BasePrice) 
                : query.OrderByDescending(l => l.BasePrice),
            "title" => searchRequest.SortDirection?.ToUpper() == "ASC" 
                ? query.OrderBy(l => l.Title) 
                : query.OrderByDescending(l => l.Title),
            "views" => searchRequest.SortDirection?.ToUpper() == "ASC" 
                ? query.OrderBy(l => l.ViewCount) 
                : query.OrderByDescending(l => l.ViewCount),
            _ => searchRequest.SortDirection?.ToUpper() == "ASC" 
                ? query.OrderBy(l => l.CreatedAt) 
                : query.OrderByDescending(l => l.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize);

        var listings = await query
            .Skip((searchRequest.Page - 1) * searchRequest.PageSize)
            .Take(searchRequest.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedListingResponse
        {
            Listings = listings.Select(MapToMarketListingDto),
            TotalCount = totalCount,
            Page = searchRequest.Page,
            PageSize = searchRequest.PageSize,
            TotalPages = totalPages,
            HasNextPage = searchRequest.Page < totalPages,
            HasPreviousPage = searchRequest.Page > 1
        };
    }

    public async Task<IEnumerable<MarketplaceCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace categories");

        var categories = await _context.MarketListings
            .Where(l => l.Status == ListingStatus.Active)
            .GroupBy(l => l.AssetType)
            .Select(g => new MarketplaceCategoryDto
            {
                Name = g.Key.ToString(),
                Description = GetAssetTypeDescription(g.Key),
                ListingCount = g.Count(),
                AssetType = g.Key
            })
            .ToListAsync(cancellationToken);

        return categories;
    }

    public async Task<IEnumerable<MarketListingDto>> GetTrendingListingsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting trending listings with limit {Limit}", limit);

        var last7Days = DateTime.UtcNow.AddDays(-7);

        var listings = await _context.MarketListings
            .Include(l => l.Orders)
            .Where(l => l.Status == ListingStatus.Active)
            .OrderByDescending(l => l.Orders.Count(o => o.CreatedAt >= last7Days))
            .ThenByDescending(l => l.ViewCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return listings.Select(MapToMarketListingDto);
    }

    public async Task<IEnumerable<MarketListingDto>> GetRecentListingsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recent listings with limit {Limit}", limit);

        var listings = await _context.MarketListings
            .Where(l => l.Status == ListingStatus.Active)
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return listings.Select(MapToMarketListingDto);
    }

    public async Task<MarketplaceHealthDto> GetMarketplaceHealthAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting marketplace health status");

        var activeListings = await _context.MarketListings
            .CountAsync(l => l.Status == ListingStatus.Active, cancellationToken);

        var pendingOrders = await _context.MarketplaceOrders
            .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

        var activeEscrows = await _context.EscrowAccounts
            .CountAsync(e => e.Status == EscrowStatus.Funded, cancellationToken);

        var issues = new List<string>();

        // Check for potential issues
        if (activeListings == 0)
        {
            issues.Add("No active listings available");
        }

        if (pendingOrders > activeListings * 0.2m)
        {
            issues.Add("High number of pending orders");
        }

        var isHealthy = issues.Count == 0;

        return new MarketplaceHealthDto
        {
            IsHealthy = isHealthy,
            ActiveListings = activeListings,
            PendingOrders = pendingOrders,
            ActiveEscrows = activeEscrows,
            LastUpdated = DateTime.UtcNow,
            Issues = issues.Any() ? string.Join("; ", issues) : null
        };
    }

    // Helper Methods
    private static MarketListingDto MapToMarketListingDto(MarketListing listing)
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

    private static string GetAssetTypeDescription(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.PlatformToken => "Platform native tokens",
            AssetType.Bitcoin => "Bitcoin and Bitcoin-based assets",
            AssetType.Ethereum => "Ethereum and ERC-20 tokens",
            AssetType.Solana => "Solana and SPL tokens",
            AssetType.OtherCrypto => "Other cryptocurrency assets",
            _ => "Unknown asset type"
        };
    }
}
