using AccountService.Data;
using AccountService.Data.Entities;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace AccountService.Services;

public class AccountLimitService : IAccountLimitService
{
    private readonly AccountDbContext _context;
    private readonly ILogger<AccountLimitService> _logger;

    public AccountLimitService(
        AccountDbContext context,
        ILogger<AccountLimitService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AccountLimitResponse> CreateLimitAsync(CreateAccountLimitRequest request, CancellationToken cancellationToken = default)
    {
        var limit = new AccountLimit
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            LimitType = Enum.Parse<LimitType>(request.LimitType),
            LimitAmount = request.LimitAmount,
            UsedAmount = 0,
            Period = Enum.Parse<LimitPeriod>(request.Period),
            IsActive = true,
            Description = request.Description,
            SetBy = request.SetBy?.ToString(),
            Reason = request.Reason,
            EffectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow,
            EffectiveTo = request.EffectiveTo,
            LastResetAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AccountLimits.Add(limit);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account limit created: {LimitId} for account {AccountId}", limit.Id, request.AccountId);

        return limit.Adapt<AccountLimitResponse>();
    }

    public async Task<AccountLimitResponse> GetLimitAsync(Guid limitId, CancellationToken cancellationToken = default)
    {
        var limit = await _context.AccountLimits
            .Include(l => l.Account)
            .FirstOrDefaultAsync(l => l.Id == limitId, cancellationToken);

        if (limit == null)
        {
            throw new InvalidOperationException($"Limit with ID {limitId} not found");
        }

        return limit.Adapt<AccountLimitResponse>();
    }

    public async Task<IEnumerable<AccountLimitResponse>> GetAccountLimitsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var limits = await _context.AccountLimits
            .Where(l => l.AccountId == accountId && l.IsActive)
            .OrderBy(l => l.LimitType)
            .ToListAsync(cancellationToken);

        return limits.Adapt<IEnumerable<AccountLimitResponse>>();
    }

