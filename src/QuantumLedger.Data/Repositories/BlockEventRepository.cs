using System;
using System.Linq;
using System.Threading.Tasks;
using QuantumLedger.Models.Blockchain;
using QuantumLedger.Models.Blockchain.Repositories;
using QuantumLedger.Data.Storage;

namespace QuantumLedger.Data.Repositories
{
    /// <summary>
    /// Repository implementation for block events.
    /// </summary>
    public class BlockEventRepository : BaseRepository<BlockCreatedEvent>, Models.Blockchain.Repositories.IBlockEventRepository
    {
        private const string EntityType = "block_event";

        /// <summary>
        /// Initializes a new instance of the BlockEventRepository class.
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        public BlockEventRepository(IDataStore dataStore)
            : base(dataStore, EntityType)
        {
        }

        /// <inheritdoc/>
        public async Task<BlockCreatedEvent> GetByBlockNumberAsync(long blockNumber)
        {
            if (blockNumber < 0)
                throw new ArgumentException("Block number must be non-negative", nameof(blockNumber));

            var allEvents = await GetAllAsync();
            return allEvents.FirstOrDefault(e => e.Block.Number == blockNumber);
        }

        /// <inheritdoc/>
        public async Task<BlockCreatedEvent> GetByBlockHashAsync(string blockHash)
        {
            if (string.IsNullOrWhiteSpace(blockHash))
                throw new ArgumentException("Block hash cannot be null or whitespace", nameof(blockHash));

            var allEvents = await GetAllAsync();
            return allEvents.FirstOrDefault(e => e.Block.Hash == blockHash);
        }

        /// <summary>
        /// Gets the ID of the block event.
        /// </summary>
        /// <param name="entity">The block event.</param>
        /// <returns>The event's ID.</returns>
        protected override string GetEntityId(BlockCreatedEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Id;
        }
    }
}
