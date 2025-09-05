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
    /// Repository implementation for account state events.
    /// </summary>
    public class AccountStateEventRepository : BaseRepository<AccountStateChangedEvent>, QuantumLedger.Models.Blockchain.Repositories.IAccountStateEventRepository
    {
        private const string EntityType = "account_state_event";

        /// <summary>
        /// Initializes a new instance of the AccountStateEventRepository class.
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        public AccountStateEventRepository(IDataStore dataStore)
            : base(dataStore, EntityType)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AccountStateChangedEvent>> GetByAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or whitespace", nameof(address));

            var allEvents = await GetAllAsync();
            return allEvents.Where(e => e.Address == address);
        }

        /// <inheritdoc/>
        public async Task<AccountStateChangedEvent> GetLatestByAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or whitespace", nameof(address));

            var addressEvents = await GetByAddressAsync(address);
            return addressEvents.OrderByDescending(e => e.Timestamp).FirstOrDefault();
        }

        /// <summary>
        /// Gets the ID of the account state event.
        /// </summary>
        /// <param name="entity">The account state event.</param>
        /// <returns>The event's ID.</returns>
        protected override string GetEntityId(AccountStateChangedEvent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Id;
        }
    }
}
