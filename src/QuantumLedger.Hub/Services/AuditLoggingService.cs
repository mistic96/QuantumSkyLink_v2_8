using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using QuantumLedger.Data;
using QuantumLedger.Models;

namespace QuantumLedger.Hub.Services;

/// <summary>
/// Enhanced audit logging service for security compliance and monitoring
/// Provides comprehensive audit trails for all security-sensitive operations
/// </summary>
public interface IAuditLoggingService
{
    Task LogSecurityEventAsync(SecurityEventType eventType, string description, object? details = null, string? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogSubstitutionKeyEventAsync(SubstitutionKeyEventType eventType, string keyId, string accountId, object? details = null, string? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogAuthenticationEventAsync(AuthenticationEventType eventType, string? userId = null, string? ipAddress = null, object? details = null, CancellationToken cancellationToken = default);
    Task LogDataAccessEventAsync(DataAccessEventType eventType, string resourceType, string resourceId, object? details = null, string? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<List<AuditLogEntry>> GetAuditTrailAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null, SecurityEventType? eventType = null, int maxResults = 1000, CancellationToken cancellationToken = default);
    Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Production implementation of audit logging service with database persistence
/// Designed for SOC2 compliance and security monitoring
/// </summary>
public class AuditLoggingService : IAuditLoggingService
{
    private readonly LedgerContext _context;
    private readonly ILogger<AuditLoggingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditLoggingService(
        LedgerContext context,
        ILogger<AuditLoggingService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Logs general security events for compliance monitoring
    /// </summary>
    public async Task LogSecurityEventAsync(SecurityEventType eventType, string description, object? details = null, string? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEntry = CreateAuditEntry(
                eventType.ToString(),
                "Security",
                description,
                details,
                userId,
                ipAddress
            );

            _context.SecurityAuditLogs.Add(auditEntry);
            await _context.SaveChangesAsync(cancellationToken);

            // Log critical security events to application logs as well
            if (IsCriticalSecurityEvent(eventType))
            {
                _logger.LogWarning("CRITICAL SECURITY EVENT: {EventType} - {Description} - User: {UserId} - IP: {IpAddress}", 
                    eventType, description, userId ?? "Unknown", ipAddress ?? "Unknown");
            }
            else
            {
                _logger.LogInformation("Security Event: {EventType} - {Description} - User: {UserId}", 
                    eventType, description, userId ?? "Unknown");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event: {EventType} - {Description}", eventType, description);
            throw;
        }
    }

    /// <summary>
    /// Logs substitution key specific events for delegation key system monitoring
    /// </summary>
    public async Task LogSubstitutionKeyEventAsync(SubstitutionKeyEventType eventType, string keyId, string accountId, object? details = null, string? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventDetails = new
            {
                KeyId = keyId,
                AccountId = accountId,
                EventType = eventType.ToString(),
                AdditionalDetails = details
            };

            var auditEntry = CreateAuditEntry(
                $"SubstitutionKey.{eventType}",
                "DelegationKeySystem",
                $"Substitution key {eventType.ToString().ToLower()}: {keyId}",
                eventDetails,
                userId,
                ipAddress
            );

            _context.SecurityAuditLogs.Add(auditEntry);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Substitution Key Event: {EventType} - Key: {KeyId} - Account: {AccountId} - User: {UserId}", 
                eventType, keyId, accountId, userId ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log substitution key event: {EventType} - Key: {KeyId}", eventType, keyId);
            throw;
        }
    }

    /// <summary>
    /// Logs authentication events for security monitoring
    /// </summary>
    public async Task LogAuthenticationEventAsync(AuthenticationEventType eventType, string? userId = null, string? ipAddress = null, object? details = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEntry = CreateAuditEntry(
                $"Authentication.{eventType}",
                "Authentication",
                $"Authentication {eventType.ToString().ToLower()}",
                details,
                userId,
                ipAddress
            );

            _context.SecurityAuditLogs.Add(auditEntry);
            await _context.SaveChangesAsync(cancellationToken);

            // Log failed authentication attempts as warnings
            if (eventType == AuthenticationEventType.Failed || eventType == AuthenticationEventType.Blocked)
            {
                _logger.LogWarning("Authentication Event: {EventType} - User: {UserId} - IP: {IpAddress}", 
                    eventType, userId ?? "Unknown", ipAddress ?? "Unknown");
            }
            else
            {
                _logger.LogInformation("Authentication Event: {EventType} - User: {UserId}", 
                    eventType, userId ?? "Unknown");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log authentication event: {EventType} - User: {UserId}", eventType, userId);
            throw;
        }
    }

    /// <summary>
    /// Logs data access events for compliance and monitoring
    /// </summary>
    public async Task LogDataAccessEventAsync(DataAccessEventType eventType, string resourceType, string resourceId, object? details = null, string? userId = null, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventDetails = new
            {
                ResourceType = resourceType,
                ResourceId = resourceId,
                EventType = eventType.ToString(),
                AdditionalDetails = details
            };

            var auditEntry = CreateAuditEntry(
                $"DataAccess.{eventType}",
                "DataAccess",
                $"Data {eventType.ToString().ToLower()}: {resourceType}/{resourceId}",
                eventDetails,
                userId,
                ipAddress
            );

            _context.SecurityAuditLogs.Add(auditEntry);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Data Access Event: {EventType} - Resource: {ResourceType}/{ResourceId} - User: {UserId}", 
                eventType, resourceType, resourceId, userId ?? "System");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log data access event: {EventType} - Resource: {ResourceType}/{ResourceId}", 
                eventType, resourceType, resourceId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves audit trail for compliance reporting and investigation
    /// </summary>
    public async Task<List<AuditLogEntry>> GetAuditTrailAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null, SecurityEventType? eventType = null, int maxResults = 1000, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.SecurityAuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            if (eventType.HasValue)
            {
                query = query.Where(a => a.EventType.StartsWith(eventType.Value.ToString()));
            }

            var securityAuditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(maxResults)
                .ToListAsync(cancellationToken);

            // Convert SecurityAuditLog to AuditLogEntry
            var results = securityAuditLogs.Select(a => new AuditLogEntry
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                EventType = a.EventType,
                Category = a.Category,
                Description = a.Description,
                Details = a.Details,
                UserId = a.UserId,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                RequestId = a.RequestId
            }).ToList();

            _logger.LogInformation("Retrieved {Count} audit log entries for query: User={UserId}, From={FromDate}, To={ToDate}, EventType={EventType}", 
                results.Count, userId, fromDate, toDate, eventType);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit trail");
            throw;
        }
    }

