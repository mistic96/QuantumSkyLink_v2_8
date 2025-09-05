using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumLedger.Models.Blockchain;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Models.Blockchain.Repositories
{
    /// <summary>
    /// Repository interface for transaction events.
    /// </summary>
    public interface ITransactionEventRepository : IRepository<TransactionSubmittedEvent>
    {
        /// <summary>
        /// Gets transaction events by address (sender or recipient).
        /// </summary>
        /// <param name="address">The blockchain address to query.</param>
        /// <returns>A collection of transaction events involving the specified address.</returns>
        Task<IEnumerable<TransactionSubmittedEvent>> GetByAddressAsync(string address);
    }
}
