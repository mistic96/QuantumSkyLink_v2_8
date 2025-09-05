namespace GovernanceService.Services.Interfaces;

public interface IGovernanceBackgroundService
{
    // Background processing
    Task ProcessExpiredProposalsAsync();
    Task ProcessScheduledExecutionsAsync();
    Task UpdateProposalStatusesAsync();
    Task ProcessDelegationExpirationsAsync();

    // Cache management
    Task InvalidateGovernanceCacheAsync();
    Task RefreshGovernanceCacheAsync();
    Task CleanupExpiredCacheEntriesAsync();

    // Maintenance tasks
    Task PerformDailyMaintenanceAsync();
    Task ArchiveOldProposalsAsync(DateTime cutoffDate);
    Task CleanupFailedExecutionsAsync();
    Task ProcessAutoRenewalDelegationsAsync();
}
