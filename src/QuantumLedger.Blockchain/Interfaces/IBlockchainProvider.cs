using System.Threading.Tasks;

namespace QuantumLedger.Blockchain.Interfaces
{
    public interface IBlockchainProvider
    {
        Task<string> GetNetworkAsync();
        Task<string> CreateWalletAsync();
        Task<string> SendTransactionAsync(string from, string to, decimal amount);
        Task<decimal> GetBalanceAsync(string address);
        Task<string> SignMessageAsync(string message, string privateKey);
        Task<bool> VerifySignatureAsync(string message, string signature, string address);
        Task<string> GetTransactionStatusAsync(string txHash);
        Task<string> DeployContractAsync(string bytecode, string abi);
    }
}
