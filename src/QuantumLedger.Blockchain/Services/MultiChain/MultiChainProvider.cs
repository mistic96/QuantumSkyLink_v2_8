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

namespace QuantumLedger.Blockchain.Services.MultiChain
{
    /// <summary>
    /// Represents the status of the blockchain.
    /// </summary>
    public class BlockchainStatus
    {
        /// <summary>
        /// Gets or sets whether the blockchain is connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the current block height.
        /// </summary>
        public long BlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the number of connected peers.
        /// </summary>
        public int PeerCount { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the last block.
        /// </summary>
        public DateTime LastBlockTime { get; set; }

        /// <summary>
        /// Gets or sets the blockchain version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets additional information about the blockchain.
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; }

        /// <summary>
        /// Gets or sets the error message if the blockchain is not connected.
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Defines the contract for blockchain provider operations.
    /// </summary>
    public interface IBlockchainProvider : IBlockchainService
    {
        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Checks if the blockchain is healthy.
        /// </summary>
        /// <returns>True if the blockchain is healthy, false otherwise.</returns>
        Task<bool> IsHealthyAsync();

        /// <summary>
        /// Gets the status of the blockchain.
        /// </summary>
        /// <returns>The blockchain status.</returns>
        Task<BlockchainStatus> GetStatusAsync();
    }

    /// <summary>
    /// MultiChain implementation of the blockchain provider interface.
    /// </summary>
    public class MultiChainProvider : IBlockchainProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MultiChainProvider> _logger;
        private readonly string _rpcUser;
        private readonly string _rpcPassword;
        private readonly JsonSerializerOptions _jsonOptions;
        private int _requestId = 1;

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string ProviderName => "MultiChain";

        /// <summary>
        /// Initializes a new instance of the MultiChainProvider class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="httpClient">The HTTP client to use.</param>
        public MultiChainProvider(
            IConfiguration configuration,
            ILogger<MultiChainProvider> logger,
            HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Get configuration values
            _rpcUser = configuration["MultiChain:RpcUser"] ?? throw new ArgumentException("MultiChain:RpcUser configuration is missing");
            _rpcPassword = configuration["MultiChain:RpcPassword"] ?? throw new ArgumentException("MultiChain:RpcPassword configuration is missing");
            var endpoint = configuration["MultiChain:Endpoint"] ?? throw new ArgumentException("MultiChain:Endpoint configuration is missing");
            
            // Set base address from configuration
            _httpClient.BaseAddress = new Uri(endpoint);
            
            // Set up basic authentication
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_rpcUser}:{_rpcPassword}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            
            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            
            _logger.LogInformation("MultiChainProvider initialized with endpoint: {Endpoint}", endpoint);
        }

        /// <inheritdoc/>
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                _logger.LogDebug("Checking MultiChain health");
                
