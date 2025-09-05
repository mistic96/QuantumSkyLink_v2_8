using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumLedger.Data.Storage;
using QuantumLedger.Models;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Data.Repositories
{
    /// <summary>
    /// Repository implementation for AccountBalance entities.
    /// </summary>
    public class AccountBalanceRepository : BaseRepository<AccountBalance>, IAccountBalanceRepository
    {
        private const string EntityType = "account";
        private readonly object _balanceLock = new object();

        /// <summary>
        /// Initializes a new instance of the AccountBalanceRepository class.
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        public AccountBalanceRepository(IDataStore dataStore)
            : base(dataStore, EntityType)
        {
        }

        /// <inheritdoc/>
        public async Task<AccountBalance> GetByAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or whitespace", nameof(address));

            return await GetByIdAsync(address);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateBalanceAsync(string address, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address cannot be null or whitespace", nameof(address));

            // Use a lock to ensure atomic balance updates
            lock (_balanceLock)
            {
                // Get current balance
                var account = GetByAddressAsync(address).Result;
                if (account == null)
                {
                    // Create new account if it doesn't exist
                    account = new AccountBalance
                    {
                        Address = address,
                        Balance = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                }

                // Update balance
                if (!account.UpdateBalance(amount))
                {
                    return false; // Balance update failed (e.g., would result in negative balance)
                }

                // Save updated balance
                return UpdateAsync(account).Result;
            }
        }

        /// <summary>
        /// Gets the ID of the AccountBalance entity.
        /// </summary>
        /// <param name="entity">The AccountBalance entity.</param>
        /// <returns>The entity's ID.</returns>
        protected override string GetEntityId(AccountBalance entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Address;
        }

        /// <inheritdoc/>
        public override async Task<bool> UpdateAsync(AccountBalance entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Update LastUpdated timestamp
            entity.LastUpdated = DateTime.UtcNow;

            return await base.UpdateAsync(entity);
        }

        /// <inheritdoc/>
        public override async Task<AccountBalance> AddAsync(AccountBalance entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Set initial LastUpdated timestamp
            entity.LastUpdated = DateTime.UtcNow;

            return await base.AddAsync(entity);
        }
    }
}
