using System.Text;
using System.Text.Json;
using PaymentGatewayService.Services.Interfaces;

namespace PaymentGatewayService.Services;

/// <summary>
/// HTTP client implementation for communicating with FeeService
/// </summary>
public class FeeServiceApi : IFeeServiceApi
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FeeServiceApi> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FeeServiceApi(HttpClient httpClient, ILogger<FeeServiceApi> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<FiatRejectionFeesResponse> CalculateFiatRejectionFeesAsync(FiatRejectionFeesRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/fees/calculate-fiat-rejection", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<FiatRejectionFeesResponse>(responseContent, _jsonOptions) 
                    ?? new FiatRejectionFeesResponse { TotalFees = 0, NetAmount = request.Amount };
            }

            _logger.LogError("Failed to calculate fiat rejection fees. Status: {StatusCode}", response.StatusCode);
            
            // Return default calculation on failure
            return new FiatRejectionFeesResponse
            {
                TotalFees = CalculateDefaultFiatFees(request.Amount),
                NetAmount = request.Amount - CalculateDefaultFiatFees(request.Amount),
                WireFees = 25m,
                SquareFees = request.Amount * 0.029m,
                InternalFees = 5m
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating fiat rejection fees");
            
            // Return default calculation on error
            var defaultFees = CalculateDefaultFiatFees(request.Amount);
            return new FiatRejectionFeesResponse
            {
                TotalFees = defaultFees,
                NetAmount = request.Amount - defaultFees,
                WireFees = 25m,
                SquareFees = request.Amount * 0.029m,
                InternalFees = 5m
            };
        }
    }

    public async Task<CryptoRejectionFeesResponse> CalculateCryptoRejectionFeesAsync(CryptoRejectionFeesRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/fees/calculate-crypto-rejection", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CryptoRejectionFeesResponse>(responseContent, _jsonOptions) 
                    ?? new CryptoRejectionFeesResponse { TotalFees = 0, NetAmount = request.Amount };
            }

            _logger.LogError("Failed to calculate crypto rejection fees. Status: {StatusCode}", response.StatusCode);
            
            // Return default calculation on failure
            return new CryptoRejectionFeesResponse
            {
                TotalFees = CalculateDefaultCryptoFees(request.Amount, request.Cryptocurrency),
                NetAmount = request.Amount - CalculateDefaultCryptoFees(request.Amount, request.Cryptocurrency),
                NetworkFees = GetDefaultNetworkFee(request.Cryptocurrency),
                InternalFees = 5m
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating crypto rejection fees");
            
            // Return default calculation on error
            var defaultFees = CalculateDefaultCryptoFees(request.Amount, request.Cryptocurrency);
            return new CryptoRejectionFeesResponse
            {
                TotalFees = defaultFees,
                NetAmount = request.Amount - defaultFees,
                NetworkFees = GetDefaultNetworkFee(request.Cryptocurrency),
                InternalFees = 5m
            };
        }
    }

    private static decimal CalculateDefaultFiatFees(decimal amount)
    {
        // Default fiat rejection fees: wire fee + Square processing + internal
        var wireFee = 25m;
        var squareFee = amount * 0.029m; // 2.9%
        var internalFee = 5m;
        
        return wireFee + squareFee + internalFee;
    }

    private static decimal CalculateDefaultCryptoFees(decimal amount, string cryptocurrency)
    {
        // Default crypto rejection fees: network fee + internal
        var networkFee = GetDefaultNetworkFee(cryptocurrency);
        var internalFee = 5m;
        
        return networkFee + internalFee;
    }

    private static decimal GetDefaultNetworkFee(string cryptocurrency)
    {
        // Default network fees by cryptocurrency
        return cryptocurrency?.ToUpperInvariant() switch
        {
            "BTC" => 0.0002m,  // ~$10 at $50k BTC
            "ETH" => 0.005m,   // ~$10 at $2k ETH
            "USDT" => 10m,     // $10 flat
            "USDC" => 10m,     // $10 flat
            _ => 10m           // Default $10
        };
    }
}
