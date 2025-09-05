using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuantumLedger.Blockchain.Models;
using QuantumLedger.Blockchain.Services.MultiChain;

namespace QuantumLedger.Blockchain.Services.MultiChain
{
    /// <summary>
    /// Service for token operations on MultiChain.
    /// </summary>
    public class MultiChainTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MultiChainTokenService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private int _requestId = 1;

        /// <summary>
        /// Initializes a new instance of the MultiChainTokenService class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="httpClient">The HTTP client to use.</param>
        public MultiChainTokenService(
            IConfiguration configuration,
            ILogger<MultiChainTokenService> logger,
            HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Get configuration values
            var rpcUser = configuration["MultiChain:RpcUser"] ?? throw new ArgumentException("MultiChain:RpcUser configuration is missing");
            var rpcPassword = configuration["MultiChain:RpcPassword"] ?? throw new ArgumentException("MultiChain:RpcPassword configuration is missing");
            var endpoint = configuration["MultiChain:Endpoint"] ?? throw new ArgumentException("MultiChain:Endpoint configuration is missing");
            
            // Set base address from configuration
            _httpClient.BaseAddress = new Uri(endpoint);
            
            // Set up basic authentication
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{rpcUser}:{rpcPassword}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            
            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            
            _logger.LogInformation("MultiChainTokenService initialized with endpoint: {Endpoint}", endpoint);
        }

