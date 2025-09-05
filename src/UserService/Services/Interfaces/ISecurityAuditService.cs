using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Models.Security;

namespace UserService.Services.Interfaces;

public interface ISecurityAuditService
{
    // Existing methods
    Task LogUserActionAsync(Guid userId, string action, string resource, object? details = null, CancellationToken cancellationToken = default);
    Task LogSecurityEventAsync(SecurityEventType eventType, string description, Guid? userId = null, string? ipAddress = null, object? details = null, CancellationToken cancellationToken = default);
    Task LogRoleChangeAsync(Guid userId, string roleName, RoleChangeType changeType, string? performedBy = null, string? reason = null, CancellationToken cancellationToken = default);
    Task LogWalletActionAsync(Guid userId, string walletAddress, WalletActionType actionType, object? details = null, CancellationToken cancellationToken = default);
    Task LogPermissionCheckAsync(Guid userId, string resource, string action, bool granted, CancellationToken cancellationToken = default);
    Task<IEnumerable<SecurityAuditLog>> GetUserAuditLogsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<SecurityAuditLog>> GetSecurityEventsAsync(SecurityEventType? eventType = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task CleanupOldAuditLogsAsync(int retentionDays = 365, CancellationToken cancellationToken = default);
    
    // Missing methods needed by controller
    Task<SecurityAuditLogResponse> CreateSecurityAuditLogAsync(CreateSecurityAuditLogRequest request, CancellationToken cancellationToken = default);
    Task<SecurityAuditLogResponse> GetSecurityAuditLogAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SecurityAuditLogResponse>> GetUserSecurityAuditLogsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SecurityAuditLogResponse>> GetSecurityAuditLogsByEventTypeAsync(SecurityEventType eventType, CancellationToken cancellationToken = default);
    Task<IEnumerable<SecurityAuditLogResponse>> GetSecurityAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<SecurityStatisticsResponse> GetSecurityStatisticsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<SecurityAuditLogResponse>> GetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ShouldLockAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}
