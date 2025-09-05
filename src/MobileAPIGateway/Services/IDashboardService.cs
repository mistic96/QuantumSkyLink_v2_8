using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Services;

/// <summary>
/// Dashboard service interface
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets the user coin metrics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User coin metrics</returns>
    Task<UserCoinMetric> GetUserCoinMetricsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the wallet extended
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet extended</returns>
    Task<WalletExtended> GetWalletExtendedAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the balance metrics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Balance metrics</returns>
    Task<List<BalanceMetric>> GetBalanceMetricsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the news
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News</returns>
    Task<List<News>> GetNewsAsync(CancellationToken cancellationToken = default);
}
