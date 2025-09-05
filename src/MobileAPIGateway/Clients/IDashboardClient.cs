using Refit;
using MobileAPIGateway.Models.Dashboard;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Dashboard client interface
/// </summary>
public interface IDashboardClient
{
    /// <summary>
    /// Gets the user coin metrics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User coin metrics</returns>
    [Get("/api/dashboard/user/{userId}/coins")]
    Task<UserCoinMetric> GetUserCoinMetricsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the wallet extended
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet extended</returns>
    [Get("/api/dashboard/user/{userId}/wallet")]
    Task<WalletExtended> GetWalletExtendedAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the balance metrics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Balance metrics</returns>
    [Get("/api/dashboard/user/{userId}/balance")]
    Task<List<BalanceMetric>> GetBalanceMetricsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the news
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News</returns>
    [Get("/api/dashboard/news")]
    Task<List<News>> GetNewsAsync(CancellationToken cancellationToken = default);
}
