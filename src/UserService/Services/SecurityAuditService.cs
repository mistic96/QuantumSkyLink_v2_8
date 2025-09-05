using System.Text.Json;
using Mapster;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Data.Entities;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Models.Security;
using UserService.Services.Interfaces;

namespace UserService.Services;

public class SecurityAuditService : ISecurityAuditService
{
    private readonly UserDbContext _context;
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SecurityAuditService(
        UserDbContext context,
        ILogger<SecurityAuditService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogUserActionAsync(Guid userId, string action, string resource, object? details = null, CancellationToken cancellationToken = default)
    {
        await LogSecurityEventAsync(
            SecurityEventType.PermissionGranted,
            $"User action: {action} on {resource}",
            userId,
            GetClientIpAddress(),
            details,
            cancellationToken);
    }

    public async Task LogSecurityEventAsync(SecurityEventType eventType, string description, Guid? userId = null, string? ipAddress = null, object? details = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new Data.Entities.SecurityAuditLog
            {
                UserId = userId,
                EventType = eventType,
                Description = description,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = GetUserAgent(),
                Level = GetAuditLevel(eventType),
                Details = details != null ? JsonSerializer.Serialize(details) : null,
                CorrelationId = GetCorrelationId(),
                SessionId = GetSessionId(),
                PerformedBy = GetCurrentUser(),
                IsSuccessful = IsSuccessfulEvent(eventType)
            };

            _context.SecurityAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            // Log to application logger as well for immediate visibility
            _logger.LogInformation("Security Event: {EventType} - {Description} (User: {UserId}, IP: {IpAddress})",
                eventType, description, userId, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event: {EventType} - {Description}", eventType, description);
            // Don't throw - audit logging should not break the main flow
        }
    }

    public async Task LogRoleChangeAsync(Guid userId, string roleName, RoleChangeType changeType, string? performedBy = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        var eventType = changeType switch
        {
            RoleChangeType.Assigned => SecurityEventType.RoleAssigned,
            RoleChangeType.Removed => SecurityEventType.RoleRemoved,
            RoleChangeType.Expired => SecurityEventType.RoleRemoved,
            RoleChangeType.Upgraded => SecurityEventType.RoleAssigned,
            RoleChangeType.Downgraded => SecurityEventType.RoleRemoved,
            _ => SecurityEventType.RoleAssigned
        };

        var details = new
        {
            RoleName = roleName,
            ChangeType = changeType.ToString(),
            PerformedBy = performedBy,
            Reason = reason
        };

        await LogSecurityEventAsync(
            eventType,
            $"Role {changeType.ToString().ToLower()}: {roleName}",
            userId,
            GetClientIpAddress(),
            details,
            cancellationToken);
    }

    public async Task LogWalletActionAsync(Guid userId, string walletAddress, WalletActionType actionType, object? details = null, CancellationToken cancellationToken = default)
    {
        var eventType = actionType switch
        {
            WalletActionType.Created => SecurityEventType.WalletCreated,
            WalletActionType.Verified => SecurityEventType.WalletVerified,
            WalletActionType.Deactivated => SecurityEventType.WalletDeactivated,
            _ => SecurityEventType.WalletCreated
        };

        var walletDetails = new
        {
            WalletAddress = walletAddress,
            ActionType = actionType.ToString(),
            AdditionalDetails = details
        };

        await LogSecurityEventAsync(
            eventType,
            $"Wallet {actionType.ToString().ToLower()}: {walletAddress}",
            userId,
            GetClientIpAddress(),
            walletDetails,
            cancellationToken);
    }

    public async Task LogPermissionCheckAsync(Guid userId, string resource, string action, bool granted, CancellationToken cancellationToken = default)
    {
        var eventType = granted ? SecurityEventType.PermissionGranted : SecurityEventType.PermissionDenied;
        var details = new
        {
            Resource = resource,
            Action = action,
            Granted = granted
        };

        await LogSecurityEventAsync(
            eventType,
            $"Permission {(granted ? "granted" : "denied")}: {action} on {resource}",
            userId,
            GetClientIpAddress(),
            details,
            cancellationToken);
    }

    public async Task<IEnumerable<Models.Security.SecurityAuditLog>> GetUserAuditLogsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.SecurityAuditLogs
                .Where(log => log.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return logs.Adapt<IEnumerable<Models.Security.SecurityAuditLog>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit logs for user {UserId}", userId);
            return Enumerable.Empty<Models.Security.SecurityAuditLog>();
        }
    }

