using MobileAPIGateway.Models.Compatibility.Global;

namespace MobileAPIGateway.Services.Compatibility
{
    public interface IGlobalCompatibilityService
    {
        Task<CountriesListResponse> GetCountriesAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
        Task<LanguagesListResponse> GetLanguagesAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
        Task<CryptoCurrenciesListResponse> GetCryptoCurrenciesAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
        Task<UserLimitsDataResponse> GetUserLimitsAsync(CancellationToken cancellationToken = default);
        Task<GendersListResponse> GetGendersAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
        Task<PaymentSourceTypesListResponse> GetPaymentSourceTypesAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
    }
}
