using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumLedger.Cryptography.Models;

namespace QuantumLedger.Cryptography.Storage
{
    /// <summary>
    /// Interface for key storage operations
    /// </summary>
    public interface IKeyStorage
    {
        /// <summary>
        /// Stores a key with the given identifier
        /// </summary>
        Task StoreKeyAsync(string identifier, string address, byte[] keyData, string algorithm, KeyCategory category, DateTime? expiresAt = null);

        /// <summary>
        /// Retrieves a key by its identifier
        /// </summary>
        Task<byte[]> RetrieveKeyAsync(string identifier);

        /// <summary>
        /// Revokes a key by its identifier
        /// </summary>
        Task RevokeKeyAsync(string identifier);

        /// <summary>
        /// Creates a new version of an existing key
        /// </summary>
        Task<string> RotateKeyAsync(string identifier, byte[] newKeyData = null);

        /// <summary>
        /// Updates the expiration date of a key
        /// </summary>
        Task UpdateExpirationAsync(string identifier, DateTime newExpirationDate);

        /// <summary>
        /// Lists all active keys of a specific type
        /// </summary>
        Task<IEnumerable<KeyEntity>> ListActiveKeysAsync(string algorithm = null, KeyCategory? category = null);
    }
}