        /// <summary>
        /// Creates a token on the MultiChain blockchain.
        /// </summary>
        /// <param name="currency">The currency to create.</param>
        /// <returns>A result containing the token ID if successful.</returns>
        public async Task<Result<string>> CreateTokenAsync(Currency currency)
        {
            if (currency == null)
                return Result<string>.Failure("Currency cannot be null");
                
            try
            {
                _logger.LogInformation(
                    "Creating token {Symbol} ({Name}) with issuer {IssuerAddress}",
                    currency.Symbol,
                    currency.Name,
                    currency.IssuerAddress);
                
                // Get initial supply from metadata
                decimal initialSupply = 0;
                if (currency.Metadata != null && currency.Metadata.TryGetValue("InitialSupply", out var initialSupplyStr))
                {
                    if (!decimal.TryParse(initialSupplyStr, out initialSupply))
                    {
                        return Result<string>.Failure("Invalid InitialSupply value in metadata");
                    }
                }
                
                // Create asset in MultiChain
                var request = new MultiChainRequest
                {
                    Method = "issue",
                    Params = new object[] 
                    { 
                        currency.IssuerAddress,  // Address to issue to
                        currency.Symbol,         // Asset name
                        initialSupply,           // Initial quantity
                        currency.Decimals,       // Units
                        0,                       // Native amount
                        new Dictionary<string, string>
                        {
                            ["name"] = currency.Name,
                            ["type"] = currency.Type,
                            ["created"] = currency.CreatedAt.ToString("o")
                        }
                    },
                    Id = GetNextRequestId()
                };
                
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<string>>(_jsonOptions);
                
                if (result?.Error != null)
                {
                    _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                    return Result<string>.Failure(result.Error.Message);
                }
                
                if (string.IsNullOrEmpty(result?.Result))
                {
                    _logger.LogError("MultiChain returned null or empty token ID");
                    return Result<string>.Failure("MultiChain returned null or empty token ID");
                }
                
                _logger.LogInformation("Token {Symbol} created successfully with ID: {TokenId}", currency.Symbol, result.Result);
                return Result<string>.Success(result.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token in MultiChain");
                return Result<string>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Transfers tokens from one address to another.
        /// </summary>
        /// <param name="fromAddress">The address to transfer from.</param>
        /// <param name="toAddress">The address to transfer to.</param>
        /// <param name="symbol">The token symbol.</param>
        /// <param name="amount">The amount to transfer.</param>
        /// <returns>A result containing the transaction ID if successful.</returns>
        public async Task<Result<string>> TransferTokenAsync(string fromAddress, string toAddress, string symbol, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(fromAddress))
                return Result<string>.Failure("From address cannot be null or empty");
                
            if (string.IsNullOrWhiteSpace(toAddress))
                return Result<string>.Failure("To address cannot be null or empty");
                
            if (string.IsNullOrWhiteSpace(symbol))
                return Result<string>.Failure("Symbol cannot be null or empty");
                
            if (amount <= 0)
                return Result<string>.Failure("Amount must be greater than zero");
                
            try
            {
                _logger.LogInformation(
                    "Transferring {Amount} {Symbol} from {FromAddress} to {ToAddress}",
                    amount,
                    symbol,
                    fromAddress,
                    toAddress);
                
                // Check if this is a token transfer or native currency transfer
                if (symbol != "MCA") // Not native currency
                {
                    // Create token transfer request
                    var request = new MultiChainRequest
                    {
                        Method = "sendassetfrom",
                        Params = new object[] 
                        { 
                            fromAddress,
                            toAddress,
                            symbol,
                            amount
                        },
                        Id = GetNextRequestId()
                    };
                    
                    var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                    response.EnsureSuccessStatusCode();
                    
                    var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<string>>(_jsonOptions);
                    
                    if (result?.Error != null)
                    {
                        _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                        return Result<string>.Failure(result.Error.Message);
                    }
                    
                    if (string.IsNullOrEmpty(result?.Result))
                    {
                        _logger.LogError("MultiChain returned null or empty transaction ID");
                        return Result<string>.Failure("MultiChain returned null or empty transaction ID");
                    }
                    
                    _logger.LogInformation("Token transfer submitted successfully with ID: {TransactionId}", result.Result);
                    return Result<string>.Success(result.Result);
                }
                else
                {
                    // Create native currency transfer request
                    var request = new MultiChainRequest
                    {
                        Method = "sendfrom",
                        Params = new object[] 
                        { 
                            fromAddress,
                            toAddress,
                            amount
                        },
                        Id = GetNextRequestId()
                    };
                    
                    var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                    response.EnsureSuccessStatusCode();
                    
                    var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<string>>(_jsonOptions);
                    
                    if (result?.Error != null)
                    {
                        _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                        return Result<string>.Failure(result.Error.Message);
                    }
                    
                    if (string.IsNullOrEmpty(result?.Result))
                    {
                        _logger.LogError("MultiChain returned null or empty transaction ID");
                        return Result<string>.Failure("MultiChain returned null or empty transaction ID");
                    }
                    
                    _logger.LogInformation("Native currency transfer submitted successfully with ID: {TransactionId}", result.Result);
                    return Result<string>.Success(result.Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring token in MultiChain");
                return Result<string>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Gets token information from the MultiChain blockchain.
        /// </summary>
        /// <param name="symbol">The token symbol.</param>
        /// <returns>A result containing the token information if successful.</returns>
        public async Task<Result<TokenInfo>> GetTokenInfoAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return Result<TokenInfo>.Failure("Symbol cannot be null or empty");
                
            try
            {
                _logger.LogDebug("Getting token info for symbol: {Symbol}", symbol);
                
                // Create RPC request
                var request = new MultiChainRequest
                {
                    Method = "listassets",
                    Params = new object[] { symbol, true },
                    Id = GetNextRequestId()
                };
                
                // Send request to MultiChain
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                // Parse response
                var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<List<MultiChainAsset>>>(_jsonOptions);
                
                if (result?.Error != null)
                {
                    _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                    return Result<TokenInfo>.Failure(result.Error.Message);
                }
                
                if (result?.Result == null || result.Result.Count == 0)
                {
                    _logger.LogError("Token not found: {Symbol}", symbol);
                    return Result<TokenInfo>.Failure($"Token not found: {symbol}");
                }
                
                var asset = result.Result[0];
                
                // Map to TokenInfo
                var tokenInfo = new TokenInfo
                {
                    Symbol = asset.Name,
                    Name = asset.Details.TryGetValue("name", out var name) ? name : asset.Name,
                    Type = asset.Details.TryGetValue("type", out var type) ? type : "Token",
                    Decimals = asset.Units,
                    TotalSupply = asset.IssueQty,
                    IssuerAddress = asset.IssueAddress,
                    Metadata = asset.Details
                };
                
                _logger.LogDebug("Token info retrieved successfully for symbol: {Symbol}", symbol);
                
                return Result<TokenInfo>.Success(tokenInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token info from MultiChain for symbol: {Symbol}", symbol);
                return Result<TokenInfo>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new address on the MultiChain blockchain.
        /// </summary>
        /// <returns>A result containing the new address if successful.</returns>
        public async Task<Result<string>> CreateAddressAsync()
        {
            try
            {
                _logger.LogDebug("Creating new address");
                
                // Create RPC request
                var request = new MultiChainRequest
                {
                    Method = "getnewaddress",
                    Params = Array.Empty<object>(),
                    Id = GetNextRequestId()
                };
                
                // Send request to MultiChain
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                // Parse response
                var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<string>>(_jsonOptions);
                
                if (result?.Error != null)
                {
                    _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                    return Result<string>.Failure(result.Error.Message);
                }
                
                if (string.IsNullOrEmpty(result?.Result))
                {
                    _logger.LogError("MultiChain returned null or empty address");
                    return Result<string>.Failure("MultiChain returned null or empty address");
                }
                
                _logger.LogInformation("New address created: {Address}", result.Result);
                return Result<string>.Success(result.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address in MultiChain");
                return Result<string>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Gets the next request ID.
        /// </summary>
        /// <returns>The next request ID.</returns>
        private int GetNextRequestId()
        {
            return System.Threading.Interlocked.Increment(ref _requestId);
        }
    }

    /// <summary>
    /// Represents token information.
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// Gets or sets the token symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the token name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places.
        /// </summary>
        public int Decimals { get; set; }

        /// <summary>
        /// Gets or sets the total supply.
        /// </summary>
        public decimal TotalSupply { get; set; }

        /// <summary>
        /// Gets or sets the issuer address.
        /// </summary>
        public string IssuerAddress { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the token.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Represents a currency in the system.
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// Gets or sets the unique identifier for the currency.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the currency.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the currency.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the type of the currency.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places.
        /// </summary>
        public int Decimals { get; set; }

        /// <summary>
        /// Gets or sets the issuer address.
        /// </summary>
        public string IssuerAddress { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the currency.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Represents a MultiChain asset.
    /// </summary>
    internal class MultiChainAsset
    {
        /// <summary>
        /// Gets or sets the asset name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the asset ID.
        /// </summary>
        public string AssetRef { get; set; }

        /// <summary>
        /// Gets or sets the issue quantity.
        /// </summary>
        public decimal IssueQty { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places.
        /// </summary>
        public int Units { get; set; }

        /// <summary>
        /// Gets or sets the issuer address.
        /// </summary>
        public string IssueAddress { get; set; }

        /// <summary>
        /// Gets or sets the issue transaction ID.
        /// </summary>
        public string IssueTxid { get; set; }

        /// <summary>
        /// Gets or sets the asset details.
        /// </summary>
        public Dictionary<string, string> Details { get; set; }
    }
}