    /// <summary>
    /// Generates security metrics for monitoring and reporting
    /// </summary>
    public async Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _context.SecurityAuditLogs
                .Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate)
                .ToListAsync(cancellationToken);

            var metrics = new SecurityMetrics
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalEvents = auditLogs.Count,
                AuthenticationEvents = auditLogs.Count(a => a.Category == "Authentication"),
                SecurityEvents = auditLogs.Count(a => a.Category == "Security"),
                DataAccessEvents = auditLogs.Count(a => a.Category == "DataAccess"),
                SubstitutionKeyEvents = auditLogs.Count(a => a.Category == "DelegationKeySystem"),
                FailedAuthenticationAttempts = auditLogs.Count(a => a.EventType.Contains("Authentication.Failed")),
                CriticalSecurityEvents = auditLogs.Count(a => IsCriticalEventType(a.EventType)),
                UniqueUsers = auditLogs.Where(a => !string.IsNullOrEmpty(a.UserId)).Select(a => a.UserId).Distinct().Count(),
                UniqueIpAddresses = auditLogs.Where(a => !string.IsNullOrEmpty(a.IpAddress)).Select(a => a.IpAddress).Distinct().Count()
            };

            _logger.LogInformation("Generated security metrics for period {FromDate} to {ToDate}: {TotalEvents} total events", 
                fromDate, toDate, metrics.TotalEvents);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate security metrics for period {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    #region Private Methods

    private SecurityAuditLog CreateAuditEntry(string eventType, string category, string description, object? details, string? userId, string? ipAddress)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        // Extract user information from context if not provided
        if (string.IsNullOrEmpty(userId) && httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     httpContext.User.FindFirst("sub")?.Value ??
                     httpContext.User.Identity.Name;
        }

        // Extract IP address from context if not provided
        if (string.IsNullOrEmpty(ipAddress) && httpContext != null)
        {
            ipAddress = GetClientIpAddress(httpContext);
        }

        var auditLog = new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            EventType = eventType,
            Category = category,
            Description = description,
            Details = details != null ? JsonSerializer.Serialize(details, _jsonOptions) : null,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = httpContext?.Request?.Headers["User-Agent"].FirstOrDefault(),
            RequestId = httpContext?.TraceIdentifier
        };

        // Set severity based on event type
        auditLog.SetSeverityFromEventType();
        
        return auditLog;
    }

    private string? GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers first (load balancers, proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static bool IsCriticalSecurityEvent(SecurityEventType eventType)
    {
        return eventType switch
        {
            SecurityEventType.UnauthorizedAccess => true,
            SecurityEventType.SuspiciousActivity => true,
            SecurityEventType.SecurityBreach => true,
            SecurityEventType.PrivilegeEscalation => true,
            SecurityEventType.DataExfiltration => true,
            _ => false
        };
    }

    private static bool IsCriticalEventType(string eventType)
    {
        return eventType.Contains("Unauthorized") ||
               eventType.Contains("Suspicious") ||
               eventType.Contains("Breach") ||
               eventType.Contains("Failed") ||
               eventType.Contains("Blocked");
    }

    #endregion
}

#region Enums and Models

/// <summary>
/// Types of security events for audit logging
/// </summary>
public enum SecurityEventType
{
    Login,
    Logout,
    UnauthorizedAccess,
    SuspiciousActivity,
    SecurityBreach,
    PrivilegeEscalation,
    DataExfiltration,
    ConfigurationChange,
    SystemAccess,
    ApiAccess
}

/// <summary>
/// Types of substitution key events for delegation key system monitoring
/// </summary>
public enum SubstitutionKeyEventType
{
    Generated,
    Validated,
    Revoked,
    Rotated,
    SignatureVerified,
    SignatureRejected,
    Accessed,
    Modified
}

/// <summary>
/// Types of authentication events for security monitoring
/// </summary>
public enum AuthenticationEventType
{
    Success,
    Failed,
    Blocked,
    PasswordReset,
    AccountLocked,
    AccountUnlocked,
    TwoFactorEnabled,
    TwoFactorDisabled
}

/// <summary>
/// Types of data access events for compliance monitoring
/// </summary>
public enum DataAccessEventType
{
    Read,
    Write,
    Update,
    Delete,
    Export,
    Import,
    Backup,
    Restore
}

/// <summary>
/// Security metrics for monitoring and reporting
/// </summary>
public class SecurityMetrics
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalEvents { get; set; }
    public int AuthenticationEvents { get; set; }
    public int SecurityEvents { get; set; }
    public int DataAccessEvents { get; set; }
    public int SubstitutionKeyEvents { get; set; }
    public int FailedAuthenticationAttempts { get; set; }
    public int CriticalSecurityEvents { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueIpAddresses { get; set; }
}

/// <summary>
/// Audit log entry for database storage
/// </summary>
public class AuditLogEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestId { get; set; }
}

#endregion
