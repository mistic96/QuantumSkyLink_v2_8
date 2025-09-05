using System.Threading.Tasks;
using QuantumLedger.Models.Blockchain;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Models.Blockchain.Repositories
{
    /// <summary>
    /// Repository interface for block events.
    /// </summary>
    public interface IBlockEventRepository : IRepository<BlockCreatedEvent>
    {
        /// <summary>
        /// Gets a block event by its block number.
        /// </summary>
        /// <param name="blockNumber">The block number to query.</param>
        /// <returns>The block event with the specified block number, or null if not found.</returns>
        Task<BlockCreatedEvent> GetByBlockNumberAsync(long blockNumber);

        /// <summary>
        /// Gets a block event by its block hash.
        /// </summary>
        /// <param name="blockHash">The block hash to query.</param>
        /// <returns>The block event with the specified block hash, or null if not found.</returns>
        Task<BlockCreatedEvent> GetByBlockHashAsync(string blockHash);
    }
}