    public async Task<IEnumerable<Models.Security.SecurityAuditLog>> GetSecurityEventsAsync(SecurityEventType? eventType = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.SecurityAuditLogs.AsQueryable();

            if (eventType.HasValue)
                query = query.Where(log => log.EventType == eventType.Value);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return logs.Adapt<IEnumerable<Models.Security.SecurityAuditLog>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security events");
            return Enumerable.Empty<Models.Security.SecurityAuditLog>();
        }
    }

    public async Task CleanupOldAuditLogsAsync(int retentionDays = 365, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting audit log cleanup for logs older than {RetentionDays} days (CorrelationId: {CorrelationId})", 
            retentionDays, correlationId);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            
            var oldLogs = await _context.SecurityAuditLogs
                .Where(log => log.Timestamp < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldLogs.Any())
            {
                _context.SecurityAuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Cleaned up {Count} old audit logs (CorrelationId: {CorrelationId})", 
                    oldLogs.Count, correlationId);
            }
            else
            {
                _logger.LogInformation("No old audit logs found for cleanup (CorrelationId: {CorrelationId})", correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old audit logs (CorrelationId: {CorrelationId})", correlationId);
            throw;
        }
    }

    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // Check for forwarded IP first (for load balancers/proxies)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers["User-Agent"].FirstOrDefault();
    }

    private string? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? 
               httpContext?.TraceIdentifier;
    }

    private string? GetSessionId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session?.Id;
    }

    private string? GetCurrentUser()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.Identity?.Name;
    }

    private static AuditLogLevel GetAuditLevel(SecurityEventType eventType)
    {
        return eventType switch
        {
            SecurityEventType.FailedLogin => AuditLogLevel.Warning,
            SecurityEventType.AccountLocked => AuditLogLevel.Warning,
            SecurityEventType.SuspiciousActivity => AuditLogLevel.Warning,
            SecurityEventType.ComplianceViolation => AuditLogLevel.Error,
            SecurityEventType.SecurityPolicyViolation => AuditLogLevel.Error,
            SecurityEventType.PermissionDenied => AuditLogLevel.Warning,
            _ => AuditLogLevel.Information
        };
    }

    private static bool IsSuccessfulEvent(SecurityEventType eventType)
    {
        return eventType switch
        {
            SecurityEventType.FailedLogin => false,
            SecurityEventType.PermissionDenied => false,
            SecurityEventType.ComplianceViolation => false,
            SecurityEventType.SecurityPolicyViolation => false,
            SecurityEventType.SuspiciousActivity => false,
            _ => true
        };
    }

    // Missing methods needed by controller
    public async Task<SecurityAuditLogResponse> CreateSecurityAuditLogAsync(CreateSecurityAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating security audit log for event {EventType} (CorrelationId: {CorrelationId})", 
            request.EventType, correlationId);

        try
        {
            // Convert string to SecurityEventType enum
            var eventType = Enum.TryParse<SecurityEventType>(request.EventType, true, out var parsedEventType) 
                ? parsedEventType 
                : SecurityEventType.PermissionGranted; // Default fallback

            await LogSecurityEventAsync(
                eventType,
                request.Description ?? "Security audit log created",
                request.UserId,
                request.IpAddress,
                null,
                cancellationToken);

            // Get the most recent log for this correlation
            var auditLog = await _context.SecurityAuditLogs
                .Where(log => log.CorrelationId == correlationId)
                .OrderByDescending(log => log.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            if (auditLog != null)
            {
                return auditLog.Adapt<SecurityAuditLogResponse>();
            }

            throw new InvalidOperationException("Failed to create security audit log");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create security audit log (CorrelationId: {CorrelationId})", correlationId);
            throw;
        }
    }

    public async Task<SecurityAuditLogResponse> GetSecurityAuditLogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var auditLog = await _context.SecurityAuditLogs
            .FirstOrDefaultAsync(log => log.Id == id, cancellationToken);

        if (auditLog == null)
        {
            throw new InvalidOperationException($"Security audit log with ID {id} not found");
        }

        return auditLog.Adapt<SecurityAuditLogResponse>();
    }

    public async Task<IEnumerable<SecurityAuditLogResponse>> GetUserSecurityAuditLogsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _context.SecurityAuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Take(100) // Limit to recent 100 logs
                .ToListAsync(cancellationToken);

            return logs.Adapt<IEnumerable<SecurityAuditLogResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security audit logs for user {UserId}", userId);
            return Enumerable.Empty<SecurityAuditLogResponse>();
        }
    }

    public async Task<IEnumerable<SecurityAuditLogResponse>> GetSecurityAuditLogsByEventTypeAsync(SecurityEventType eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _context.SecurityAuditLogs
                .Where(log => log.EventType == eventType)
                .OrderByDescending(log => log.Timestamp)
                .Take(100) // Limit to recent 100 logs
                .ToListAsync(cancellationToken);

            return logs.Adapt<IEnumerable<SecurityAuditLogResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security audit logs for event type {EventType}", eventType);
            return Enumerable.Empty<SecurityAuditLogResponse>();
        }
    }

    public async Task<IEnumerable<SecurityAuditLogResponse>> GetSecurityAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _context.SecurityAuditLogs
                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
                .OrderByDescending(log => log.Timestamp)
                .Take(500) // Limit to 500 logs for date range queries
                .ToListAsync(cancellationToken);

            return logs.Adapt<IEnumerable<SecurityAuditLogResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security audit logs for date range {StartDate} to {EndDate}", startDate, endDate);
            return Enumerable.Empty<SecurityAuditLogResponse>();
        }
    }

    public async Task<SecurityStatisticsResponse> GetSecurityStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            var totalEvents = await _context.SecurityAuditLogs.CountAsync(cancellationToken);
            var failedLogins = await _context.SecurityAuditLogs
                .CountAsync(log => log.EventType == SecurityEventType.FailedLogin && log.Timestamp >= last24Hours, cancellationToken);
            var successfulLogins = await _context.SecurityAuditLogs
                .CountAsync(log => log.EventType == SecurityEventType.PermissionGranted && log.Timestamp >= last24Hours, cancellationToken);

            var stats = new SecurityStatisticsResponse
            {
                StartDate = last30Days,
                EndDate = now,
                TotalEvents = totalEvents,
                FailedLogins = failedLogins,
                SuccessfulLogins = successfulLogins,
                AccountLockouts = await _context.SecurityAuditLogs
                    .CountAsync(log => log.EventType == SecurityEventType.AccountLocked && log.Timestamp >= last30Days, cancellationToken),
                SuspiciousActivities = await _context.SecurityAuditLogs
                    .CountAsync(log => log.EventType == SecurityEventType.SuspiciousActivity && log.Timestamp >= last30Days, cancellationToken),
                FailureRate = totalEvents > 0 ? (decimal)failedLogins / totalEvents * 100 : 0,
                GeneratedAt = now
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get security statistics");
            return new SecurityStatisticsResponse
            {
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<IEnumerable<SecurityAuditLogResponse>> GetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _context.SecurityAuditLogs
                .Where(log => log.UserId == userId && log.EventType == SecurityEventType.FailedLogin)
                .OrderByDescending(log => log.Timestamp)
                .Take(50) // Limit to recent 50 failed attempts
                .ToListAsync(cancellationToken);

            return logs.Adapt<IEnumerable<SecurityAuditLogResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get failed login attempts for user {UserId}", userId);
            return Enumerable.Empty<SecurityAuditLogResponse>();
        }
    }

    public async Task<bool> ShouldLockAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var last15Minutes = DateTime.UtcNow.AddMinutes(-15);
            
            var recentFailedAttempts = await _context.SecurityAuditLogs
                .CountAsync(log => log.UserId == userId && 
                           log.EventType == SecurityEventType.FailedLogin && 
                           log.Timestamp >= last15Minutes, cancellationToken);

            // Lock account if 5 or more failed attempts in last 15 minutes
            return recentFailedAttempts >= 5;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if account should be locked for user {UserId}", userId);
            return false;
        }
    }
}
