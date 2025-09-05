using MobileAPIGateway.Models.Compatibility.Market;

namespace MobileAPIGateway.Services.Compatibility
{
    public interface IMarketCompatibilityService
    {
        // Search APIs
        Task<SearchResponse> SearchAsync(string emailAddress, string clientIpAddress, string searchRequest, string lang, int take, int skip, CancellationToken cancellationToken = default);
        
        // Market APIs
        Task<ProductDetailsDataResponse> GetProductAsync(string emailAddress, string clientIpAddress, string symbol, string name, CancellationToken cancellationToken = default);
        Task<ProductStatsDataResponse> GetProductStatsAsync(string symbol, string name, int skip, int take, string from, string to, CancellationToken cancellationToken = default);
        Task<MarketSearchDataResponse> PostMarketSearchAsync(MarketSearchRequest request, CancellationToken cancellationToken = default);
        Task<CreateListingDataResponse> PostCreateListingAsync(CreateListingRequest request, CancellationToken cancellationToken = default);
        Task<ListingDetailsDataResponse> GetListingByIdAsync(string listingId, string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
        
        // Market Profile APIs
        Task<CurrentMarketProfileDataResponse> GetCurrentMarketProfileAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
        Task<CreateMarketProfileDataResponse> PostCreateMarketProfileAsync(CreateMarketProfileRequest request, CancellationToken cancellationToken = default);
    }
}
