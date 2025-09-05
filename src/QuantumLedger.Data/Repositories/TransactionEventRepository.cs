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
    /// Repository implementation for transaction events.
    /// </summary>
    public class TransactionEventRepository : BaseRepository<TransactionSubmittedEvent>, Models.Blockchain.Repositories.ITransactionEventRepository
    {
        private const string EntityType = "transaction_event";

        /// <summary>
        /// Initializes a new instance of the TransactionEventRepository class.
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        public TransactionEventRepository(IDataStore dataStore)
            : base(dataStore, EntityType)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TransactionSubmittedEvent>> GetByAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or whitespace", nameof(address));

            var allEvents = await GetAllAsync();
            return allEvents.Where(e => 
                e.Transaction.FromAddress == address || 
                e.Transaction.ToAddress == address);
        }

        /// <summary>
        /// Gets the ID of the transaction event.
        /// </summary>
        /// <param name="entity">The transaction event.</param>
        /// <returns>The event's ID.</returns>
        protected override string GetEntityId(TransactionSubmittedEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Id;
        }
    }
}