    public async Task<AccountLimitResponse?> GetLimitByTypeAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default)
    {
        var limit = await _context.AccountLimits
            .Where(l => l.AccountId == accountId && l.LimitType == limitType && l.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        return limit?.Adapt<AccountLimitResponse>();
    }

    public async Task<AccountLimitResponse> UpdateLimitAsync(Guid limitId, UpdateAccountLimitRequest request, CancellationToken cancellationToken = default)
    {
        var limit = await _context.AccountLimits
            .FirstOrDefaultAsync(l => l.Id == limitId, cancellationToken);

        if (limit == null)
        {
            throw new InvalidOperationException($"Limit with ID {limitId} not found");
        }

        limit.LimitAmount = request.LimitAmount;
        limit.Description = request.Description ?? limit.Description;
        limit.Reason = request.Reason ?? limit.Reason;
        limit.EffectiveTo = request.EffectiveTo;
        limit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account limit updated: {LimitId}", limitId);

        return limit.Adapt<AccountLimitResponse>();
    }

    public async Task<bool> DeleteLimitAsync(Guid limitId, CancellationToken cancellationToken = default)
    {
        var limit = await _context.AccountLimits
            .FirstOrDefaultAsync(l => l.Id == limitId, cancellationToken);

        if (limit == null)
        {
            return false;
        }

        limit.IsActive = false;
        limit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account limit deactivated: {LimitId}", limitId);

        return true;
    }

    public async Task<bool> LimitExistsAsync(Guid limitId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountLimits
            .AnyAsync(l => l.Id == limitId, cancellationToken);
    }

    public async Task<bool> UserOwnsLimitAsync(Guid userId, Guid limitId, CancellationToken cancellationToken = default)
    {
        return await _context.AccountLimits
            .Include(l => l.Account)
            .AnyAsync(l => l.Id == limitId && l.Account.UserId == userId, cancellationToken);
    }

    public async Task<bool> ValidateTransactionAgainstLimitsAsync(Guid accountId, decimal amount, TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        var limitValidation = await CheckTransactionLimitsAsync(accountId, amount, transactionType, cancellationToken);
        return limitValidation.IsValid;
    }

    public async Task<LimitValidationResponse> CheckTransactionLimitsAsync(Guid accountId, decimal amount, TransactionType transactionType, CancellationToken cancellationToken = default)
    {
        var relevantLimitTypes = GetRelevantLimitTypes(transactionType);
        var violations = new List<string>();

        foreach (var limitType in relevantLimitTypes)
        {
            var limit = await GetLimitByTypeAsync(accountId, limitType, cancellationToken);
            if (limit == null) continue;

            var currentUsage = await GetCurrentUsageAsync(accountId, limitType, cancellationToken);
            var projectedUsage = currentUsage + amount;

            if (projectedUsage > limit.LimitAmount)
            {
                violations.Add($"{limitType} limit exceeded: {projectedUsage:C} > {limit.LimitAmount:C}");
            }
        }

        return new LimitValidationResponse
        {
            IsValid = violations.Count == 0,
            Violations = violations,
            CheckedAt = DateTime.UtcNow
        };
    }

    public async Task<decimal> GetCurrentUsageAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default)
    {
        // Work directly with entity instead of response model to avoid mapping issues
        var limitEntity = await _context.AccountLimits
            .Where(l => l.AccountId == accountId && l.LimitType == limitType && l.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (limitEntity == null) return 0;

        var (startDate, endDate) = GetPeriodDates(limitEntity.Period, limitEntity.LastResetAt);

        var transactionTypes = GetTransactionTypesForLimit(limitType);
        
        // Use database aggregation to avoid EF Core LINQ translation issues
        var hasTransactions = await _context.AccountTransactions
            .Where(t => t.AccountId == accountId 
                && transactionTypes.Contains(t.TransactionType)
                && t.Status == TransactionStatus.Completed
                && t.Timestamp >= startDate 
                && t.Timestamp <= endDate)
            .AnyAsync(cancellationToken);

        if (!hasTransactions)
            return 0m;

        // Use ToListAsync then Sum to avoid EF Core nullable decimal aggregation issues
        var transactions = await _context.AccountTransactions
            .Where(t => t.AccountId == accountId 
                && transactionTypes.Contains(t.TransactionType)
                && t.Status == TransactionStatus.Completed
                && t.Timestamp >= startDate 
                && t.Timestamp <= endDate)
            .Select(t => t.Amount)
            .ToListAsync(cancellationToken);

        return transactions.Sum();
    }

    public async Task<decimal> GetRemainingLimitAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default)
    {
        var limit = await GetLimitByTypeAsync(accountId, limitType, cancellationToken);
        if (limit == null) return 0;

        var currentUsage = await GetCurrentUsageAsync(accountId, limitType, cancellationToken);
        return Math.Max(0, limit.LimitAmount - currentUsage);
    }

    public async Task<LimitUsageResponse> GetLimitUsageAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default)
    {
        var limit = await GetLimitByTypeAsync(accountId, limitType, cancellationToken);
        if (limit == null)
        {
        return new LimitUsageResponse
        {
            AccountId = accountId,
            LimitType = limitType.ToString(),
            LimitAmount = 0,
            UsedAmount = 0,
            RemainingAmount = 0,
            UsagePercentage = 0
        };
        }

        var currentUsage = await GetCurrentUsageAsync(accountId, limitType, cancellationToken);
        var remaining = Math.Max(0, limit.LimitAmount - currentUsage);
        var percentage = limit.LimitAmount > 0 ? (currentUsage / limit.LimitAmount) * 100 : 0;

        return new LimitUsageResponse
        {
            AccountId = accountId,
            LimitType = limitType.ToString(),
            LimitAmount = limit.LimitAmount,
            UsedAmount = currentUsage,
            RemainingAmount = remaining,
            UsagePercentage = percentage
        };
    }

    public async Task<IEnumerable<LimitUsageResponse>> GetAllLimitUsageAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var limits = await GetAccountLimitsAsync(accountId, cancellationToken);
        var usageResponses = new List<LimitUsageResponse>();

        foreach (var limit in limits)
        {
            var usage = await GetLimitUsageAsync(accountId, Enum.Parse<LimitType>(limit.LimitType), cancellationToken);
            usageResponses.Add(usage);
        }

        return usageResponses;
    }

    public async Task<bool> ResetLimitUsageAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default)
    {
        var limit = await _context.AccountLimits
            .FirstOrDefaultAsync(l => l.AccountId == accountId && l.LimitType == limitType && l.IsActive, cancellationToken);

        if (limit == null)
        {
            return false;
        }

        limit.UsedAmount = 0;
        limit.LastResetAt = DateTime.UtcNow;
        limit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Limit usage reset: {LimitType} for account {AccountId}", limitType, accountId);

        return true;
    }

    public async Task<bool> ResetAllLimitUsageAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var limits = await _context.AccountLimits
            .Where(l => l.AccountId == accountId && l.IsActive)
            .ToListAsync(cancellationToken);

        if (limits.Count == 0)
        {
            return false;
        }

        foreach (var limit in limits)
        {
            limit.UsedAmount = 0;
            limit.LastResetAt = DateTime.UtcNow;
            limit.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("All limit usage reset for account {AccountId}", accountId);

        return true;
    }

    public async Task<AccountLimitResponse> IncreaseLimitAsync(Guid limitId, decimal newAmount, string reason, CancellationToken cancellationToken = default)
    {
        var limit = await _context.AccountLimits
            .FirstOrDefaultAsync(l => l.Id == limitId, cancellationToken);

        if (limit == null)
        {
            throw new InvalidOperationException($"Limit with ID {limitId} not found");
        }

        var oldAmount = limit.LimitAmount;
        limit.LimitAmount = newAmount;
        limit.Reason = reason;
        limit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Limit increased: {LimitId} from {OldAmount} to {NewAmount}, Reason: {Reason}", 
            limitId, oldAmount, newAmount, reason);

        return limit.Adapt<AccountLimitResponse>();
    }

    public async Task<AccountLimitResponse> DecreaseLimitAsync(Guid limitId, decimal newAmount, string reason, CancellationToken cancellationToken = default)
    {
        var limit = await _context.AccountLimits
            .FirstOrDefaultAsync(l => l.Id == limitId, cancellationToken);

        if (limit == null)
        {
            throw new InvalidOperationException($"Limit with ID {limitId} not found");
        }

        var oldAmount = limit.LimitAmount;
        limit.LimitAmount = newAmount;
        limit.Reason = reason;
        limit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Limit decreased: {LimitId} from {OldAmount} to {NewAmount}, Reason: {Reason}", 
            limitId, oldAmount, newAmount, reason);

        return limit.Adapt<AccountLimitResponse>();
    }

    public async Task<IEnumerable<LimitAlertResponse>> GetLimitAlertsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var alerts = new List<LimitAlertResponse>();
        var limits = await GetAccountLimitsAsync(accountId, cancellationToken);

        foreach (var limit in limits)
        {
            var usage = await GetLimitUsageAsync(accountId, Enum.Parse<LimitType>(limit.LimitType), cancellationToken);
            
            if (usage.UsagePercentage >= 80) // Alert at 80% usage
            {
                alerts.Add(new LimitAlertResponse
                {
                    AccountId = accountId,
                    LimitType = limit.LimitType,
                    UsagePercentage = usage.UsagePercentage,
                    Severity = usage.UsagePercentage >= 95 ? "Critical" : "Warning",
                    Message = $"{limit.LimitType} limit at {usage.UsagePercentage:F1}% usage",
                    AlertedAt = DateTime.UtcNow
                });
            }
        }

        return alerts;
    }

    public async Task<bool> IsLimitExceededAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default)
    {
        var usage = await GetLimitUsageAsync(accountId, limitType, cancellationToken);
        return usage.UsedAmount >= usage.LimitAmount;
    }

    public async Task<bool> IsLimitNearExceededAsync(Guid accountId, LimitType limitType, decimal threshold = 0.8m, CancellationToken cancellationToken = default)
    {
        var usage = await GetLimitUsageAsync(accountId, limitType, cancellationToken);
        return usage.UsagePercentage >= (threshold * 100);
    }

    public async Task<IEnumerable<AccountLimitResponse>> CreateDefaultLimitsAsync(Guid accountId, AccountType accountType, CancellationToken cancellationToken = default)
    {
        var defaultLimits = GetDefaultLimitsForAccountType(accountType);
        var createdLimits = new List<AccountLimitResponse>();

        foreach (var defaultLimit in defaultLimits)
        {
            var request = new CreateAccountLimitRequest
            {
                AccountId = accountId,
                LimitType = defaultLimit.LimitType,
                LimitAmount = defaultLimit.Amount,
                Period = defaultLimit.Period,
                Description = defaultLimit.Description,
                SetBy = Guid.NewGuid(),
                Reason = "Default limit for account type"
            };

            var limit = await CreateLimitAsync(request, cancellationToken);
            createdLimits.Add(limit);
        }

        _logger.LogInformation("Default limits created for account {AccountId}, type {AccountType}", accountId, accountType);

        return createdLimits;
    }

    public async Task<IEnumerable<DefaultLimitResponse>> GetDefaultLimitsForAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        return GetDefaultLimitsForAccountType(accountType);
    }

    public async Task<bool> ApplyDefaultLimitsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account == null)
        {
            return false;
        }

        await CreateDefaultLimitsAsync(accountId, account.AccountType, cancellationToken);
        return true;
    }

    public async Task<IEnumerable<LimitHistoryResponse>> GetLimitHistoryAsync(Guid accountId, LimitType? limitType = null, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = _context.AccountLimits
            .Where(l => l.AccountId == accountId);

        if (limitType.HasValue)
        {
            query = query.Where(l => l.LimitType == limitType.Value);
        }

        var limits = await query
            .OrderByDescending(l => l.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return limits.Select(l => new LimitHistoryResponse
        {
            LimitId = l.Id,
            AccountId = accountId,
            LimitType = l.LimitType.ToString(),
            LimitAmount = l.LimitAmount,
            SetBy = string.IsNullOrEmpty(l.SetBy) ? null : Guid.TryParse(l.SetBy, out var setByGuid) ? setByGuid : null,
            Reason = l.Reason,
            EffectiveFrom = l.EffectiveFrom,
            EffectiveTo = l.EffectiveTo,
            CreatedAt = l.CreatedAt
        });
    }

    public async Task<LimitStatisticsResponse> GetLimitStatisticsAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var limits = await GetAccountLimitsAsync(accountId, cancellationToken);
        var statistics = new LimitStatisticsResponse
        {
            AccountId = accountId,
            TotalLimits = limits.Count(),
            ActiveLimits = limits.Count(l => l.IsActive),
            GeneratedAt = DateTime.UtcNow
        };

        var usageData = new List<LimitUsageStatistic>();
        foreach (var limit in limits)
        {
            var usage = await GetLimitUsageAsync(accountId, Enum.Parse<LimitType>(limit.LimitType), cancellationToken);
            usageData.Add(new LimitUsageStatistic
            {
                LimitType = limit.LimitType.ToString(),
                UsagePercentage = usage.UsagePercentage,
                IsNearLimit = usage.UsagePercentage >= 80
            });
        }

        statistics.UsageStatistics = usageData;
        return statistics;
    }

    // Additional methods expected by controllers
    public async Task<IEnumerable<AccountLimitResponse>> GetUserLimitsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var limits = await _context.AccountLimits
            .Include(l => l.Account)
            .Where(l => l.Account.UserId == userId && l.IsActive)
            .OrderBy(l => l.LimitType)
            .ToListAsync(cancellationToken);

        return limits.Adapt<IEnumerable<AccountLimitResponse>>();
    }

    public async Task<LimitValidationResponse> ValidateLimitAsync(Guid accountId, decimal amount, LimitType limitType, CancellationToken cancellationToken = default)
    {
        var limit = await GetLimitByTypeAsync(accountId, limitType, cancellationToken);
        if (limit == null)
        {
            return new LimitValidationResponse
            {
                IsValid = true,
                Violations = new List<string>(),
                CheckedAt = DateTime.UtcNow
            };
        }

        var currentUsage = await GetCurrentUsageAsync(accountId, limitType, cancellationToken);
        var projectedUsage = currentUsage + amount;
        var violations = new List<string>();

        if (projectedUsage > limit.LimitAmount)
        {
            violations.Add($"{limitType} limit exceeded: {projectedUsage:C} > {limit.LimitAmount:C}");
        }

        return new LimitValidationResponse
        {
            IsValid = violations.Count == 0,
            Violations = violations,
            CheckedAt = DateTime.UtcNow
        };
    }

    public async Task<ComplianceCheckResponse> CheckLimitAsync(Guid accountId, LimitType limitType, CancellationToken cancellationToken = default)
    {
        var usage = await GetLimitUsageAsync(accountId, limitType, cancellationToken);
        var isCompliant = usage.UsagePercentage < 100;
        var issues = new List<string>();

        if (!isCompliant)
        {
            issues.Add($"{limitType} limit exceeded");
        }
        else if (usage.UsagePercentage >= 80)
        {
            issues.Add($"{limitType} limit near threshold");
        }

        return new ComplianceCheckResponse
        {
            IsCompliant = isCompliant,
            Issues = issues
        };
    }

    public async Task<IEnumerable<LimitUsageResponse>> GetRemainingLimitsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await GetAllLimitUsageAsync(accountId, cancellationToken);
    }

    public async Task<bool> UpdateUsageAsync(Guid accountId, UpdateUsageRequest request, CancellationToken cancellationToken = default)
    {
        var limitType = Enum.Parse<LimitType>(request.LimitType);
        var limit = await _context.AccountLimits
            .FirstOrDefaultAsync(l => l.AccountId == accountId && l.LimitType == limitType && l.IsActive, cancellationToken);

        if (limit == null)
        {
            return false;
        }

        limit.UsedAmount += request.Amount.GetValueOrDefault(0);
        limit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Limit usage updated: {LimitType} for account {AccountId}, amount: {Amount}", 
            limitType, accountId, request.Amount);

        return true;
    }

    public async Task<IEnumerable<DefaultLimitResponse>> GetDefaultLimitRequirementsAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        return await GetDefaultLimitsForAccountTypeAsync(accountType, cancellationToken);
    }

    private static List<LimitType> GetRelevantLimitTypes(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.Withdrawal => new List<LimitType> { LimitType.DailyWithdrawal, LimitType.WeeklyWithdrawal, LimitType.MonthlyWithdrawal, LimitType.SingleTransaction },
            TransactionType.Deposit => new List<LimitType> { LimitType.DailyDeposit, LimitType.WeeklyDeposit, LimitType.MonthlyDeposit, LimitType.SingleTransaction },
            TransactionType.Transfer => new List<LimitType> { LimitType.DailyTransfer, LimitType.WeeklyTransfer, LimitType.MonthlyTransfer, LimitType.SingleTransaction },
            _ => new List<LimitType> { LimitType.SingleTransaction }
        };
    }

    private static List<TransactionType> GetTransactionTypesForLimit(LimitType limitType)
    {
        return limitType switch
        {
            LimitType.DailyWithdrawal or LimitType.WeeklyWithdrawal or LimitType.MonthlyWithdrawal => new List<TransactionType> { TransactionType.Withdrawal },
            LimitType.DailyDeposit or LimitType.WeeklyDeposit or LimitType.MonthlyDeposit => new List<TransactionType> { TransactionType.Deposit },
            LimitType.DailyTransfer or LimitType.WeeklyTransfer or LimitType.MonthlyTransfer => new List<TransactionType> { TransactionType.Transfer },
            LimitType.SingleTransaction => new List<TransactionType> { TransactionType.Withdrawal, TransactionType.Deposit, TransactionType.Transfer },
            _ => new List<TransactionType>()
        };
    }

    private static (DateTime startDate, DateTime endDate) GetPeriodDates(LimitPeriod period, DateTime lastResetAt)
    {
        var now = DateTime.UtcNow;
        return period switch
        {
            LimitPeriod.Daily => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
            LimitPeriod.Weekly => (now.Date.AddDays(-(int)now.DayOfWeek), now.Date.AddDays(7 - (int)now.DayOfWeek).AddTicks(-1)),
            LimitPeriod.Monthly => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),
            LimitPeriod.Yearly => (new DateTime(now.Year, 1, 1), new DateTime(now.Year + 1, 1, 1).AddTicks(-1)),
            LimitPeriod.PerTransaction => (DateTime.MinValue, DateTime.MaxValue),
            LimitPeriod.Lifetime => (DateTime.MinValue, DateTime.MaxValue),
            _ => (lastResetAt, now)
        };
    }

    private static List<DefaultLimitResponse> GetDefaultLimitsForAccountType(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Individual => new List<DefaultLimitResponse>
            {
                new() { LimitType = LimitType.DailyWithdrawal.ToString(), Amount = 1000, Period = LimitPeriod.Daily.ToString(), Description = "Daily withdrawal limit" },
                new() { LimitType = LimitType.DailyDeposit.ToString(), Amount = 10000, Period = LimitPeriod.Daily.ToString(), Description = "Daily deposit limit" },
                new() { LimitType = LimitType.MonthlyWithdrawal.ToString(), Amount = 25000, Period = LimitPeriod.Monthly.ToString(), Description = "Monthly withdrawal limit" },
                new() { LimitType = LimitType.SingleTransaction.ToString(), Amount = 5000, Period = LimitPeriod.PerTransaction.ToString(), Description = "Single transaction limit" }
            },
            AccountType.Business => new List<DefaultLimitResponse>
            {
                new() { LimitType = LimitType.DailyWithdrawal.ToString(), Amount = 10000, Period = LimitPeriod.Daily.ToString(), Description = "Daily withdrawal limit" },
                new() { LimitType = LimitType.DailyDeposit.ToString(), Amount = 100000, Period = LimitPeriod.Daily.ToString(), Description = "Daily deposit limit" },
                new() { LimitType = LimitType.MonthlyWithdrawal.ToString(), Amount = 250000, Period = LimitPeriod.Monthly.ToString(), Description = "Monthly withdrawal limit" },
                new() { LimitType = LimitType.SingleTransaction.ToString(), Amount = 50000, Period = LimitPeriod.PerTransaction.ToString(), Description = "Single transaction limit" }
            },
            AccountType.Institutional => new List<DefaultLimitResponse>
            {
                new() { LimitType = LimitType.DailyWithdrawal.ToString(), Amount = 100000, Period = LimitPeriod.Daily.ToString(), Description = "Daily withdrawal limit" },
                new() { LimitType = LimitType.DailyDeposit.ToString(), Amount = 1000000, Period = LimitPeriod.Daily.ToString(), Description = "Daily deposit limit" },
                new() { LimitType = LimitType.MonthlyWithdrawal.ToString(), Amount = 2500000, Period = LimitPeriod.Monthly.ToString(), Description = "Monthly withdrawal limit" },
                new() { LimitType = LimitType.SingleTransaction.ToString(), Amount = 500000, Period = LimitPeriod.PerTransaction.ToString(), Description = "Single transaction limit" }
            },
            AccountType.Trading => new List<DefaultLimitResponse>
            {
                new() { LimitType = LimitType.DailyWithdrawal.ToString(), Amount = 5000, Period = LimitPeriod.Daily.ToString(), Description = "Daily withdrawal limit" },
                new() { LimitType = LimitType.DailyDeposit.ToString(), Amount = 50000, Period = LimitPeriod.Daily.ToString(), Description = "Daily deposit limit" },
                new() { LimitType = LimitType.MonthlyWithdrawal.ToString(), Amount = 100000, Period = LimitPeriod.Monthly.ToString(), Description = "Monthly withdrawal limit" },
                new() { LimitType = LimitType.SingleTransaction.ToString(), Amount = 25000, Period = LimitPeriod.PerTransaction.ToString(), Description = "Single transaction limit" }
            },
            _ => new List<DefaultLimitResponse>
            {
                new() { LimitType = LimitType.DailyWithdrawal.ToString(), Amount = 500, Period = LimitPeriod.Daily.ToString(), Description = "Daily withdrawal limit" },
                new() { LimitType = LimitType.DailyDeposit.ToString(), Amount = 5000, Period = LimitPeriod.Daily.ToString(), Description = "Daily deposit limit" },
                new() { LimitType = LimitType.SingleTransaction.ToString(), Amount = 1000, Period = LimitPeriod.PerTransaction.ToString(), Description = "Single transaction limit" }
            }
        };
    }
}
