using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Dashboard;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Dashboard controller
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardController"/> class
    /// </summary>
    /// <param name="dashboardService">Dashboard service</param>
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    
    /// <summary>
    /// Gets the user coin metrics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User coin metrics</returns>
    [HttpGet("user/{userId}/coins")]
    public async Task<ActionResult<UserCoinMetric>> GetUserCoinMetricsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userCoinMetric = await _dashboardService.GetUserCoinMetricsAsync(userId, cancellationToken);
        return Ok(userCoinMetric);
    }
    
    /// <summary>
    /// Gets the wallet extended
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet extended</returns>
    [HttpGet("user/{userId}/wallet")]
    public async Task<ActionResult<WalletExtended>> GetWalletExtendedAsync(string userId, CancellationToken cancellationToken = default)
    {
        var walletExtended = await _dashboardService.GetWalletExtendedAsync(userId, cancellationToken);
        return Ok(walletExtended);
    }
    
    /// <summary>
    /// Gets the balance metrics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Balance metrics</returns>
    [HttpGet("user/{userId}/balance")]
    public async Task<ActionResult<List<BalanceMetric>>> GetBalanceMetricsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var balanceMetrics = await _dashboardService.GetBalanceMetricsAsync(userId, cancellationToken);
        return Ok(balanceMetrics);
    }
    
    /// <summary>
    /// Gets the news
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>News</returns>
    [HttpGet("news")]
    public async Task<ActionResult<List<News>>> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        var news = await _dashboardService.GetNewsAsync(cancellationToken);
        return Ok(news);
    }
}
