using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Services;

/// <summary>
/// Secondary markets service interface
/// </summary>
public interface ISecondaryMarketsService
{
    /// <summary>
    /// Gets the market listings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market listings</returns>
    Task<List<MarketListing>> GetMarketListingsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the market listing by ID
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market listing</returns>
    Task<MarketListing> GetMarketListingByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a market listing
    /// </summary>
    /// <param name="marketListing">Market listing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created market listing</returns>
    Task<MarketListing> CreateMarketListingAsync(MarketListing marketListing, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a market listing
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="marketListing">Market listing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated market listing</returns>
    Task<MarketListing> UpdateMarketListingAsync(string id, MarketListing marketListing, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a market listing
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task DeleteMarketListingAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the listing sales
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Listing sales</returns>
    Task<List<ListingSale>> GetListingSalesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the listing sale by ID
    /// </summary>
    /// <param name="id">Listing sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Listing sale</returns>
    Task<ListingSale> GetListingSaleByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a listing sale
    /// </summary>
    /// <param name="listingSale">Listing sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created listing sale</returns>
    Task<ListingSale> CreateListingSaleAsync(ListingSale listingSale, CancellationToken cancellationToken = default);
}
