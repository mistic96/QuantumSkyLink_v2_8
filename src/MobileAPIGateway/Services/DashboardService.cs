using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Services;

/// <summary>
/// Dashboard service
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardClient _dashboardClient;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardService"/> class
    /// </summary>
    /// <param name="dashboardClient">Dashboard client</param>
    public DashboardService(IDashboardClient dashboardClient)
    {
        _dashboardClient = dashboardClient;
    }
    
    /// <inheritdoc/>
    public async Task<UserCoinMetric> GetUserCoinMetricsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dashboardClient.GetUserCoinMetricsAsync(userId, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<WalletExtended> GetWalletExtendedAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dashboardClient.GetWalletExtendedAsync(userId, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<List<BalanceMetric>> GetBalanceMetricsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dashboardClient.GetBalanceMetricsAsync(userId, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<List<News>> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        return await _dashboardClient.GetNewsAsync(cancellationToken);
    }
}
