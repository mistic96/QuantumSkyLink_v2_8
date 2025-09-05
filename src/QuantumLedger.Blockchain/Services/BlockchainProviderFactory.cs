using Microsoft.Extensions.Logging;
using QuantumLedger.Blockchain.Interfaces;
using QuantumLedger.Blockchain.Services.Evm;
using QuantumLedger.Blockchain.Services.Solana;
using QuantumLedger.Blockchain.Services.Sui;
using QuantumLedger.Blockchain.Services.Tron;

namespace QuantumLedger.Blockchain.Services;

public class BlockchainProviderFactory : IBlockchainProviderFactory
{
    private readonly ILogger<BlockchainProviderFactory> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public BlockchainProviderFactory(
        ILogger<BlockchainProviderFactory> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public IBlockchainProvider GetProvider(string network)
    {
        _logger.LogInformation("Getting blockchain provider for network: {Network}", network);
        
        return network?.ToLowerInvariant() switch
        {
            "ethereum" or "eth" or "evm" => new EvmBlockchainProvider(),
            "binance" or "bsc" => new EvmBlockchainProvider(),
            "polygon" or "matic" => new EvmBlockchainProvider(),
            "avalanche" or "avax" => new EvmBlockchainProvider(),
            "arbitrum" or "arb" => new EvmBlockchainProvider(),
            "optimism" or "op" => new EvmBlockchainProvider(),
            "solana" or "sol" => new SolanaBlockchainProvider(),
            "sui" => new SuiBlockchainProvider(),
            "tron" or "trx" => new TronBlockchainProvider(),
            _ => throw new NotSupportedException($"Network {network} is not supported")
        };
    }
    
    public IEnumerable<string> GetSupportedNetworks()
    {
        return new[]
        {
            "ethereum", "binance", "polygon", "avalanche", 
            "arbitrum", "optimism", "solana", "sui", "tron"
        };
    }
    
    public bool IsNetworkSupported(string network)
    {
        var supportedNetworks = GetSupportedNetworks();
        return supportedNetworks.Contains(network?.ToLowerInvariant());
    }
}

public interface IBlockchainProviderFactory
{
    IBlockchainProvider GetProvider(string network);
    IEnumerable<string> GetSupportedNetworks();
    bool IsNetworkSupported(string network);
}
