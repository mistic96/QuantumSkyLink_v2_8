using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using LiquidationService.Data;
using LiquidationService.Data.Entities;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using Mapster;

namespace LiquidationService.Services;

/// <summary>
/// Service for compliance checks and risk management
/// Follows the PaymentGatewayService pattern - returns response models directly and throws exceptions for errors
/// </summary>
public class ComplianceService : IComplianceService
{
    private readonly LiquidationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ComplianceService> _logger;

    public ComplianceService(
        LiquidationDbContext context,
        IDistributedCache cache,
        ILogger<ComplianceService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Perform KYC verification for a liquidation request
    /// </summary>
    public async Task<ComplianceCheckResponse> PerformKycCheckAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing KYC check for liquidation request {LiquidationRequestId}, user {UserId}",
            liquidationRequestId, userId);

        try
        {
            // Create compliance check record
            var complianceCheck = new ComplianceCheck
            {
                Id = Guid.NewGuid(),
                LiquidationRequestId = liquidationRequestId,
                CheckType = ComplianceCheckType.KycVerification,
                Result = ComplianceCheckResult.Pending,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ComplianceChecks.Add(complianceCheck);
            await _context.SaveChangesAsync(cancellationToken);

            // Simulate KYC verification process
            // In a real implementation, this would integrate with external KYC providers
            await Task.Delay(100, cancellationToken); // Simulate processing time

            // For simulation, we'll pass most KYC checks
            var random = new Random();
            var passRate = 0.95; // 95% pass rate
            var passed = random.NextDouble() < passRate;

            complianceCheck.Result = passed ? ComplianceCheckResult.Passed : ComplianceCheckResult.Failed;
            complianceCheck.CompletedAt = DateTime.UtcNow;
            complianceCheck.DurationMs = (long)(complianceCheck.CompletedAt.Value - complianceCheck.StartedAt.Value).TotalMilliseconds;
            complianceCheck.Provider = "SimulatedKYCProvider";
            complianceCheck.ExternalReferenceId = $"KYC_{Guid.NewGuid():N}";
            complianceCheck.RiskScore = random.Next(0, 101);
            complianceCheck.RiskLevel = complianceCheck.RiskScore switch
            {
                <= 25 => RiskLevel.Low,
                <= 50 => RiskLevel.Medium,
                <= 75 => RiskLevel.High,
                _ => RiskLevel.Critical
            };

            if (!passed)
            {
                complianceCheck.FailureReason = "KYC verification failed - insufficient documentation";
                complianceCheck.RequiresManualReview = true;
            }

            complianceCheck.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("KYC check completed for liquidation request {LiquidationRequestId}, result: {Result}",
                liquidationRequestId, complianceCheck.Result);

            return complianceCheck.Adapt<ComplianceCheckResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing KYC check for liquidation request {LiquidationRequestId}", liquidationRequestId);
            throw;
        }
    }

