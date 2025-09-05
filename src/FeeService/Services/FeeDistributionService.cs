using Microsoft.EntityFrameworkCore;
using FeeService.Data;
using FeeService.Data.Entities;
using FeeService.Models.Requests;
using FeeService.Models.Responses;
using FeeService.Services.Interfaces;
using Mapster;

namespace FeeService.Services;

public class FeeDistributionService : IFeeDistributionService
{
    private readonly FeeDbContext _context;
    private readonly ILogger<FeeDistributionService> _logger;
    private readonly IConfiguration _configuration;

    private readonly bool _mockMode;

    public FeeDistributionService(
        FeeDbContext context,
        ILogger<FeeDistributionService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _mockMode = _configuration.GetValue<bool>("FeeService:EnableMockMode", true);
    }

    public async Task<IEnumerable<FeeDistributionResponse>> DistributeFeesAsync(DistributeFeesRequest request)
    {
        try
        {
            _logger.LogInformation("Distributing fees for transaction {TransactionId}, type {FeeType}, amount {Amount} {Currency}", 
                request.TransactionId, request.FeeType, request.TotalAmount, request.Currency);

            // Get the transaction
            var transaction = await _context.FeeTransactions
                .Include(ft => ft.FeeConfiguration)
                .FirstOrDefaultAsync(ft => ft.Id == request.TransactionId);

            if (transaction == null)
            {
                throw new InvalidOperationException($"Transaction not found: {request.TransactionId}");
            }

            if (transaction.Status != "Completed" && !request.ForceDistribution)
            {
                throw new InvalidOperationException($"Cannot distribute fees for transaction with status: {transaction.Status}");
            }

            // Get distribution rules
            var distributionRules = await GetDistributionRulesAsync(request.FeeType, true);
            if (!distributionRules.Any())
            {
                throw new InvalidOperationException($"No distribution rules found for fee type: {request.FeeType}");
            }

            // Validate distribution rules
            if (!await ValidateDistributionRulesAsync(request.FeeType))
            {
                throw new InvalidOperationException($"Distribution rules validation failed for fee type: {request.FeeType}");
            }

            // Calculate distributions
            var distributions = new List<FeeDistribution>();
            var remainingAmount = request.TotalAmount;

            foreach (var rule in distributionRules.OrderBy(r => r.Priority))
            {
                if (remainingAmount <= 0) break;

                var distributionAmount = CalculateDistributionAmount(rule, request.TotalAmount, remainingAmount);
                if (distributionAmount <= 0) continue;

                var distribution = new FeeDistribution
                {
                    Id = Guid.NewGuid(),
                    FeeTransactionId = request.TransactionId,
                    DistributionRuleId = rule.Id,
                    RecipientType = rule.RecipientType,
                    RecipientId = rule.RecipientId,
                    Amount = distributionAmount,
                    Currency = request.Currency,
                    Percentage = rule.Percentage,
                    Status = "Pending",
                    Description = $"Fee distribution to {rule.RecipientType} ({rule.Percentage}%)",
                    Metadata = request.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                distributions.Add(distribution);
                remainingAmount -= distributionAmount;

                _logger.LogDebug("Created distribution: {RecipientType} {RecipientId} - {Amount} {Currency} ({Percentage}%)", 
                    rule.RecipientType, rule.RecipientId, distributionAmount, request.Currency, rule.Percentage);
            }

            // Handle any remaining amount (rounding differences)
            if (remainingAmount > 0.01m) // Allow for small rounding differences
            {
                _logger.LogWarning("Remaining amount after distribution: {RemainingAmount} {Currency}", remainingAmount, request.Currency);
                
                // Add remainder to the first distribution (treasury typically)
                var firstDistribution = distributions.FirstOrDefault();
                if (firstDistribution != null)
                {
                    firstDistribution.Amount += remainingAmount;
                    _logger.LogDebug("Added remainder {RemainingAmount} to {RecipientType}", remainingAmount, firstDistribution.RecipientType);
                }
            }

            // Save distributions
            _context.FeeDistributions.AddRange(distributions);
            await _context.SaveChangesAsync();

            // Process distributions
            var distributionResponses = new List<FeeDistributionResponse>();
            foreach (var distribution in distributions)
            {
                var processResult = await ProcessDistributionAsync(distribution);
                
                distribution.Status = processResult.Success ? "Completed" : "Failed";
                distribution.ProcessedAt = DateTime.UtcNow;
                distribution.FailureReason = processResult.FailureReason;
                distribution.TransactionHash = processResult.TransactionHash;
                distribution.UpdatedAt = DateTime.UtcNow;

                var response = distribution.Adapt<FeeDistributionResponse>();
                response.DistributionRule = distributionRules.First(r => r.Id == distribution.DistributionRuleId);
                distributionResponses.Add(response);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Distributed {DistributionCount} fee distributions for transaction {TransactionId}, total {TotalAmount} {Currency}", 
                distributions.Count, request.TransactionId, request.TotalAmount, request.Currency);

            return distributionResponses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error distributing fees for transaction {TransactionId}", request.TransactionId);
            throw;
        }
    }

    public async Task<DistributionRuleResponse> CreateOrUpdateDistributionRuleAsync(CreateDistributionRuleRequest request)
    {
        try
        {
            _logger.LogInformation("Creating/updating distribution rule for {FeeType}, recipient {RecipientType} {RecipientId}", 
                request.FeeType, request.RecipientType, request.RecipientId);

            // Validate the rule
            await ValidateDistributionRuleAsync(request);

            // Check if rule already exists
            var existingRule = await _context.DistributionRules
                .Where(dr => dr.FeeType == request.FeeType && 
                            dr.RecipientType == request.RecipientType &&
                            dr.RecipientId == request.RecipientId &&
                            dr.IsActive)
                .FirstOrDefaultAsync();

            DistributionRule distributionRule;

            if (existingRule != null)
            {
                // Update existing rule
                existingRule.Percentage = request.Percentage;
                existingRule.Priority = request.Priority;
                existingRule.EffectiveFrom = request.EffectiveFrom;
                existingRule.EffectiveUntil = request.EffectiveUntil;
                existingRule.Description = request.Description;
                existingRule.Conditions = request.Conditions != null ? System.Text.Json.JsonSerializer.Serialize(request.Conditions) : null;
                existingRule.MinimumAmount = request.MinimumAmount;
                existingRule.MaximumAmount = request.MaximumAmount;
                existingRule.Currency = request.Currency;
                existingRule.UpdatedBy = request.CreatedBy;
                existingRule.UpdatedAt = DateTime.UtcNow;

                distributionRule = existingRule;
            }
            else
            {
                // Create new rule
                distributionRule = new DistributionRule
                {
                    Id = Guid.NewGuid(),
                    FeeType = request.FeeType,
                    RecipientType = request.RecipientType,
                    RecipientId = request.RecipientId,
                    Percentage = request.Percentage,
                    Priority = request.Priority,
                    IsActive = true,
                    EffectiveFrom = request.EffectiveFrom,
                    EffectiveUntil = request.EffectiveUntil,
                    Description = request.Description,
                    Conditions = request.Conditions != null ? System.Text.Json.JsonSerializer.Serialize(request.Conditions) : null,
                    MinimumAmount = request.MinimumAmount,
                    MaximumAmount = request.MaximumAmount,
                    Currency = request.Currency,
                    CreatedBy = request.CreatedBy,
                    UpdatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.DistributionRules.Add(distributionRule);
            }

            await _context.SaveChangesAsync();

            var response = distributionRule.Adapt<DistributionRuleResponse>();
            _logger.LogInformation("Distribution rule created/updated for {FeeType} with ID {Id}", request.FeeType, distributionRule.Id);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating distribution rule for {FeeType}", request.FeeType);
            throw;
        }
    }

    public async Task<IEnumerable<DistributionRuleResponse>> GetDistributionRulesAsync(string feeType, bool activeOnly = true)
    {
        try
        {
            var query = _context.DistributionRules
                .Where(dr => dr.FeeType == feeType);

            if (activeOnly)
            {
                query = query.Where(dr => dr.IsActive && 
                                         dr.EffectiveFrom <= DateTime.UtcNow &&
                                         (dr.EffectiveUntil == null || dr.EffectiveUntil > DateTime.UtcNow));
            }

            var rules = await query
                .OrderBy(dr => dr.Priority)
                .ThenBy(dr => dr.CreatedAt)
                .ToListAsync();

            return rules.Adapt<IEnumerable<DistributionRuleResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distribution rules for {FeeType}", feeType);
            throw;
        }
    }

    public async Task<IEnumerable<FeeDistributionResponse>> GetDistributionHistoryAsync(Guid transactionId)
    {
        try
        {
            var distributions = await _context.FeeDistributions
                .Where(fd => fd.FeeTransactionId == transactionId)
                .Include(fd => fd.DistributionRule)
                .Include(fd => fd.FeeTransaction)
                .OrderBy(fd => fd.CreatedAt)
                .ToListAsync();

            return distributions.Adapt<IEnumerable<FeeDistributionResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distribution history for transaction {TransactionId}", transactionId);
            throw;
        }
    }

    public async Task<SettlementResponse> ProcessSettlementAsync(ProcessSettlementRequest request)
    {
        try
        {
            _logger.LogInformation("Processing settlement for {DistributionCount} distributions using {SettlementMethod}", 
                request.DistributionIds.Count(), request.SettlementMethod);

            // Get distributions
            var distributions = await _context.FeeDistributions
                .Where(fd => request.DistributionIds.Contains(fd.Id))
                .Include(fd => fd.DistributionRule)
                .ToListAsync();

            if (distributions.Count != request.DistributionIds.Count())
            {
                throw new InvalidOperationException("Some distributions were not found");
            }

            // Validate distributions can be settled
            var invalidDistributions = distributions.Where(d => d.Status != "Completed").ToList();
            if (invalidDistributions.Any())
            {
                throw new InvalidOperationException($"Cannot settle distributions with non-completed status: {string.Join(", ", invalidDistributions.Select(d => d.Id))}");
            }

            var totalAmount = distributions.Sum(d => d.Amount);
            var currency = distributions.First().Currency;

            // Ensure all distributions use the same currency
            if (distributions.Any(d => d.Currency != currency))
            {
                throw new InvalidOperationException("All distributions must use the same currency for settlement");
            }

            // Create settlement record
            var settlement = new Data.Entities.Settlement
            {
                Id = Guid.NewGuid(),
                DistributionIds = System.Text.Json.JsonSerializer.Serialize(request.DistributionIds),
                SettlementMethod = request.SettlementMethod,
                SettlementReference = request.SettlementReference,
                Status = "Pending",
                TotalAmount = totalAmount,
                Currency = currency,
                ProcessedBy = request.ProcessedBy,
                Notes = request.Notes,
                Metadata = request.Metadata != null ? System.Text.Json.JsonSerializer.Serialize(request.Metadata) : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Settlements.Add(settlement);
            await _context.SaveChangesAsync();

            // Process settlement
            var settlementResult = await ProcessSettlementTransactionAsync(settlement, distributions);

            // Update settlement status
            settlement.Status = settlementResult.Success ? "Completed" : "Failed";
            settlement.ProcessedAt = DateTime.UtcNow;
            settlement.FailureReason = settlementResult.FailureReason;
            settlement.SettlementReference = settlementResult.SettlementReference ?? settlement.SettlementReference;
            settlement.UpdatedAt = DateTime.UtcNow;

            // Update distribution statuses
            foreach (var distribution in distributions)
            {
                distribution.Status = settlementResult.Success ? "Settled" : "Failed";
                distribution.UpdatedAt = DateTime.UtcNow;
                if (!settlementResult.Success)
                {
                    distribution.FailureReason = settlementResult.FailureReason;
                }
            }

            await _context.SaveChangesAsync();

            var response = new SettlementResponse
            {
                Id = settlement.Id,
                DistributionIds = request.DistributionIds,
                SettlementMethod = settlement.SettlementMethod,
                SettlementReference = settlement.SettlementReference,
                Status = settlement.Status,
                TotalAmount = settlement.TotalAmount,
                Currency = settlement.Currency,
                CreatedAt = settlement.CreatedAt,
                UpdatedAt = settlement.UpdatedAt,
                ProcessedAt = settlement.ProcessedAt,
                ProcessedBy = settlement.ProcessedBy,
                Notes = settlement.Notes,
                FailureReason = settlement.FailureReason,
                Metadata = settlement.Metadata,
                Distributions = distributions.Adapt<IEnumerable<FeeDistributionResponse>>()
            };

            _logger.LogInformation("Settlement {Status} for {DistributionCount} distributions, total {TotalAmount} {Currency}", 
                settlement.Status, distributions.Count, totalAmount, currency);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing settlement");
            throw;
        }
    }

    public async Task<IEnumerable<FeeDistributionResponse>> GetPendingDistributionsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            
            var distributions = await _context.FeeDistributions
                .Where(fd => fd.Status == "Pending" || fd.Status == "Completed")
                .OrderBy(fd => fd.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(fd => fd.DistributionRule)
                .Include(fd => fd.FeeTransaction)
                .ToListAsync();

            return distributions.Adapt<IEnumerable<FeeDistributionResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending distributions");
            throw;
        }
    }

    public async Task<FeeDistributionResponse> UpdateDistributionStatusAsync(Guid distributionId, string status, string? reason = null)
    {
        try
        {
            var distribution = await _context.FeeDistributions
                .Include(fd => fd.DistributionRule)
                .Include(fd => fd.FeeTransaction)
                .FirstOrDefaultAsync(fd => fd.Id == distributionId);

            if (distribution == null)
            {
                throw new InvalidOperationException($"Distribution not found: {distributionId}");
            }

            var oldStatus = distribution.Status;
            distribution.Status = status;
            distribution.UpdatedAt = DateTime.UtcNow;

            if (status == "Failed" && !string.IsNullOrEmpty(reason))
            {
                distribution.FailureReason = reason;
            }

            if (status == "Completed" && oldStatus == "Pending")
            {
                distribution.ProcessedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var response = distribution.Adapt<FeeDistributionResponse>();
            response.DistributionRule = distribution.DistributionRule?.Adapt<DistributionRuleResponse>();

            _logger.LogInformation("Distribution {DistributionId} status updated from {OldStatus} to {NewStatus}", 
                distributionId, oldStatus, status);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating distribution status for {DistributionId}", distributionId);
            throw;
        }
    }

    public async Task<bool> ValidateDistributionRulesAsync(string feeType)
    {
        try
        {
            var rules = await GetDistributionRulesAsync(feeType, true);
            var totalPercentage = rules.Sum(r => r.Percentage);

            // Allow for small rounding differences
            var isValid = Math.Abs(totalPercentage - 100) <= 0.01m;

            if (!isValid)
            {
                _logger.LogWarning("Distribution rules validation failed for {FeeType}: total percentage is {TotalPercentage}%", 
                    feeType, totalPercentage);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating distribution rules for {FeeType}", feeType);
            return false;
        }
    }

    public async Task<DistributionStatisticsResponse> GetDistributionStatisticsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        string? feeType = null)
    {
        try
        {
            var query = _context.FeeDistributions
                .Where(fd => fd.CreatedAt >= fromDate && fd.CreatedAt <= toDate);

            if (!string.IsNullOrEmpty(feeType))
            {
                query = query.Where(fd => fd.FeeTransaction != null && fd.FeeTransaction.FeeConfiguration != null && 
                                         fd.FeeTransaction.FeeConfiguration.FeeType == feeType);
            }

            var distributions = await query
                .Include(fd => fd.FeeTransaction)
                .ThenInclude(ft => ft!.FeeConfiguration)
                .ToListAsync();

            var totalDistributed = distributions.Where(d => d.Status == "Completed" || d.Status == "Settled").Sum(d => d.Amount);
            var currency = distributions.FirstOrDefault()?.Currency ?? "USD";

            var recipientBreakdown = distributions
                .Where(d => d.Status == "Completed" || d.Status == "Settled")
                .GroupBy(d => new { d.RecipientType, d.RecipientId })
                .Select(g => new RecipientStatistics
                {
                    RecipientType = g.Key.RecipientType,
                    RecipientId = g.Key.RecipientId,
                    TotalAmount = g.Sum(d => d.Amount),
                    DistributionCount = g.Count(),
                    Percentage = totalDistributed > 0 ? (g.Sum(d => d.Amount) / totalDistributed) * 100 : 0
                })
                .OrderByDescending(r => r.TotalAmount)
                .ToList();

            var feeTypeBreakdown = distributions
                .Where(d => d.Status == "Completed" || d.Status == "Settled" && d.FeeTransaction?.FeeConfiguration != null)
                .GroupBy(d => d.FeeTransaction!.FeeConfiguration!.FeeType)
                .Select(g => new FeeTypeStatistics
                {
                    FeeType = g.Key,
                    TotalAmount = g.Sum(d => d.Amount),
                    DistributionCount = g.Count(),
                    Percentage = totalDistributed > 0 ? (g.Sum(d => d.Amount) / totalDistributed) * 100 : 0
                })
                .OrderByDescending(f => f.TotalAmount)
                .ToList();

            return new DistributionStatisticsResponse
            {
                FromDate = fromDate,
                ToDate = toDate,
                FeeType = feeType,
                TotalDistributed = totalDistributed,
                Currency = currency,
                TotalDistributions = distributions.Count,
                CompletedDistributions = distributions.Count(d => d.Status == "Completed" || d.Status == "Settled"),
                PendingDistributions = distributions.Count(d => d.Status == "Pending"),
                FailedDistributions = distributions.Count(d => d.Status == "Failed"),
                AverageDistributionAmount = distributions.Any() ? distributions.Average(d => d.Amount) : 0,
                RecipientBreakdown = recipientBreakdown,
                FeeTypeBreakdown = feeTypeBreakdown,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distribution statistics");
            throw;
        }
    }

    private decimal CalculateDistributionAmount(DistributionRuleResponse rule, decimal totalAmount, decimal remainingAmount)
    {
        var calculatedAmount = totalAmount * (rule.Percentage / 100);
        
        // Apply min/max limits if specified
        if (rule.MinimumAmount.HasValue && calculatedAmount < rule.MinimumAmount.Value)
        {
            calculatedAmount = rule.MinimumAmount.Value;
        }

        if (rule.MaximumAmount.HasValue && calculatedAmount > rule.MaximumAmount.Value)
        {
            calculatedAmount = rule.MaximumAmount.Value;
        }

        // Don't exceed remaining amount
        return Math.Min(calculatedAmount, remainingAmount);
    }

    private async Task<DistributionResult> ProcessDistributionAsync(FeeDistribution distribution)
    {
        try
        {
            if (_mockMode)
            {
                // Mock distribution processing
                await Task.Delay(50); // Simulate processing time
                
                // 97% success rate in mock mode
                var success = Random.Shared.NextDouble() > 0.03;
                
                return new DistributionResult
                {
                    Success = success,
                    TransactionHash = success ? $"0x{Guid.NewGuid():N}[..16]" : null,
                    FailureReason = success ? null : "Mock distribution failure for testing"
                };
            }

            // Real distribution processing would go here
            // This could involve blockchain transactions, bank transfers, etc.
            
            _logger.LogInformation("Processing real distribution for {RecipientType} {RecipientId}", 
                distribution.RecipientType, distribution.RecipientId);
            
            // Placeholder for real distribution processing
            return new DistributionResult
            {
                Success = true,
                TransactionHash = $"0x{Guid.NewGuid():N}[..16]"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing distribution {DistributionId}", distribution.Id);
            return new DistributionResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }

    private async Task<SettlementResult> ProcessSettlementTransactionAsync(Data.Entities.Settlement settlement, List<FeeDistribution> distributions)
    {
        try
        {
            if (_mockMode)
            {
                // Mock settlement processing
                await Task.Delay(200); // Simulate processing time
                
                // 99% success rate for settlements in mock mode
                var success = Random.Shared.NextDouble() > 0.01;
                
                return new SettlementResult
                {
                    Success = success,
                    SettlementReference = success ? $"SETTLE_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid():N}[..8]" : null,
                    FailureReason = success ? null : "Mock settlement failure for testing"
                };
            }

            // Real settlement processing would go here
            _logger.LogInformation("Processing real settlement {SettlementId} using {SettlementMethod}", 
                settlement.Id, settlement.SettlementMethod);
            
            // Placeholder for real settlement processing
            return new SettlementResult
            {
                Success = true,
                SettlementReference = $"SETTLE_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid():N}[..8]"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing settlement {SettlementId}", settlement.Id);
            return new SettlementResult
            {
                Success = false,
                FailureReason = ex.Message
            };
        }
    }

    private async Task ValidateDistributionRuleAsync(CreateDistributionRuleRequest request)
    {
        if (request.Percentage <= 0 || request.Percentage > 100)
        {
            throw new ArgumentException("Percentage must be between 0 and 100");
        }

        if (request.MinimumAmount.HasValue && request.MaximumAmount.HasValue && 
            request.MinimumAmount > request.MaximumAmount)
        {
            throw new ArgumentException("Minimum amount cannot be greater than maximum amount");
        }

        // Check if total percentage would exceed 100%
        var existingRules = await GetDistributionRulesAsync(request.FeeType, true);
        var totalPercentage = existingRules
            .Where(r => !(r.RecipientType == request.RecipientType && r.RecipientId == request.RecipientId))
            .Sum(r => r.Percentage) + request.Percentage;

        if (totalPercentage > 100.01m) // Allow for small rounding differences
        {
            throw new ArgumentException($"Total percentage would exceed 100%: {totalPercentage}%");
        }
    }

    private class DistributionResult
    {
        public bool Success { get; set; }
        public string? TransactionHash { get; set; }
        public string? FailureReason { get; set; }
    }

    private class SettlementResult
    {
        public bool Success { get; set; }
        public string? SettlementReference { get; set; }
        public string? FailureReason { get; set; }
    }

}
