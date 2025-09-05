using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuantumLedger.Blockchain.Interfaces
{
    public interface IMultisigService
    {
        Task<string> CreateMultisigWalletAsync(List<string> owners, int requiredSignatures);
        Task<string> SubmitTransactionAsync(string walletAddress, string destination, decimal amount);
        Task<bool> ApproveTransactionAsync(string walletAddress, string transactionId, string approver);
        Task<bool> ExecuteTransactionAsync(string walletAddress, string transactionId);
        Task<List<string>> GetPendingTransactionsAsync(string walletAddress);
        Task<int> GetRequiredSignaturesAsync(string walletAddress);
        Task<List<string>> GetOwnersAsync(string walletAddress);
    }
}
