using MobileAPIGateway.Models.Compatibility.CustomerMarkets;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for customer markets compatibility service
/// </summary>
public interface ICustomerMarketsCompatibilityService
{
    /// <summary>
    /// Gets the customer market list
    /// </summary>
    /// <param name="request">The customer market list request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The customer market list response</returns>
    Task<CustomerMarketListResponse> GetCustomerMarketListAsync(CustomerMarketListRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the customer trading pairs for a customer market
    /// </summary>
    /// <param name="request">The customer trading pair request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The customer trading pair response</returns>
    Task<CustomerTradingPairResponse> GetCustomerTradingPairsAsync(CustomerTradingPairRequest request, CancellationToken cancellationToken = default);
}
