using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumLedger.Data.Storage;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Data.Repositories
{
    /// <summary>
    /// Base repository implementation providing common functionality for all repositories.
    /// </summary>
    /// <typeparam name="T">The type of entity this repository handles.</typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        private readonly IDataStore _dataStore;
        private readonly string _entityType;

        /// <summary>
        /// Initializes a new instance of the BaseRepository class.
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        /// <param name="entityType">The type name of the entity for storage keys.</param>
        protected BaseRepository(IDataStore dataStore, string entityType)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null or whitespace", nameof(id));

            var key = SerializationHelper.GenerateKey(_entityType, id);
            var data = await _dataStore.RetrieveAsync(key);
            return data != null ? await SerializationHelper.DeserializeFromBytesAsync<T>(data) : null;
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            var keys = await _dataStore.ListKeysAsync($"{_entityType}:");
            var result = new List<T>();

            foreach (var key in keys)
            {
                var data = await _dataStore.RetrieveAsync(key);
                if (data != null)
                {
                    var entity = await SerializationHelper.DeserializeFromBytesAsync<T>(data);
                    if (entity != null)
                    {
                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var id = GetEntityId(entity);
            if (string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException("Entity ID cannot be null or whitespace");

            var key = SerializationHelper.GenerateKey(_entityType, id);
            var data = await SerializationHelper.SerializeToBytesAsync(entity);
            var success = await _dataStore.AppendAsync(key, data);

            return success ? entity : null;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // In our immutable store, update is the same as add
            // The store will maintain the history, and the latest version will be retrieved
            var result = await AddAsync(entity);
            return result != null;
        }

        /// <summary>
        /// Gets the ID of the entity. Must be implemented by derived classes.
        /// </summary>
        /// <param name="entity">The entity to get the ID from.</param>
        /// <returns>The entity's ID.</returns>
        protected abstract string GetEntityId(T entity);
    }
}
