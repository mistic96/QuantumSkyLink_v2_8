using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumLedger.Models.Blockchain;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Models.Blockchain.Repositories
{
    /// <summary>
    /// Repository interface for blockchain events.
    /// </summary>
    public interface IBlockchainEventRepository : IRepository<BlockchainEvent>
    {
        /// <summary>
        /// Gets events by their type.
        /// </summary>
        /// <param name="eventType">The type of events to retrieve.</param>
        /// <returns>A collection of events of the specified type.</returns>
        Task<IEnumerable<BlockchainEvent>> GetByTypeAsync(string eventType);

        /// <summary>
        /// Gets events within a specific time range.
        /// </summary>
        /// <param name="start">The start of the time range.</param>
        /// <param name="end">The end of the time range.</param>
        /// <returns>A collection of events within the specified time range.</returns>
        Task<IEnumerable<BlockchainEvent>> GetByTimeRangeAsync(DateTime start, DateTime end);

        /// <summary>
        /// Gets events of a specific type T.
        /// </summary>
        /// <typeparam name="T">The type of events to retrieve.</typeparam>
        /// <param name="since">Optional timestamp to filter events after a specific time.</param>
        /// <returns>A collection of events of type T.</returns>
        Task<IEnumerable<T>> GetByTypeAsync<T>(DateTime? since = null) where T : BlockchainEvent;
    }
}
