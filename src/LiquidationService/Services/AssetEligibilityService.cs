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
/// Service for managing asset eligibility rules and restrictions
/// Follows the PaymentGatewayService pattern - returns response models directly and throws exceptions for errors
/// </summary>
public class AssetEligibilityService : IAssetEligibilityService
{
    private readonly LiquidationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AssetEligibilityService> _logger;

    public AssetEligibilityService(
        LiquidationDbContext context,
        IDistributedCache cache,
        ILogger<AssetEligibilityService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Configure asset eligibility rules
    /// </summary>
    public async Task<AssetEligibilityResponse> ConfigureAssetEligibilityAsync(
        ConfigureAssetEligibilityModel request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Configuring asset eligibility for {AssetSymbol}", request.AssetSymbol);

        try
        {
            // Check if asset eligibility already exists
            var existingEligibility = await _context.AssetEligibilities
                .FirstOrDefaultAsync(ae => ae.AssetSymbol == request.AssetSymbol, cancellationToken);

            AssetEligibility assetEligibility;

            if (existingEligibility != null)
            {
                // Update existing eligibility
                existingEligibility.AssetName = request.AssetName;
                existingEligibility.Status = request.Status;
                existingEligibility.IsEnabled = request.IsEnabled;
                existingEligibility.MinimumLiquidationAmount = request.MinimumLiquidationAmount;
                existingEligibility.MaximumLiquidationAmount = request.MaximumLiquidationAmount;
                existingEligibility.DailyLiquidationLimit = request.DailyLiquidationLimit;
                existingEligibility.MonthlyLiquidationLimit = request.MonthlyLiquidationLimit;
                existingEligibility.LockupPeriodDays = request.LockupPeriodDays;
                existingEligibility.MinimumHoldingPeriodDays = request.MinimumHoldingPeriodDays;
                existingEligibility.CoolingOffPeriodHours = request.CoolingOffPeriodHours;
                existingEligibility.RequiresKyc = request.RequiresKyc;
                existingEligibility.RequiresEnhancedDueDiligence = request.RequiresEnhancedDueDiligence;
                existingEligibility.RequiresMultiSignature = request.RequiresMultiSignature;
                existingEligibility.MultiSignatureThreshold = request.MultiSignatureThreshold;
                existingEligibility.RiskLevel = request.RiskLevel;
                existingEligibility.SupportedOutputCurrencies = request.SupportedOutputCurrencies;
                existingEligibility.RestrictedCountries = request.RestrictedCountries;
                existingEligibility.FeePercentage = request.FeePercentage;
                existingEligibility.FixedFee = request.FixedFee;
                existingEligibility.BlockchainNetwork = request.BlockchainNetwork;
                existingEligibility.ContractAddress = request.ContractAddress;
                existingEligibility.IsStablecoin = request.IsStablecoin;
                existingEligibility.IsPrivacyCoin = request.IsPrivacyCoin;
                existingEligibility.Notes = request.Notes;
                existingEligibility.UpdatedAt = DateTime.UtcNow;

                assetEligibility = existingEligibility;
            }
            else
            {
                // Create new eligibility
                assetEligibility = new AssetEligibility
                {
                    Id = Guid.NewGuid(),
                    AssetSymbol = request.AssetSymbol,
                    AssetName = request.AssetName,
                    Status = request.Status,
                    IsEnabled = request.IsEnabled,
                    MinimumLiquidationAmount = request.MinimumLiquidationAmount,
                    MaximumLiquidationAmount = request.MaximumLiquidationAmount,
                    DailyLiquidationLimit = request.DailyLiquidationLimit,
                    MonthlyLiquidationLimit = request.MonthlyLiquidationLimit,
                    LockupPeriodDays = request.LockupPeriodDays,
                    MinimumHoldingPeriodDays = request.MinimumHoldingPeriodDays,
                    CoolingOffPeriodHours = request.CoolingOffPeriodHours,
                    RequiresKyc = request.RequiresKyc,
                    RequiresEnhancedDueDiligence = request.RequiresEnhancedDueDiligence,
                    RequiresMultiSignature = request.RequiresMultiSignature,
                    MultiSignatureThreshold = request.MultiSignatureThreshold,
                    RiskLevel = request.RiskLevel,
                    SupportedOutputCurrencies = request.SupportedOutputCurrencies,
                    RestrictedCountries = request.RestrictedCountries,
                    FeePercentage = request.FeePercentage,
                    FixedFee = request.FixedFee,
                    BlockchainNetwork = request.BlockchainNetwork,
                    ContractAddress = request.ContractAddress,
                    IsStablecoin = request.IsStablecoin,
                    IsPrivacyCoin = request.IsPrivacyCoin,
                    Notes = request.Notes,
                    FirstEligibleAt = request.Status == AssetEligibilityStatus.Eligible ? DateTime.UtcNow : null,
                    EstimatedProcessingTimeMinutes = 30, // Default processing time
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.AssetEligibilities.Add(assetEligibility);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Asset eligibility configured for {AssetSymbol}, status: {Status}",
                request.AssetSymbol, request.Status);

            return assetEligibility.Adapt<AssetEligibilityResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring asset eligibility for {AssetSymbol}", request.AssetSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get asset eligibility by asset symbol
    /// </summary>
    public async Task<AssetEligibilityResponse?> GetAssetEligibilityAsync(
        string assetSymbol, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting asset eligibility for {AssetSymbol}", assetSymbol);

        try
        {
            var assetEligibility = await _context.AssetEligibilities
                .FirstOrDefaultAsync(ae => ae.AssetSymbol == assetSymbol, cancellationToken);

            if (assetEligibility == null)
            {
                _logger.LogWarning("Asset eligibility not found for {AssetSymbol}", assetSymbol);
                return null;
            }

            return assetEligibility.Adapt<AssetEligibilityResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting asset eligibility for {AssetSymbol}", assetSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get all asset eligibilities with filtering
    /// </summary>
    public async Task<PaginatedResponse<AssetEligibilityResponse>> GetAssetEligibilitiesAsync(
        string? status = null,
        bool? isEnabled = null,
        string? riskLevel = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting asset eligibilities with filters - Status: {Status}, Enabled: {IsEnabled}, RiskLevel: {RiskLevel}, Page: {Page}",
            status, isEnabled, riskLevel, page);

        try
        {
            var query = _context.AssetEligibilities.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AssetEligibilityStatus>(status, out var statusEnum))
                query = query.Where(ae => ae.Status == statusEnum);

            if (isEnabled.HasValue)
                query = query.Where(ae => ae.IsEnabled == isEnabled.Value);

            if (!string.IsNullOrEmpty(riskLevel) && Enum.TryParse<RiskLevel>(riskLevel, out var riskLevelEnum))
                query = query.Where(ae => ae.RiskLevel == riskLevelEnum);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination and ordering
            var assetEligibilities = await query
                .OrderBy(ae => ae.AssetSymbol)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var responses = assetEligibilities.Adapt<List<AssetEligibilityResponse>>();

            return new PaginatedResponse<AssetEligibilityResponse>
            {
                Items = responses,
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
            _logger.LogError(ex, "Error getting asset eligibilities");
            throw;
        }
    }

    /// <summary>
    /// Check if an asset is eligible for liquidation
    /// </summary>
    public async Task<bool> IsAssetEligibleForLiquidationAsync(
        string assetSymbol,
        decimal amount,
        Guid userId,
        string? userCountry = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking asset eligibility for liquidation - Asset: {AssetSymbol}, Amount: {Amount}, User: {UserId}, Country: {UserCountry}",
            assetSymbol, amount, userId, userCountry);

        try
        {
            var assetEligibility = await _context.AssetEligibilities
                .FirstOrDefaultAsync(ae => ae.AssetSymbol == assetSymbol, cancellationToken);

            if (assetEligibility == null)
            {
                _logger.LogWarning("Asset {AssetSymbol} not found in eligibility database", assetSymbol);
                return false;
            }

            // Check basic eligibility
            if (assetEligibility.Status != AssetEligibilityStatus.Eligible || !assetEligibility.IsEnabled)
            {
                _logger.LogInformation("Asset {AssetSymbol} is not eligible or disabled", assetSymbol);
                return false;
            }

            // Check amount limits
            if (assetEligibility.MinimumLiquidationAmount.HasValue && amount < assetEligibility.MinimumLiquidationAmount.Value)
            {
                _logger.LogInformation("Amount {Amount} below minimum {MinAmount} for asset {AssetSymbol}",
                    amount, assetEligibility.MinimumLiquidationAmount.Value, assetSymbol);
                return false;
            }

            if (assetEligibility.MaximumLiquidationAmount.HasValue && amount > assetEligibility.MaximumLiquidationAmount.Value)
            {
                _logger.LogInformation("Amount {Amount} above maximum {MaxAmount} for asset {AssetSymbol}",
                    amount, assetEligibility.MaximumLiquidationAmount.Value, assetSymbol);
                return false;
            }

            // Check country restrictions
            if (!string.IsNullOrEmpty(userCountry) && !string.IsNullOrEmpty(assetEligibility.RestrictedCountries))
            {
                var restrictedCountries = assetEligibility.RestrictedCountries.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim().ToUpper()).ToList();

                if (restrictedCountries.Contains(userCountry.ToUpper()))
                {
                    _logger.LogInformation("Asset {AssetSymbol} is restricted in country {Country}", assetSymbol, userCountry);
                    return false;
                }
            }

            // Check daily/monthly limits (simplified check - in real implementation would check actual usage)
            // For now, we'll assume limits are not exceeded

            _logger.LogInformation("Asset {AssetSymbol} is eligible for liquidation", assetSymbol);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking asset eligibility for {AssetSymbol}", assetSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get detailed eligibility validation for an asset
    /// </summary>
    public async Task<object> ValidateAssetEligibilityAsync(
        string assetSymbol,
        decimal amount,
        Guid userId,
        string? userCountry = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating asset eligibility details for {AssetSymbol}", assetSymbol);

        try
        {
            var assetEligibility = await _context.AssetEligibilities
                .FirstOrDefaultAsync(ae => ae.AssetSymbol == assetSymbol, cancellationToken);

            if (assetEligibility == null)
            {
                return new
                {
                    AssetSymbol = assetSymbol,
                    IsEligible = false,
                    Reason = "Asset not found in eligibility database",
                    ValidationResults = new List<object>(),
                    ValidatedAt = DateTime.UtcNow
                };
            }

            var validationResults = new List<object>();
            var isEligible = true;
            var reasons = new List<string>();

            // Basic eligibility check
            if (assetEligibility.Status != AssetEligibilityStatus.Eligible)
            {
                isEligible = false;
                reasons.Add($"Asset status is {assetEligibility.Status}");
                validationResults.Add(new
                {
                    Check = "AssetStatus",
                    Passed = false,
                    Message = $"Asset status is {assetEligibility.Status}, expected Eligible"
                });
            }
            else
            {
                validationResults.Add(new
                {
                    Check = "AssetStatus",
                    Passed = true,
                    Message = "Asset status is eligible"
                });
            }

            // Enabled check
            if (!assetEligibility.IsEnabled)
            {
                isEligible = false;
                reasons.Add("Asset is disabled");
                validationResults.Add(new
                {
                    Check = "AssetEnabled",
                    Passed = false,
                    Message = "Asset is currently disabled"
                });
            }
            else
            {
                validationResults.Add(new
                {
                    Check = "AssetEnabled",
                    Passed = true,
                    Message = "Asset is enabled"
                });
            }

            // Amount limits check
            if (assetEligibility.MinimumLiquidationAmount.HasValue)
            {
                if (amount < assetEligibility.MinimumLiquidationAmount.Value)
                {
                    isEligible = false;
                    reasons.Add($"Amount below minimum of {assetEligibility.MinimumLiquidationAmount.Value}");
                    validationResults.Add(new
                    {
                        Check = "MinimumAmount",
                        Passed = false,
                        Message = $"Amount {amount} is below minimum {assetEligibility.MinimumLiquidationAmount.Value}"
                    });
                }
                else
                {
                    validationResults.Add(new
                    {
                        Check = "MinimumAmount",
                        Passed = true,
                        Message = $"Amount meets minimum requirement of {assetEligibility.MinimumLiquidationAmount.Value}"
                    });
                }
            }

            if (assetEligibility.MaximumLiquidationAmount.HasValue)
            {
                if (amount > assetEligibility.MaximumLiquidationAmount.Value)
                {
                    isEligible = false;
                    reasons.Add($"Amount exceeds maximum of {assetEligibility.MaximumLiquidationAmount.Value}");
                    validationResults.Add(new
                    {
                        Check = "MaximumAmount",
                        Passed = false,
                        Message = $"Amount {amount} exceeds maximum {assetEligibility.MaximumLiquidationAmount.Value}"
                    });
                }
                else
                {
                    validationResults.Add(new
                    {
                        Check = "MaximumAmount",
                        Passed = true,
                        Message = $"Amount is within maximum limit of {assetEligibility.MaximumLiquidationAmount.Value}"
                    });
                }
            }

            // Country restrictions check
            if (!string.IsNullOrEmpty(userCountry) && !string.IsNullOrEmpty(assetEligibility.RestrictedCountries))
            {
                var restrictedCountries = assetEligibility.RestrictedCountries.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim().ToUpper()).ToList();

                if (restrictedCountries.Contains(userCountry.ToUpper()))
                {
                    isEligible = false;
                    reasons.Add($"Asset restricted in country {userCountry}");
                    validationResults.Add(new
                    {
                        Check = "CountryRestriction",
                        Passed = false,
                        Message = $"Asset is restricted in country {userCountry}"
                    });
                }
                else
                {
                    validationResults.Add(new
                    {
                        Check = "CountryRestriction",
                        Passed = true,
                        Message = $"Asset is allowed in country {userCountry}"
                    });
                }
            }

            return new
            {
                AssetSymbol = assetSymbol,
                AssetName = assetEligibility.AssetName,
                IsEligible = isEligible,
                Reason = isEligible ? "Asset is eligible for liquidation" : string.Join("; ", reasons),
                RiskLevel = assetEligibility.RiskLevel.ToString(),
                RequiresKyc = assetEligibility.RequiresKyc,
                RequiresEnhancedDueDiligence = assetEligibility.RequiresEnhancedDueDiligence,
                RequiresMultiSignature = assetEligibility.RequiresMultiSignature,
                EstimatedProcessingTimeMinutes = assetEligibility.EstimatedProcessingTimeMinutes,
                ValidationResults = validationResults,
                ValidatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating asset eligibility for {AssetSymbol}", assetSymbol);
            throw;
        }
    }

    /// <summary>
    /// Update asset eligibility status
    /// </summary>
    public async Task<AssetEligibilityResponse> UpdateAssetStatusAsync(
        string assetSymbol,
        bool isEnabled,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating asset status for {AssetSymbol} to {IsEnabled}, reason: {Reason}",
            assetSymbol, isEnabled, reason);

        try
        {
            var assetEligibility = await _context.AssetEligibilities
                .FirstOrDefaultAsync(ae => ae.AssetSymbol == assetSymbol, cancellationToken);

            if (assetEligibility == null)
                throw new InvalidOperationException($"Asset eligibility not found for {assetSymbol}");

            assetEligibility.IsEnabled = isEnabled;
            assetEligibility.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(reason))
            {
                assetEligibility.Notes = $"{assetEligibility.Notes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Status changed to {(isEnabled ? "Enabled" : "Disabled")}: {reason}";
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Asset status updated for {AssetSymbol}", assetSymbol);

            return assetEligibility.Adapt<AssetEligibilityResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset status for {AssetSymbol}", assetSymbol);
            throw;
        }
    }

    /// <summary>
    /// Get supported output currencies for an asset
    /// </summary>
    public async Task<IEnumerable<string>> GetSupportedOutputCurrenciesAsync(
        string assetSymbol, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting supported output currencies for {AssetSymbol}", assetSymbol);

        try
        {
            var assetEligibility = await _context.AssetEligibilities
                .FirstOrDefaultAsync(ae => ae.AssetSymbol == assetSymbol, cancellationToken);

            if (assetEligibility == null || string.IsNullOrEmpty(assetEligibility.SupportedOutputCurrencies))
            {
                // Return default supported currencies if none specified
                return new[] { "USD", "EUR", "USDT", "USDC", "BTC", "ETH" };
            }

            var currencies = assetEligibility.SupportedOutputCurrencies
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToUpper())
                .Distinct()
                .ToList();

            return currencies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported output currencies for {AssetSymbol}", assetSymbol);
            throw;
        }
    }

    /// <summary>
    /// Check if asset liquidation is restricted in a country
    /// </summary>
    public async Task<bool> IsAssetRestrictedInCountryAsync(
        string assetSymbol,
        string country,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if asset {AssetSymbol} is restricted in country {Country}", assetSymbol, country);

        try
        {
            var assetEligibility = await _context.AssetEligibilities
                .FirstOrDefaultAsync(ae => ae.AssetSymbol == assetSymbol, cancellationToken);

            if (assetEligibility == null || string.IsNullOrEmpty(assetEligibility.RestrictedCountries))
            {
                return false; // Not restricted if no restrictions defined
            }

            var restrictedCountries = assetEligibility.RestrictedCountries
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToUpper())
                .ToList();

            var isRestricted = restrictedCountries.Contains(country.ToUpper());

            _logger.LogInformation("Asset {AssetSymbol} restriction check for {Country}: {IsRestricted}",
                assetSymbol, country, isRestricted);

            return isRestricted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking country restriction for asset {AssetSymbol} in {Country}", assetSymbol, country);
            throw;
        }
    }

    /// <summary>
    /// Get asset eligibility statistics
    /// </summary>
    public async Task<object> GetAssetEligibilityStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting asset eligibility statistics");

        try
        {
            var assetEligibilities = await _context.AssetEligibilities.ToListAsync(cancellationToken);

            var totalAssets = assetEligibilities.Count;
            var eligibleAssets = assetEligibilities.Count(ae => ae.Status == AssetEligibilityStatus.Eligible);
            var enabledAssets = assetEligibilities.Count(ae => ae.IsEnabled);
            var restrictedAssets = assetEligibilities.Count(ae => ae.Status == AssetEligibilityStatus.Restricted);

            var statusDistribution = assetEligibilities.GroupBy(ae => ae.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = totalAssets > 0 ? (decimal)g.Count() / totalAssets * 100 : 0
                })
                .ToList();

            var riskDistribution = assetEligibilities.GroupBy(ae => ae.RiskLevel)
                .Select(g => new
                {
                    RiskLevel = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = totalAssets > 0 ? (decimal)g.Count() / totalAssets * 100 : 0
                })
                .ToList();

            var assetTypes = new
            {
                Stablecoins = assetEligibilities.Count(ae => ae.IsStablecoin),
                PrivacyCoins = assetEligibilities.Count(ae => ae.IsPrivacyCoin),
                RegularAssets = assetEligibilities.Count(ae => !ae.IsStablecoin && !ae.IsPrivacyCoin)
            };

            var averageProcessingTime = assetEligibilities.Where(ae => ae.EstimatedProcessingTimeMinutes.HasValue)
                .Average(ae => ae.EstimatedProcessingTimeMinutes.Value);

            return new
            {
                Summary = new
                {
                    TotalAssets = totalAssets,
                    EligibleAssets = eligibleAssets,
                    EnabledAssets = enabledAssets,
                    RestrictedAssets = restrictedAssets,
                    EligibilityRate = totalAssets > 0 ? (decimal)eligibleAssets / totalAssets * 100 : 0,
                    EnabledRate = totalAssets > 0 ? (decimal)enabledAssets / totalAssets * 100 : 0
                },
                StatusDistribution = statusDistribution,
                RiskDistribution = riskDistribution,
                AssetTypes = assetTypes,
                AverageProcessingTimeMinutes = averageProcessingTime,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting asset eligibility statistics");
            throw;
        }
    }
}
