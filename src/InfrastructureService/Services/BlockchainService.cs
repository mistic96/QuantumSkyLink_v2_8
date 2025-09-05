using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.HdWallet;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using InfrastructureService.Services.Interfaces;
using System.Numerics;

namespace InfrastructureService.Services;

public class BlockchainService : IBlockchainService
{
    private readonly ILogger<BlockchainService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _networkUrls;

    public BlockchainService(ILogger<BlockchainService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Initialize network URLs from Aspire connection strings and local configuration
        // Build network URLs with preference order:
        // 1. Aspire connection string (GetConnectionString)
        // 2. SixNetworkConfiguration RpcUrl
        // 3. ExternalServices AlchemyApiKey (if available) -> build Alchemy template
        // 4. Infura fallback URL
        var alchemyKey = _configuration["ExternalServices:AlchemyApiKey"] 
                         ?? _configuration["ALCHEMY_KEY_SEPOLIA_UAT"] 
                         ?? _configuration["ALCHEMY_KEY_UAT"] 
                         ?? string.Empty;

        string BuildUrl(string connName, string configPath, string alchemyTemplate, string infuraFallback)
        {
            var conn = _configuration.GetConnectionString(connName);
            if (!string.IsNullOrEmpty(conn))
                return conn;

            var cfg = _configuration[configPath];
            if (!string.IsNullOrEmpty(cfg))
                return cfg;

            if (!string.IsNullOrEmpty(alchemyKey) && !string.IsNullOrEmpty(alchemyTemplate))
                return alchemyTemplate.Replace("{ALCHEMY_API_KEY}", alchemyKey);

            return infuraFallback;
        }

        _networkUrls = new Dictionary<string, string>
        {
            { "MultiChain", _configuration.GetConnectionString("multichain-external") ?? _configuration["SixNetworkConfiguration:Network1_MultiChain:RpcUrl"] ?? "http://localhost:7446" },
            { "Ethereum", BuildUrl("ethereum-sepolia", "SixNetworkConfiguration:Network2_EthereumSepolia:RpcUrl", "https://eth-sepolia.g.alchemy.com/v2/{ALCHEMY_API_KEY}", "https://sepolia.infura.io/v3/37e72ed233a54416abc10f8af0243e70") },
            { "Polygon", BuildUrl("polygon-mumbai", "SixNetworkConfiguration:Network3_PolygonMumbai:RpcUrl", "https://polygon-mumbai.g.alchemy.com/v2/{ALCHEMY_API_KEY}", "https://polygon-mumbai.infura.io/v3/37e72ed233a54416abc10f8af0243e70") },
            { "Arbitrum", BuildUrl("arbitrum-sepolia", "SixNetworkConfiguration:Network4_ArbitrumSepolia:RpcUrl", "https://arb-sepolia.g.alchemy.com/v2/{ALCHEMY_API_KEY}", "https://arbitrum-sepolia.infura.io/v3/37e72ed233a54416abc10f8af0243e70") },
            { "Bitcoin", _configuration.GetConnectionString("bitcoin-testnet") ?? _configuration["SixNetworkConfiguration:Network5_BitcoinTestnet:RpcUrl"] ?? "https://testnet.blockstream.info/api" },
            { "BSC", _configuration.GetConnectionString("bsc-testnet") ?? _configuration["SixNetworkConfiguration:Network6_BSCTestnet:RpcUrl"] ?? "https://data-seed-prebsc-1-s1.binance.org:8545" }
        };

        _logger.LogInformation("Initialized BlockchainService with {NetworkCount} networks", _networkUrls.Count);
        foreach (var network in _networkUrls)
        {
            _logger.LogInformation("Network {NetworkName}: {RpcUrl}", network.Key, network.Value);
        }
    }