                var request = new MultiChainRequest
                {
                    Method = "getinfo",
                    Params = Array.Empty<object>(),
                    Id = GetNextRequestId()
                };
                
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("MultiChain health check failed with status code: {StatusCode}", response.StatusCode);
                    return false;
                }
                
                _logger.LogDebug("MultiChain health check successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking MultiChain health");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<BlockchainStatus> GetStatusAsync()
        {
            try
            {
                _logger.LogDebug("Getting MultiChain status");
                
                var request = new MultiChainRequest
                {
                    Method = "getinfo",
                    Params = Array.Empty<object>(),
                    Id = GetNextRequestId()
                };
                
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<MultiChainInfo>>(_jsonOptions);
                
                if (result?.Error != null)
                {
                    _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                    throw new Exception($"MultiChain error: {result.Error.Message}");
                }
                
                if (result?.Result == null)
                {
                    _logger.LogError("MultiChain returned null result");
                    throw new Exception("MultiChain returned null result");
                }
                
                _logger.LogDebug("MultiChain status retrieved successfully");
                
                return new BlockchainStatus
                {
                    IsConnected = true,
                    BlockHeight = result.Result.Blocks,
                    PeerCount = result.Result.Connections,
                    LastBlockTime = DateTimeOffset.FromUnixTimeSeconds(result.Result.Time).DateTime,
                    Version = result.Result.Version,
                    AdditionalInfo = new Dictionary<string, string>
                    {
                        ["ChainName"] = result.Result.ChainName,
                        ["Protocol"] = result.Result.Protocol.ToString(),
                        ["NodeAddress"] = result.Result.NodeAddress,
                        ["Difficulty"] = result.Result.Difficulty.ToString(),
                        ["NetworkHashps"] = result.Result.NetworkHashps.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MultiChain status");
                
                return new BlockchainStatus
                {
                    IsConnected = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <inheritdoc/>
        public async Task<Result<string>> SubmitTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                return Result<string>.Failure("Transaction cannot be null");
                
            try
            {
                _logger.LogInformation(
                    "Submitting transaction from {FromAddress} to {ToAddress} with amount {Amount}",
                    transaction.FromAddress,
                    transaction.ToAddress,
                    transaction.Amount);
                
                // Get currency symbol from metadata
                if (!transaction.Metadata.TryGetValue("CurrencySymbol", out var currencySymbol))
                {
                    return Result<string>.Failure("Transaction metadata must contain CurrencySymbol");
                }
                
                // Check if this is a token transfer or native currency transfer
                if (currencySymbol != "MCA") // Not native currency
                {
                    // Create token transfer request
                    var request = new MultiChainRequest
                    {
                        Method = "sendasset",
                        Params = new object[] 
                        { 
                            transaction.ToAddress,
                            currencySymbol,
                            transaction.Amount
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
                    
                    _logger.LogInformation("Token transaction submitted successfully with ID: {TransactionId}", result.Result);
                    return Result<string>.Success(result.Result);
                }
                else
                {
                    // Create native currency transfer request
                    var request = new MultiChainRequest
                    {
                        Method = "sendtoaddress",
                        Params = new object[] 
                        { 
                            transaction.ToAddress,
                            transaction.Amount
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
                    
                    _logger.LogInformation("Native currency transaction submitted successfully with ID: {TransactionId}", result.Result);
                    return Result<string>.Success(result.Result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting transaction to MultiChain");
                return Result<string>.Failure(ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<TransactionStatus>> GetTransactionStatusAsync(string txId)
        {
            if (string.IsNullOrWhiteSpace(txId))
                return Result<TransactionStatus>.Failure("Transaction ID cannot be null or empty");
                
            try
            {
                _logger.LogDebug("Getting transaction status for ID: {TransactionId}", txId);
                
                // Create RPC request
                var request = new MultiChainRequest
                {
                    Method = "gettransaction",
                    Params = new object[] { txId },
                    Id = GetNextRequestId()
                };
                
                // Send request to MultiChain
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                // Parse response
                var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<MultiChainTransaction>>(_jsonOptions);
                
                if (result?.Error != null)
                {
                    _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                    return Result<TransactionStatus>.Failure(result.Error.Message);
                }
                
                if (result?.Result == null)
                {
                    _logger.LogError("MultiChain returned null transaction");
                    return Result<TransactionStatus>.Failure("MultiChain returned null transaction");
                }
                
                // Map to TransactionStatus
                var status = new TransactionStatus
                {
                    TransactionId = txId,
                    Status = result.Result.Confirmations >= 1 ? TransactionStatuses.Confirmed : TransactionStatuses.Pending,
                    BlockNumber = result.Result.BlockHash != null ? await GetBlockNumberAsync(result.Result.BlockHash) : null,
                    LastUpdated = DateTime.UtcNow
                };
                
                _logger.LogDebug("Transaction status retrieved successfully for ID: {TransactionId}, Status: {Status}", txId, status.Status);
                
                return Result<TransactionStatus>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transaction status from MultiChain for ID: {TransactionId}", txId);
                return Result<TransactionStatus>.Failure(ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<BlockInfo>> GetBlockInfoAsync(string blockId)
        {
            if (string.IsNullOrWhiteSpace(blockId))
                return Result<BlockInfo>.Failure("Block ID cannot be null or empty");
                
            try
            {
                _logger.LogDebug("Getting block info for ID: {BlockId}", blockId);
                
                // Create RPC request
                var request = new MultiChainRequest
                {
                    Method = "getblock",
                    Params = new object[] { blockId },
                    Id = GetNextRequestId()
                };
                
                // Send request to MultiChain
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                // Parse response
                var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<MultiChainBlock>>(_jsonOptions);
                
                if (result?.Error != null)
                {
                    _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", result.Error.Code, result.Error.Message);
                    return Result<BlockInfo>.Failure(result.Error.Message);
                }
                
                if (result?.Result == null)
                {
                    _logger.LogError("MultiChain returned null block");
                    return Result<BlockInfo>.Failure("MultiChain returned null block");
                }
                
                // Map to BlockInfo
                var blockInfo = new BlockInfo
                {
                    Number = result.Result.Height,
                    Hash = result.Result.Hash,
                    PreviousHash = result.Result.PreviousBlockHash,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(result.Result.Time).DateTime,
                    TransactionIds = result.Result.Tx
                };
                
                _logger.LogDebug("Block info retrieved successfully for ID: {BlockId}", blockId);
                
                return Result<BlockInfo>.Success(blockInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting block info from MultiChain for ID: {BlockId}", blockId);
                return Result<BlockInfo>.Failure(ex.Message);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<AccountState>> GetAccountStateAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return Result<AccountState>.Failure("Address cannot be null or empty");
                
            try
            {
                _logger.LogDebug("Getting account state for address: {Address}", address);
                
                // Create RPC request for native balance
                var nativeBalanceRequest = new MultiChainRequest
                {
                    Method = "getaddressbalances",
                    Params = new object[] { address, 0 }, // 0 means include watch-only addresses
                    Id = GetNextRequestId()
                };
                
                // Send request to MultiChain
                var nativeBalanceResponse = await _httpClient.PostAsJsonAsync("", nativeBalanceRequest, _jsonOptions);
                nativeBalanceResponse.EnsureSuccessStatusCode();
                
                // Parse response
                var nativeBalanceResult = await nativeBalanceResponse.Content.ReadFromJsonAsync<MultiChainResponse<List<MultiChainBalance>>>(_jsonOptions);
                
                if (nativeBalanceResult?.Error != null)
                {
                    _logger.LogError("MultiChain error: {ErrorCode} - {ErrorMessage}", nativeBalanceResult.Error.Code, nativeBalanceResult.Error.Message);
                    return Result<AccountState>.Failure(nativeBalanceResult.Error.Message);
                }
                
                // Get native balance
                decimal nativeBalance = 0;
                if (nativeBalanceResult?.Result != null)
                {
                    foreach (var balance in nativeBalanceResult.Result)
                    {
                        if (balance.Name == "")
                        {
                            nativeBalance = balance.Qty;
                            break;
                        }
                    }
                }
                
                // Create account state
                var accountState = new AccountState
                {
                    Address = address,
                    Balance = nativeBalance,
                    Nonce = 0, // MultiChain doesn't have a nonce concept like Ethereum
                    LastUpdated = DateTime.UtcNow,
                    Metadata = new Dictionary<string, string>()
                };
                
                // Add token balances to metadata
                if (nativeBalanceResult?.Result != null)
                {
                    foreach (var balance in nativeBalanceResult.Result)
                    {
                        if (!string.IsNullOrEmpty(balance.Name))
                        {
                            accountState.Metadata[balance.Name] = balance.Qty.ToString();
                        }
                    }
                }
                
                _logger.LogDebug("Account state retrieved successfully for address: {Address}", address);
                
                return Result<AccountState>.Success(accountState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account state from MultiChain for address: {Address}", address);
                return Result<AccountState>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Gets the block number for a block hash.
        /// </summary>
        /// <param name="blockHash">The block hash to query.</param>
        /// <returns>The block number if found, or null if not found.</returns>
        private async Task<long?> GetBlockNumberAsync(string blockHash)
        {
            try
            {
                // Create RPC request
                var request = new MultiChainRequest
                {
                    Method = "getblock",
                    Params = new object[] { blockHash },
                    Id = GetNextRequestId()
                };
                
                // Send request to MultiChain
                var response = await _httpClient.PostAsJsonAsync("", request, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                // Parse response
                var result = await response.Content.ReadFromJsonAsync<MultiChainResponse<MultiChainBlock>>(_jsonOptions);
                
                if (result?.Error != null || result?.Result == null)
                {
                    return null;
                }
                
                return result.Result.Height;
            }
            catch
            {
                return null;
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
    /// Represents a MultiChain RPC request.
    /// </summary>
    internal class MultiChainRequest
    {
        /// <summary>
        /// Gets or sets the method to call.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the method.
        /// </summary>
        public object[] Params { get; set; }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Represents a MultiChain RPC response.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    internal class MultiChainResponse<T>
    {
        /// <summary>
        /// Gets or sets the result of the call.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the error if the call failed.
        /// </summary>
        public MultiChainError Error { get; set; }

        /// <summary>
        /// Gets or sets the request ID.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// Represents a MultiChain RPC error.
    /// </summary>
    internal class MultiChainError
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Represents the result of the getinfo call.
    /// </summary>
    internal class MultiChainInfo
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the protocol version.
        /// </summary>
        public int Protocol { get; set; }

        /// <summary>
        /// Gets or sets the chain name.
        /// </summary>
        public string ChainName { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks.
        /// </summary>
        public long Blocks { get; set; }

        /// <summary>
        /// Gets or sets the number of connections.
        /// </summary>
        public int Connections { get; set; }

        /// <summary>
        /// Gets or sets the difficulty.
        /// </summary>
        public decimal Difficulty { get; set; }

        /// <summary>
        /// Gets or sets the network hashrate.
        /// </summary>
        public decimal NetworkHashps { get; set; }

        /// <summary>
        /// Gets or sets the node address.
        /// </summary>
        public string NodeAddress { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public long Time { get; set; }
    }

    /// <summary>
    /// Represents a MultiChain transaction.
    /// </summary>
    internal class MultiChainTransaction
    {
        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public string Txid { get; set; }

        /// <summary>
        /// Gets or sets the number of confirmations.
        /// </summary>
        public int Confirmations { get; set; }

        /// <summary>
        /// Gets or sets the block hash.
        /// </summary>
        public string BlockHash { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public long Time { get; set; }
    }

    /// <summary>
    /// Represents a MultiChain block.
    /// </summary>
    internal class MultiChainBlock
    {
        /// <summary>
        /// Gets or sets the block hash.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the previous block hash.
        /// </summary>
        public string PreviousBlockHash { get; set; }

        /// <summary>
        /// Gets or sets the block height.
        /// </summary>
        public long Height { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the transaction IDs.
        /// </summary>
        public List<string> Tx { get; set; }
    }

    /// <summary>
    /// Represents a MultiChain balance.
    /// </summary>
    internal class MultiChainBalance
    {
        /// <summary>
        /// Gets or sets the asset name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the asset quantity.
        /// </summary>
        public decimal Qty { get; set; }
    }
}
