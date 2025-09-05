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
/// Service for managing liquidity providers
/// Follows the PaymentGatewayService pattern - returns response models directly and throws exceptions for errors
/// </summary>
public class LiquidityProviderService : ILiquidityProviderService
{
    private readonly LiquidationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<LiquidityProviderService> _logger;

    public LiquidityProviderService(
        LiquidationDbContext context,
        IDistributedCache cache,
        ILogger<LiquidityProviderService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Register a new liquidity provider
    /// </summary>
    public async Task<LiquidityProviderResponse> RegisterLiquidityProviderAsync(
        Guid userId, 
        RegisterLiquidityProviderModel request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering liquidity provider for user {UserId}", userId);

        try
        {
            // Check if user already has a liquidity provider
            var existingProvider = await _context.LiquidityProviders
                .FirstOrDefaultAsync(lp => lp.UserId == userId, cancellationToken);

            if (existingProvider != null)
                throw new InvalidOperationException($"User {userId} already has a registered liquidity provider");

            // Create new liquidity provider
            var provider = new LiquidityProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Country = request.Country,
                Status = LiquidityProviderStatus.Pending,
                MinimumTransactionAmount = request.MinimumTransactionAmount,
                MaximumTransactionAmount = request.MaximumTransactionAmount,
                SupportedAssets = request.SupportedAssets,
                SupportedOutputCurrencies = request.SupportedOutputCurrencies,
                FeePercentage = request.FeePercentage,
                LiquidityPoolAddress = request.LiquidityPoolAddress,
                OperatingHours = request.OperatingHours,
                TimeZone = request.TimeZone,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.LiquidityProviders.Add(provider);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidity provider {ProviderId} registered successfully for user {UserId}", 
                provider.Id, userId);

            return provider.Adapt<LiquidityProviderResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering liquidity provider for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get liquidity provider by ID
    /// </summary>
    public async Task<LiquidityProviderResponse?> GetLiquidityProviderAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting liquidity provider {ProviderId}", providerId);

        try
        {
            var provider = await _context.LiquidityProviders
                .FirstOrDefaultAsync(lp => lp.Id == providerId, cancellationToken);

            if (provider == null)
            {
                _logger.LogWarning("Liquidity provider {ProviderId} not found", providerId);
                return null;
            }

            return provider.Adapt<LiquidityProviderResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting liquidity provider {ProviderId}", providerId);
            throw;
        }
    }

    /// <summary>
    /// Get all liquidity providers with filtering
    /// </summary>
    public async Task<PaginatedResponse<LiquidityProviderResponse>> GetLiquidityProvidersAsync(
        string? status = null,
        string? country = null,
        string? assetSymbol = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting liquidity providers with filters - Status: {Status}, Country: {Country}, Asset: {AssetSymbol}, Page: {Page}",
            status, country, assetSymbol, page);

        try
        {
            var query = _context.LiquidityProviders.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<LiquidityProviderStatus>(status, out var statusEnum))
                query = query.Where(lp => lp.Status == statusEnum);

            if (!string.IsNullOrEmpty(country))
                query = query.Where(lp => lp.Country == country);

            if (!string.IsNullOrEmpty(assetSymbol))
                query = query.Where(lp => lp.SupportedAssets != null && lp.SupportedAssets.Contains(assetSymbol));

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var providers = await query
                .OrderByDescending(lp => lp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var providerResponses = providers.Adapt<List<LiquidityProviderResponse>>();

            return new PaginatedResponse<LiquidityProviderResponse>
            {
                Items = providerResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = page < (int)Math.Ceiling((double)totalCount / pageSize),
                HasPreviousPage = page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting liquidity providers");
            throw;
        }
    }

    /// <summary>
    /// Update liquidity provider information
    /// </summary>
    public async Task<LiquidityProviderResponse> UpdateLiquidityProviderAsync(
        Guid providerId, 
        UpdateLiquidityProviderModel request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating liquidity provider {ProviderId}", providerId);

        try
        {
            var provider = await _context.LiquidityProviders
                .FirstOrDefaultAsync(lp => lp.Id == providerId, cancellationToken);

            if (provider == null)
                throw new InvalidOperationException($"Liquidity provider {providerId} not found");

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name))
                provider.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Email))
                provider.Email = request.Email;

            if (request.PhoneNumber != null)
                provider.PhoneNumber = request.PhoneNumber;

            if (request.Address != null)
                provider.Address = request.Address;

            if (request.MinimumTransactionAmount.HasValue)
                provider.MinimumTransactionAmount = request.MinimumTransactionAmount;

            if (request.MaximumTransactionAmount.HasValue)
                provider.MaximumTransactionAmount = request.MaximumTransactionAmount;

            if (request.SupportedAssets != null)
                provider.SupportedAssets = request.SupportedAssets;

            if (request.SupportedOutputCurrencies != null)
                provider.SupportedOutputCurrencies = request.SupportedOutputCurrencies;

            if (request.FeePercentage.HasValue)
                provider.FeePercentage = request.FeePercentage;

            if (request.LiquidityPoolAddress != null)
                provider.LiquidityPoolAddress = request.LiquidityPoolAddress;

            if (request.AvailableLiquidity.HasValue)
                provider.AvailableLiquidity = request.AvailableLiquidity;

            if (request.IsAvailable.HasValue)
                provider.IsAvailable = request.IsAvailable.Value;

            if (request.OperatingHours != null)
                provider.OperatingHours = request.OperatingHours;

            if (request.TimeZone != null)
                provider.TimeZone = request.TimeZone;

            if (request.Notes != null)
                provider.Notes = request.Notes;

            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidity provider {ProviderId} updated successfully", providerId);

            return provider.Adapt<LiquidityProviderResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating liquidity provider {ProviderId}", providerId);
            throw;
        }
    }

    /// <summary>
    /// Approve a liquidity provider
    /// </summary>
    public async Task<LiquidityProviderResponse> ApproveLiquidityProviderAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving liquidity provider {ProviderId}", providerId);

        try
        {
            var provider = await _context.LiquidityProviders
                .FirstOrDefaultAsync(lp => lp.Id == providerId, cancellationToken);

            if (provider == null)
                throw new InvalidOperationException($"Liquidity provider {providerId} not found");

            if (provider.Status != LiquidityProviderStatus.Pending)
                throw new InvalidOperationException($"Cannot approve liquidity provider {providerId} with status {provider.Status}");

            provider.Status = LiquidityProviderStatus.Active;
            provider.ApprovedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidity provider {ProviderId} approved successfully", providerId);

            return provider.Adapt<LiquidityProviderResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving liquidity provider {ProviderId}", providerId);
            throw;
        }
    }

    /// <summary>
    /// Suspend a liquidity provider
    /// </summary>
    public async Task<LiquidityProviderResponse> SuspendLiquidityProviderAsync(
        Guid providerId, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Suspending liquidity provider {ProviderId}, reason: {Reason}", providerId, reason);

        try
        {
            var provider = await _context.LiquidityProviders
                .FirstOrDefaultAsync(lp => lp.Id == providerId, cancellationToken);

            if (provider == null)
                throw new InvalidOperationException($"Liquidity provider {providerId} not found");

            if (provider.Status == LiquidityProviderStatus.Suspended)
                throw new InvalidOperationException($"Liquidity provider {providerId} is already suspended");

            provider.Status = LiquidityProviderStatus.Suspended;
            provider.Notes = $"{provider.Notes}\n[SUSPENDED] {DateTime.UtcNow:yyyy-MM-dd}: {reason}";
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidity provider {ProviderId} suspended successfully", providerId);

            return provider.Adapt<LiquidityProviderResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending liquidity provider {ProviderId}", providerId);
            throw;
        }
    }

    /// <summary>
    /// Find the best liquidity provider for a liquidation request
    /// </summary>
    public async Task<LiquidityProviderSummaryResponse> FindBestLiquidityProviderAsync(
        string assetSymbol,
        string outputSymbol,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Finding best liquidity provider for {AssetSymbol} -> {OutputSymbol}, amount: {Amount}",
            assetSymbol, outputSymbol, amount);

        try
        {
            var providers = await _context.LiquidityProviders
                .Where(lp => lp.Status == LiquidityProviderStatus.Active && 
                            lp.IsAvailable &&
                            (lp.SupportedAssets == null || lp.SupportedAssets.Contains(assetSymbol)) &&
                            (lp.SupportedOutputCurrencies == null || lp.SupportedOutputCurrencies.Contains(outputSymbol)) &&
                            (lp.MinimumTransactionAmount == null || amount >= lp.MinimumTransactionAmount) &&
                            (lp.MaximumTransactionAmount == null || amount <= lp.MaximumTransactionAmount))
                .OrderBy(lp => lp.FeePercentage ?? 0)
                .ThenByDescending(lp => lp.Rating ?? 0)
                .ThenBy(lp => lp.AverageResponseTimeMinutes ?? double.MaxValue)
                .ToListAsync(cancellationToken);

            if (!providers.Any())
                throw new InvalidOperationException($"No available liquidity providers found for {assetSymbol} -> {outputSymbol} with amount {amount}");

            var bestProvider = providers.First();

            _logger.LogInformation("Best liquidity provider found: {ProviderId} ({Name})", bestProvider.Id, bestProvider.Name);

            return bestProvider.Adapt<LiquidityProviderSummaryResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding best liquidity provider for {AssetSymbol} -> {OutputSymbol}",
                assetSymbol, outputSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get liquidity provider statistics
    /// </summary>
    public async Task<object> GetLiquidityProviderStatisticsAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting statistics for liquidity provider {ProviderId}", providerId);

        try
        {
            var provider = await _context.LiquidityProviders
                .FirstOrDefaultAsync(lp => lp.Id == providerId, cancellationToken);

            if (provider == null)
                throw new InvalidOperationException($"Liquidity provider {providerId} not found");

            // Return basic statistics (can be expanded with actual transaction data)
            return new
            {
                ProviderId = provider.Id,
                Name = provider.Name,
                Status = provider.Status.ToString(),
                TotalLiquidityProvided = provider.TotalLiquidityProvided,
                TotalFeesEarned = provider.TotalFeesEarned,
                SuccessfulLiquidations = provider.SuccessfulLiquidations,
                FailedLiquidations = provider.FailedLiquidations,
                AverageResponseTimeMinutes = provider.AverageResponseTimeMinutes,
                Rating = provider.Rating,
                SuccessRate = provider.SuccessfulLiquidations + provider.FailedLiquidations > 0 
                    ? (decimal)provider.SuccessfulLiquidations / (provider.SuccessfulLiquidations + provider.FailedLiquidations) * 100 
                    : 0,
                CreatedAt = provider.CreatedAt,
                LastActiveAt = provider.LastActiveAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for liquidity provider {ProviderId}", providerId);
            throw;
        }
    }

    /// <summary>
    /// Update liquidity provider availability
    /// </summary>
    public async Task<LiquidityProviderResponse> UpdateAvailabilityAsync(
        Guid providerId, 
        bool isAvailable, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating availability for liquidity provider {ProviderId} to {IsAvailable}",
            providerId, isAvailable);

        try
        {
            var provider = await _context.LiquidityProviders
                .FirstOrDefaultAsync(lp => lp.Id == providerId, cancellationToken);

            if (provider == null)
                throw new InvalidOperationException($"Liquidity provider {providerId} not found");

            provider.IsAvailable = isAvailable;
            provider.UpdatedAt = DateTime.UtcNow;

            if (isAvailable)
                provider.LastActiveAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Liquidity provider {ProviderId} availability updated to {IsAvailable}",
                providerId, isAvailable);

            return provider.Adapt<LiquidityProviderResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating availability for liquidity provider {ProviderId}", providerId);
            throw;
        }
    }
}