    /// <summary>
    /// Perform AML (Anti-Money Laundering) check
    /// </summary>
    public async Task<ComplianceCheckResponse> PerformAmlCheckAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        decimal amount, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing AML check for liquidation request {LiquidationRequestId}, user {UserId}, amount {Amount}",
            liquidationRequestId, userId, amount);

        try
        {
            // Create compliance check record
            var complianceCheck = new ComplianceCheck
            {
                Id = Guid.NewGuid(),
                LiquidationRequestId = liquidationRequestId,
                CheckType = ComplianceCheckType.AmlScreening,
                Result = ComplianceCheckResult.Pending,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ComplianceChecks.Add(complianceCheck);
            await _context.SaveChangesAsync(cancellationToken);

            // Simulate AML screening process
            await Task.Delay(150, cancellationToken); // Simulate processing time

            var random = new Random();
            
            // Higher amounts have slightly higher risk
            var basePassRate = amount > 100000 ? 0.85 : 0.95;
            var passed = random.NextDouble() < basePassRate;

            complianceCheck.Result = passed ? ComplianceCheckResult.Passed : ComplianceCheckResult.Failed;
            complianceCheck.CompletedAt = DateTime.UtcNow;
            complianceCheck.DurationMs = (long)(complianceCheck.CompletedAt.Value - complianceCheck.StartedAt.Value).TotalMilliseconds;
            complianceCheck.Provider = "SimulatedAMLProvider";
            complianceCheck.ExternalReferenceId = $"AML_{Guid.NewGuid():N}";
            
            // Risk score influenced by amount
            var baseRiskScore = random.Next(0, 51);
            if (amount > 100000) baseRiskScore += 20;
            if (amount > 500000) baseRiskScore += 15;
            complianceCheck.RiskScore = Math.Min(baseRiskScore, 100);
            
            complianceCheck.RiskLevel = complianceCheck.RiskScore switch
            {
                <= 25 => RiskLevel.Low,
                <= 50 => RiskLevel.Medium,
                <= 75 => RiskLevel.High,
                _ => RiskLevel.Critical
            };

            if (!passed)
            {
                complianceCheck.FailureReason = "AML screening detected suspicious patterns";
                complianceCheck.RequiresManualReview = true;
                complianceCheck.Recommendations = "Manual review required for transaction patterns";
            }

            complianceCheck.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("AML check completed for liquidation request {LiquidationRequestId}, result: {Result}",
                liquidationRequestId, complianceCheck.Result);

            return complianceCheck.Adapt<ComplianceCheckResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing AML check for liquidation request {LiquidationRequestId}", liquidationRequestId);
            throw;
        }
    }

    /// <summary>
    /// Perform sanctions screening
    /// </summary>
    public async Task<ComplianceCheckResponse> PerformSanctionsScreeningAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing sanctions screening for liquidation request {LiquidationRequestId}, user {UserId}",
            liquidationRequestId, userId);

        try
        {
            // Create compliance check record
            var complianceCheck = new ComplianceCheck
            {
                Id = Guid.NewGuid(),
                LiquidationRequestId = liquidationRequestId,
                CheckType = ComplianceCheckType.SanctionsScreening,
                Result = ComplianceCheckResult.Pending,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ComplianceChecks.Add(complianceCheck);
            await _context.SaveChangesAsync(cancellationToken);

            // Simulate sanctions screening process
            await Task.Delay(200, cancellationToken); // Simulate processing time

            var random = new Random();
            
            // Very high pass rate for sanctions (99.9%)
            var passed = random.NextDouble() < 0.999;

            complianceCheck.Result = passed ? ComplianceCheckResult.Passed : ComplianceCheckResult.Failed;
            complianceCheck.CompletedAt = DateTime.UtcNow;
            complianceCheck.DurationMs = (long)(complianceCheck.CompletedAt.Value - complianceCheck.StartedAt.Value).TotalMilliseconds;
            complianceCheck.Provider = "SimulatedSanctionsProvider";
            complianceCheck.ExternalReferenceId = $"SANCTIONS_{Guid.NewGuid():N}";
            complianceCheck.RiskScore = passed ? random.Next(0, 26) : random.Next(90, 101);
            complianceCheck.RiskLevel = passed ? RiskLevel.Low : RiskLevel.Critical;

            if (!passed)
            {
                complianceCheck.FailureReason = "User appears on sanctions list";
                complianceCheck.RequiresManualReview = true;
                complianceCheck.Recommendations = "Immediate escalation required - potential sanctions match";
            }

            complianceCheck.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sanctions screening completed for liquidation request {LiquidationRequestId}, result: {Result}",
                liquidationRequestId, complianceCheck.Result);

            return complianceCheck.Adapt<ComplianceCheckResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing sanctions screening for liquidation request {LiquidationRequestId}", liquidationRequestId);
            throw;
        }
    }

    /// <summary>
    /// Perform risk assessment for a liquidation request
    /// </summary>
    public async Task<ComplianceCheckResponse> PerformRiskAssessmentAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        string assetSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing risk assessment for liquidation request {LiquidationRequestId}, user {UserId}, asset {AssetSymbol}, amount {Amount}",
            liquidationRequestId, userId, assetSymbol, amount);

        try
        {
            // Create compliance check record
            var complianceCheck = new ComplianceCheck
            {
                Id = Guid.NewGuid(),
                LiquidationRequestId = liquidationRequestId,
                CheckType = ComplianceCheckType.RiskAssessment,
                Result = ComplianceCheckResult.Pending,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ComplianceChecks.Add(complianceCheck);
            await _context.SaveChangesAsync(cancellationToken);

            // Simulate risk assessment process
            await Task.Delay(300, cancellationToken); // Simulate processing time

            var random = new Random();
            
            // Calculate risk score based on multiple factors
            var riskScore = 0;
            
            // Amount-based risk
            if (amount > 1000000) riskScore += 30;
            else if (amount > 500000) riskScore += 20;
            else if (amount > 100000) riskScore += 10;
            else if (amount > 50000) riskScore += 5;
            
            // Asset-based risk (privacy coins are higher risk)
            var privacyCoins = new[] { "XMR", "ZEC", "DASH" };
            if (privacyCoins.Contains(assetSymbol.ToUpper()))
                riskScore += 25;
            
            // Add some randomness
            riskScore += random.Next(0, 21);
            
            // Ensure score is within bounds
            riskScore = Math.Min(riskScore, 100);

            var riskLevel = riskScore switch
            {
                <= 25 => RiskLevel.Low,
                <= 50 => RiskLevel.Medium,
                <= 75 => RiskLevel.High,
                _ => RiskLevel.Critical
            };

            var passed = riskScore <= 75; // Fail if critical risk

            complianceCheck.Result = passed ? ComplianceCheckResult.Passed : ComplianceCheckResult.RequiresReview;
            complianceCheck.CompletedAt = DateTime.UtcNow;
            complianceCheck.DurationMs = (long)(complianceCheck.CompletedAt.Value - complianceCheck.StartedAt.Value).TotalMilliseconds;
            complianceCheck.Provider = "InternalRiskEngine";
            complianceCheck.ExternalReferenceId = $"RISK_{Guid.NewGuid():N}";
            complianceCheck.RiskScore = riskScore;
            complianceCheck.RiskLevel = riskLevel;

            if (!passed)
            {
                complianceCheck.FailureReason = "High risk transaction detected";
                complianceCheck.RequiresManualReview = true;
                complianceCheck.Recommendations = "Manual review required due to high risk score";
            }
            else if (riskLevel == RiskLevel.High)
            {
                complianceCheck.RequiresManualReview = true;
                complianceCheck.Recommendations = "Enhanced monitoring recommended";
            }

            complianceCheck.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Risk assessment completed for liquidation request {LiquidationRequestId}, risk score: {RiskScore}, result: {Result}",
                liquidationRequestId, riskScore, complianceCheck.Result);

            return complianceCheck.Adapt<ComplianceCheckResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing risk assessment for liquidation request {LiquidationRequestId}", liquidationRequestId);
            throw;
        }
    }

    /// <summary>
    /// Get compliance check by ID
    /// </summary>
    public async Task<ComplianceCheckResponse?> GetComplianceCheckAsync(
        Guid checkId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting compliance check {CheckId}", checkId);

        try
        {
            var complianceCheck = await _context.ComplianceChecks
                .FirstOrDefaultAsync(cc => cc.Id == checkId, cancellationToken);

            if (complianceCheck == null)
            {
                _logger.LogWarning("Compliance check {CheckId} not found", checkId);
                return null;
            }

            return complianceCheck.Adapt<ComplianceCheckResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance check {CheckId}", checkId);
            throw;
        }
    }

    /// <summary>
    /// Get all compliance checks for a liquidation request
    /// </summary>
    public async Task<IEnumerable<ComplianceCheckResponse>> GetComplianceChecksForRequestAsync(
        Guid liquidationRequestId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting compliance checks for liquidation request {LiquidationRequestId}", liquidationRequestId);

        try
        {
            var complianceChecks = await _context.ComplianceChecks
                .Where(cc => cc.LiquidationRequestId == liquidationRequestId)
                .OrderBy(cc => cc.CreatedAt)
                .ToListAsync(cancellationToken);

            return complianceChecks.Adapt<List<ComplianceCheckResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance checks for liquidation request {LiquidationRequestId}", liquidationRequestId);
            throw;
        }
    }

    /// <summary>
    /// Override a compliance check result (admin function)
    /// </summary>
    public async Task<ComplianceCheckResponse> OverrideComplianceCheckAsync(
        Guid checkId, 
        string newResult, 
        string reason, 
        Guid reviewedBy, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Overriding compliance check {CheckId} to {NewResult}, reviewed by {ReviewedBy}",
            checkId, newResult, reviewedBy);

        try
        {
            var complianceCheck = await _context.ComplianceChecks
                .FirstOrDefaultAsync(cc => cc.Id == checkId, cancellationToken);

            if (complianceCheck == null)
                throw new InvalidOperationException($"Compliance check {checkId} not found");

            // Parse the new result
            if (!Enum.TryParse<ComplianceCheckResult>(newResult, out var parsedResult))
                throw new ArgumentException($"Invalid compliance result: {newResult}");

            complianceCheck.Result = parsedResult;
            complianceCheck.IsOverridden = true;
            complianceCheck.ReviewedAt = DateTime.UtcNow;
            complianceCheck.ReviewComments = $"Override by admin {reviewedBy}: {reason}";
            complianceCheck.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Compliance check {CheckId} overridden successfully", checkId);

            return complianceCheck.Adapt<ComplianceCheckResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error overriding compliance check {CheckId}", checkId);
            throw;
        }
    }

    /// <summary>
    /// Check if a liquidation request passes all compliance requirements
    /// </summary>
    public async Task<bool> IsComplianceApprovedAsync(
        Guid liquidationRequestId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking compliance approval for liquidation request {LiquidationRequestId}", liquidationRequestId);

        try
        {
            var complianceChecks = await _context.ComplianceChecks
                .Where(cc => cc.LiquidationRequestId == liquidationRequestId)
                .ToListAsync(cancellationToken);

            if (!complianceChecks.Any())
            {
                _logger.LogWarning("No compliance checks found for liquidation request {LiquidationRequestId}", liquidationRequestId);
                return false;
            }

            // All checks must pass or be skipped
            var allPassed = complianceChecks.All(cc => 
                cc.Result == ComplianceCheckResult.Passed || 
                cc.Result == ComplianceCheckResult.Skipped);

            _logger.LogInformation("Compliance approval for liquidation request {LiquidationRequestId}: {Approved}",
                liquidationRequestId, allPassed);

            return allPassed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking compliance approval for liquidation request {LiquidationRequestId}", liquidationRequestId);
            throw;
        }
    }

    /// <summary>
    /// Get compliance statistics for reporting
    /// </summary>
    public async Task<object> GetComplianceStatisticsAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting compliance statistics from {FromDate} to {ToDate}", fromDate, toDate);

        try
        {
            var query = _context.ComplianceChecks.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(cc => cc.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(cc => cc.CreatedAt <= toDate.Value);

            var checks = await query.ToListAsync(cancellationToken);

            var totalChecks = checks.Count;
            var passedChecks = checks.Count(c => c.Result == ComplianceCheckResult.Passed);
            var failedChecks = checks.Count(c => c.Result == ComplianceCheckResult.Failed);
            var pendingChecks = checks.Count(c => c.Result == ComplianceCheckResult.Pending);
            var reviewChecks = checks.Count(c => c.Result == ComplianceCheckResult.RequiresReview);

            var checksByType = checks.GroupBy(c => c.CheckType)
                .Select(g => new
                {
                    CheckType = g.Key.ToString(),
                    Total = g.Count(),
                    Passed = g.Count(c => c.Result == ComplianceCheckResult.Passed),
                    Failed = g.Count(c => c.Result == ComplianceCheckResult.Failed),
                    PassRate = g.Count() > 0 ? (decimal)g.Count(c => c.Result == ComplianceCheckResult.Passed) / g.Count() * 100 : 0
                })
                .ToList();

            var riskDistribution = checks.Where(c => c.RiskLevel.HasValue)
                .GroupBy(c => c.RiskLevel.Value)
                .Select(g => new
                {
                    RiskLevel = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = totalChecks > 0 ? (decimal)g.Count() / totalChecks * 100 : 0
                })
                .ToList();

            var averageProcessingTime = checks.Where(c => c.DurationMs.HasValue)
                .Average(c => c.DurationMs.Value);

            return new
            {
                Period = new
                {
                    FromDate = fromDate,
                    ToDate = toDate
                },
                Summary = new
                {
                    TotalChecks = totalChecks,
                    PassedChecks = passedChecks,
                    FailedChecks = failedChecks,
                    PendingChecks = pendingChecks,
                    ReviewChecks = reviewChecks,
                    PassRate = totalChecks > 0 ? (decimal)passedChecks / totalChecks * 100 : 0,
                    AverageProcessingTimeMs = averageProcessingTime
                },
                ChecksByType = checksByType,
                RiskDistribution = riskDistribution,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance statistics");
            throw;
        }
    }

    /// <summary>
    /// Perform comprehensive compliance check (all checks combined)
    /// </summary>
    public async Task<IEnumerable<ComplianceCheckResponse>> PerformComprehensiveComplianceCheckAsync(
        Guid liquidationRequestId, 
        Guid userId, 
        string assetSymbol, 
        decimal amount, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing comprehensive compliance check for liquidation request {LiquidationRequestId}",
            liquidationRequestId);

        try
        {
            var results = new List<ComplianceCheckResponse>();

            // Perform all compliance checks in parallel for efficiency
            var kycTask = PerformKycCheckAsync(liquidationRequestId, userId, cancellationToken);
            var amlTask = PerformAmlCheckAsync(liquidationRequestId, userId, amount, cancellationToken);
            var sanctionsTask = PerformSanctionsScreeningAsync(liquidationRequestId, userId, cancellationToken);
            var riskTask = PerformRiskAssessmentAsync(liquidationRequestId, userId, assetSymbol, amount, cancellationToken);

            await Task.WhenAll(kycTask, amlTask, sanctionsTask, riskTask);

            results.Add(await kycTask);
            results.Add(await amlTask);
            results.Add(await sanctionsTask);
            results.Add(await riskTask);

            _logger.LogInformation("Comprehensive compliance check completed for liquidation request {LiquidationRequestId}, {CheckCount} checks performed",
                liquidationRequestId, results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing comprehensive compliance check for liquidation request {LiquidationRequestId}", liquidationRequestId);
            throw;
        }
    }

    /// <summary>
    /// Get pending compliance checks requiring manual review
    /// </summary>
    public async Task<PaginatedResponse<ComplianceCheckResponse>> GetPendingReviewsAsync(
        int page = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting pending compliance reviews, page {Page}, size {PageSize}", page, pageSize);

        try
        {
            var query = _context.ComplianceChecks
                .Where(cc => cc.RequiresManualReview && cc.ReviewedAt == null)
                .OrderBy(cc => cc.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var checks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var responses = checks.Adapt<List<ComplianceCheckResponse>>();

            return new PaginatedResponse<ComplianceCheckResponse>
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
            _logger.LogError(ex, "Error getting pending compliance reviews");
            throw;
        }
    }
}
