using Refit;
using MobileAPIGateway.Models.SecondaryMarkets;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Secondary markets client interface
/// </summary>
public interface ISecondaryMarketsClient
{
    /// <summary>
    /// Gets the market listings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market listings</returns>
    [Get("/api/secondary-markets/listings")]
    Task<List<MarketListing>> GetMarketListingsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the market listing by ID
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market listing</returns>
    [Get("/api/secondary-markets/listings/{id}")]
    Task<MarketListing> GetMarketListingByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a market listing
    /// </summary>
    /// <param name="marketListing">Market listing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created market listing</returns>
    [Post("/api/secondary-markets/listings")]
    Task<MarketListing> CreateMarketListingAsync([Body] MarketListing marketListing, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a market listing
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="marketListing">Market listing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated market listing</returns>
    [Put("/api/secondary-markets/listings/{id}")]
    Task<MarketListing> UpdateMarketListingAsync(string id, [Body] MarketListing marketListing, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a market listing
    /// </summary>
    /// <param name="id">Market listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    [Delete("/api/secondary-markets/listings/{id}")]
    Task DeleteMarketListingAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the listing sales
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Listing sales</returns>
    [Get("/api/secondary-markets/sales")]
    Task<List<ListingSale>> GetListingSalesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the listing sale by ID
    /// </summary>
    /// <param name="id">Listing sale ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Listing sale</returns>
    [Get("/api/secondary-markets/sales/{id}")]
    Task<ListingSale> GetListingSaleByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a listing sale
    /// </summary>
    /// <param name="listingSale">Listing sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created listing sale</returns>
    [Post("/api/secondary-markets/sales")]
    Task<ListingSale> CreateListingSaleAsync([Body] ListingSale listingSale, CancellationToken cancellationToken = default);
}