    // Wallet Generation and Management
    public async Task<(string address, string privateKey, string publicKey)> GenerateWalletAsync(string network)
    {
        try
        {
            var ecKey = EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var address = ecKey.GetPublicAddress();
            var publicKey = ecKey.GetPubKey().ToHex();

            _logger.LogInformation("Generated new wallet address: {Address} for network: {Network}", address, network);

            return (address, privateKey, publicKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating wallet for network: {Network}", network);
            throw;
        }
    }

    public async Task<(string address, string privateKey, string publicKey)> GenerateHdWalletAsync(string network, string mnemonic, int accountIndex = 0)
    {
        try
        {
            var wallet = new Wallet(mnemonic, null);
            var account = wallet.GetAccount(accountIndex);
            
            var address = account.Address;
            var privateKey = account.PrivateKey;
            var publicKey = account.PublicKey;

            _logger.LogInformation("Generated HD wallet address: {Address} for network: {Network}, account index: {AccountIndex}", 
                address, network, accountIndex);

            return (address, privateKey, publicKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HD wallet for network: {Network}, account index: {AccountIndex}", network, accountIndex);
            throw;
        }
    }

    public async Task<string> GetPublicKeyFromPrivateKeyAsync(string privateKey)
    {
        try
        {
            var ecKey = new EthECKey(privateKey);
            return ecKey.GetPubKey().ToHex();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public key from private key");
            throw;
        }
    }

    public async Task<string> GetAddressFromPrivateKeyAsync(string privateKey)
    {
        try
        {
            var ecKey = new EthECKey(privateKey);
            return ecKey.GetPublicAddress();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting address from private key");
            throw;
        }
    }

    // Balance Operations
    public async Task<decimal> GetEthBalanceAsync(string address, string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(address);
            return Web3.Convert.FromWei(balance.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ETH balance for address: {Address} on network: {Network}", address, network);
            return 0; // Return 0 instead of throwing for balance queries
        }
    }

    public async Task<decimal> GetTokenBalanceAsync(string address, string tokenAddress, string network)
    {
        try
        {
            // TODO: Implement ERC-20 token balance query
            // This is a placeholder implementation
            _logger.LogWarning("Token balance query not yet implemented for address: {Address}, token: {TokenAddress}, network: {Network}", 
                address, tokenAddress, network);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token balance for address: {Address}, token: {TokenAddress}, network: {Network}", 
                address, tokenAddress, network);
            return 0;
        }
    }

    public async Task<decimal> GetTokenBalanceWithDecimalsAsync(string address, string tokenAddress, int decimals, string network)
    {
        try
        {
            var balance = await GetTokenBalanceAsync(address, tokenAddress, network);
            return balance / (decimal)Math.Pow(10, decimals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token balance with decimals for address: {Address}, token: {TokenAddress}, network: {Network}", 
                address, tokenAddress, network);
            return 0;
        }
    }

    // Transaction Operations
    public async Task<string> SendEthTransactionAsync(string fromPrivateKey, string toAddress, decimal amount, decimal gasPrice, long gasLimit, string network)
    {
        try
        {
            // TODO: Implement ETH transaction sending
            // This is a placeholder implementation
            _logger.LogWarning("ETH transaction sending not yet implemented");
            throw new NotImplementedException("ETH transaction sending will be implemented in the next phase");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ETH transaction");
            throw;
        }
    }

    public async Task<string> SendTokenTransactionAsync(string fromPrivateKey, string toAddress, string tokenAddress, decimal amount, decimal gasPrice, long gasLimit, string network)
    {
        try
        {
            // TODO: Implement token transaction sending
            // This is a placeholder implementation
            _logger.LogWarning("Token transaction sending not yet implemented");
            throw new NotImplementedException("Token transaction sending will be implemented in the next phase");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending token transaction");
            throw;
        }
    }

    public async Task<string> SendRawTransactionAsync(string signedTransaction, string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var txHash = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);
            return txHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending raw transaction on network: {Network}", network);
            throw;
        }
    }

    // Gas Estimation and Pricing
    public async Task<long> EstimateGasAsync(string fromAddress, string toAddress, decimal amount, string? data, string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var amountInWei = Web3.Convert.ToWei(amount);
            
            var callInput = new Nethereum.RPC.Eth.DTOs.CallInput
            {
                From = fromAddress,
                To = toAddress,
                Value = new HexBigInteger(amountInWei),
                Data = data
            };

            var gasEstimate = await web3.Eth.Transactions.EstimateGas.SendRequestAsync(callInput);
            return (long)gasEstimate.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating gas for transaction on network: {Network}", network);
            return 21000; // Return default gas limit for simple transfers
        }
    }

    public async Task<long> EstimateTokenGasAsync(string fromAddress, string toAddress, string tokenAddress, decimal amount, string network)
    {
        try
        {
            // TODO: Implement token gas estimation
            // This is a placeholder implementation
            _logger.LogWarning("Token gas estimation not yet implemented");
            return 65000; // Default gas limit for token transfers
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating token gas");
            return 65000;
        }
    }

    public async Task<decimal> GetCurrentGasPriceAsync(string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            return Web3.Convert.FromWei(gasPrice.Value, UnitConversion.EthUnit.Gwei);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current gas price for network: {Network}", network);
            return 20; // Return default gas price in Gwei
        }
    }

    public async Task<decimal> GetFastGasPriceAsync(string network)
    {
        try
        {
            var currentGasPrice = await GetCurrentGasPriceAsync(network);
            return currentGasPrice * 1.2m; // 20% higher for fast transactions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fast gas price for network: {Network}", network);
            return 25; // Return default fast gas price in Gwei
        }
    }

    // Transaction Information
    public async Task<long> GetTransactionCountAsync(string address, string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address);
            return (long)nonce.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction count for address: {Address} on network: {Network}", address, network);
            return 0;
        }
    }

    public async Task<(bool isConfirmed, long? blockNumber, string? status)> GetTransactionStatusAsync(string transactionHash, string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            
            if (receipt == null)
            {
                return (false, null, "Pending");
            }

            var isSuccess = receipt.Status?.Value == 1;
            return (true, (long)receipt.BlockNumber.Value, isSuccess ? "Success" : "Failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction status for hash: {TransactionHash} on network: {Network}", transactionHash, network);
            return (false, null, "Unknown");
        }
    }

    public async Task<(decimal gasUsed, bool success, string? errorMessage)> GetTransactionReceiptAsync(string transactionHash, string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            
            if (receipt == null)
            {
                return (0, false, "Transaction not found");
            }

            var gasUsed = Web3.Convert.FromWei(receipt.GasUsed.Value);
            var success = receipt.Status?.Value == 1;
            
            return (gasUsed, success, success ? null : "Transaction failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction receipt for hash: {TransactionHash} on network: {Network}", transactionHash, network);
            return (0, false, ex.Message);
        }
    }

    // Network Information
    public async Task<long> GetLatestBlockNumberAsync(string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return (long)blockNumber.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest block number for network: {Network}", network);
            return 0;
        }
    }

    public async Task<bool> IsValidAddressAsync(string address)
    {
        try
        {
            return address.IsValidEthereumAddressHexFormat();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating address: {Address}", address);
            return false;
        }
    }

    public async Task<bool> IsContractAddressAsync(string address, string network)
    {
        try
        {
            var web3 = GetWeb3Instance(network);
            var code = await web3.Eth.GetCode.SendRequestAsync(address);
            return !string.IsNullOrEmpty(code) && code != "0x";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if address is contract: {Address} on network: {Network}", address, network);
            return false;
        }
    }

    // Multi-Signature Operations - Placeholder implementations
    public async Task<string> CreateMultiSigTransactionAsync(string[] signerAddresses, int requiredSignatures, string toAddress, decimal amount, string network)
    {
        // TODO: Implement multi-sig transaction creation
        throw new NotImplementedException("Multi-sig transaction creation will be implemented in the next phase");
    }

    public async Task<string> SignMultiSigTransactionAsync(string privateKey, string transactionData)
    {
        // TODO: Implement multi-sig transaction signing
        throw new NotImplementedException("Multi-sig transaction signing will be implemented in the next phase");
    }

    public async Task<bool> VerifySignatureAsync(string message, string signature, string signerAddress)
    {
        try
        {
            var signer = new EthereumMessageSigner();
            var recoveredAddress = signer.EncodeUTF8AndEcRecover(message, signature);
            return string.Equals(recoveredAddress, signerAddress, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying signature");
            return false;
        }
    }

    // Token Information - Placeholder implementations
    public async Task<(string name, string symbol, int decimals)> GetTokenInfoAsync(string tokenAddress, string network)
    {
        // TODO: Implement token info retrieval
        throw new NotImplementedException("Token info retrieval will be implemented in the next phase");
    }

    public async Task<decimal> GetTokenTotalSupplyAsync(string tokenAddress, string network)
    {
        // TODO: Implement token total supply retrieval
        throw new NotImplementedException("Token total supply retrieval will be implemented in the next phase");
    }

    // Utility Functions
    public async Task<string> ConvertWeiToEthAsync(decimal wei)
    {
        var weiBigInteger = new BigInteger(wei);
        return Web3.Convert.FromWei(weiBigInteger).ToString();
    }

    public async Task<decimal> ConvertEthToWeiAsync(decimal eth)
    {
        var weiBigInteger = Web3.Convert.ToWei(eth);
        return (decimal)weiBigInteger;
    }

    public async Task<decimal> ConvertTokenAmountAsync(decimal amount, int fromDecimals, int toDecimals)
    {
        var factor = (decimal)Math.Pow(10, toDecimals - fromDecimals);
        return amount * factor;
    }

    public async Task<bool> ValidateTransactionDataAsync(string data)
    {
        try
        {
            // Basic validation - check if it's valid hex
            if (string.IsNullOrEmpty(data))
                return true;

            if (!data.StartsWith("0x"))
                return false;

            // Try to parse as hex
            Convert.FromHexString(data[2..]);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Private helper methods
    private Web3 GetWeb3Instance(string network)
    {
        if (!_networkUrls.TryGetValue(network, out var rpcUrl))
        {
            throw new ArgumentException($"Unsupported network: {network}");
        }

        return new Web3(rpcUrl);
    }
}
