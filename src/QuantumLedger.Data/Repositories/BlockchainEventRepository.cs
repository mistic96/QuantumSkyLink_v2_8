using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuantumLedger.Models.Blockchain;
using QuantumLedger.Models.Blockchain.Repositories;
using QuantumLedger.Data.Storage;

namespace QuantumLedger.Data.Repositories
{
    /// <summary>
    /// Repository implementation for blockchain events.
    /// </summary>
    public class BlockchainEventRepository : BaseRepository<BlockchainEvent>, Models.Blockchain.Repositories.IBlockchainEventRepository
    {
        private const string EntityType = "blockchain_event";

        /// <summary>
        /// Initializes a new instance of the BlockchainEventRepository class.
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        public BlockchainEventRepository(IDataStore dataStore)
            : base(dataStore, EntityType)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlockchainEvent>> GetByTypeAsync(string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be null or whitespace", nameof(eventType));

            var allEvents = await GetAllAsync();
            return allEvents.Where(e => e.EventType == eventType);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlockchainEvent>> GetByTimeRangeAsync(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentException("Start time must be before end time", nameof(start));

            var allEvents = await GetAllAsync();
            return allEvents.Where(e => e.Timestamp >= start && e.Timestamp <= end);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> GetByTypeAsync<T>(DateTime? since = null) where T : BlockchainEvent
        {
            var allEvents = await GetAllAsync();
            var filteredEvents = allEvents.OfType<T>();

            if (since.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.Timestamp >= since.Value);
            }

            return filteredEvents;
        }

        /// <summary>
        /// Gets the ID of the blockchain event.
        /// </summary>
        /// <param name="entity">The blockchain event.</param>
        /// <returns>The event's ID.</returns>
        protected override string GetEntityId(BlockchainEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Id;
        }
    }
}
