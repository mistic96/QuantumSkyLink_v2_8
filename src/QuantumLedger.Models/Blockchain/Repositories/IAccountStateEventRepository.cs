using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumLedger.Models.Blockchain;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Models.Blockchain.Repositories
{
    /// <summary>
    /// Repository interface for account state events.
    /// </summary>
    public interface IAccountStateEventRepository : IRepository<AccountStateChangedEvent>
    {
        /// <summary>
        /// Gets account state events by address.
        /// </summary>
        /// <param name="address">The blockchain address to query.</param>
        /// <returns>A collection of account state events for the specified address.</returns>
        Task<IEnumerable<AccountStateChangedEvent>> GetByAddressAsync(string address);

        /// <summary>
        /// Gets the latest account state event for an address.
        /// </summary>
        /// <param name="address">The blockchain address to query.</param>
        /// <returns>The latest account state event for the specified address, or null if not found.</returns>
        Task<AccountStateChangedEvent> GetLatestByAddressAsync(string address);
    }
}
