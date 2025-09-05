using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Services;

/// <summary>
/// Secondary markets service
/// </summary>
public class SecondaryMarketsService : ISecondaryMarketsService
{
    private readonly ISecondaryMarketsClient _secondaryMarketsClient;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SecondaryMarketsService"/> class
    /// </summary>
    /// <param name="secondaryMarketsClient">Secondary markets client</param>
    public SecondaryMarketsService(ISecondaryMarketsClient secondaryMarketsClient)
    {
        _secondaryMarketsClient = secondaryMarketsClient;
    }
    
    /// <inheritdoc/>
    public async Task<List<MarketListing>> GetMarketListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _secondaryMarketsClient.GetMarketListingsAsync(cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<MarketListing> GetMarketListingByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _secondaryMarketsClient.GetMarketListingByIdAsync(id, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<MarketListing> CreateMarketListingAsync(MarketListing marketListing, CancellationToken cancellationToken = default)
    {
        return await _secondaryMarketsClient.CreateMarketListingAsync(marketListing, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<MarketListing> UpdateMarketListingAsync(string id, MarketListing marketListing, CancellationToken cancellationToken = default)
    {
        return await _secondaryMarketsClient.UpdateMarketListingAsync(id, marketListing, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task DeleteMarketListingAsync(string id, CancellationToken cancellationToken = default)
    {
        await _secondaryMarketsClient.DeleteMarketListingAsync(id, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<List<ListingSale>> GetListingSalesAsync(CancellationToken cancellationToken = default)
    {
        return await _secondaryMarketsClient.GetListingSalesAsync(cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<ListingSale> GetListingSaleByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _secondaryMarketsClient.GetListingSaleByIdAsync(id, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<ListingSale> CreateListingSaleAsync(ListingSale listingSale, CancellationToken cancellationToken = default)
    {
        return await _secondaryMarketsClient.CreateListingSaleAsync(listingSale, cancellationToken);
    }
}
