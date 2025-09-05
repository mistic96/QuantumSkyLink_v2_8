using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuantumLedger.Data.Storage
{
    /// <summary>
    /// Defines the contract for the immutable data store in the Quantum Ledger system.
    /// </summary>
    public interface IDataStore
    {
        /// <summary>
        /// Appends data to the store with the specified key.
        /// </summary>
        /// <param name="key">The unique identifier for the data.</param>
        /// <param name="data">The data to append.</param>
        /// <returns>True if the append operation was successful; otherwise, false.</returns>
        Task<bool> AppendAsync(string key, byte[] data);

        /// <summary>
        /// Retrieves data from the store by its key.
        /// </summary>
        /// <param name="key">The unique identifier of the data to retrieve.</param>
        /// <returns>The data if found; otherwise, null.</returns>
        Task<byte[]> RetrieveAsync(string key);

        /// <summary>
        /// Lists all keys in the store that start with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to filter keys by.</param>
        /// <returns>An enumerable collection of matching keys.</returns>
        Task<IEnumerable<string>> ListKeysAsync(string prefix);
    }
}
