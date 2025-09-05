using QuantumLedger.Cryptography.Models;

namespace QuantumLedger.Cryptography.Interfaces;

/// <summary>
/// Interface for key management operations
/// </summary>
public interface IKeyManager
{
    /// <summary>
    /// Generates a new key pair for an address
    /// </summary>
    /// <param name="address">The address to generate keys for</param>
    /// <param name="algorithm">The algorithm to generate keys for</param>
    /// <param name="category">The key category (Traditional or PQC)</param>
    /// <param name="version">Optional specific version number</param>
    /// <returns>The ID of the generated key pair</returns>
    Task<string> GenerateKeyPairAsync(string address, string algorithm, KeyCategory category, int? version = null);

    /// <summary>
    /// Gets the public key for a key pair
    /// </summary>
    /// <param name="keyId">The key pair ID</param>
    /// <returns>The public key bytes</returns>
    Task<byte[]> GetPublicKeyAsync(string keyId);

    /// <summary>
    /// Gets the private key for a key pair
    /// </summary>
    /// <param name="keyId">The key pair ID</param>
    /// <returns>The private key bytes</returns>
    Task<byte[]> GetPrivateKeyAsync(string keyId);

    /// <summary>
    /// Gets the algorithm for a key pair
    /// </summary>
    /// <param name="keyId">The key pair ID</param>
    /// <returns>The algorithm name</returns>
    Task<string> GetAlgorithmAsync(string keyId);

    /// <summary>
    /// Gets the version number for a key pair
    /// </summary>
    /// <param name="keyId">The key pair ID</param>
    /// <returns>The version number</returns>
    Task<int?> GetVersionAsync(string keyId);

    /// <summary>
    /// Gets the latest version number for an address and category
    /// </summary>
    /// <param name="address">The address to check</param>
    /// <param name="category">The key category</param>
    /// <returns>The latest version number</returns>
    Task<int?> GetLatestVersionAsync(string address, KeyCategory category);

    /// <summary>
    /// Rotates keys for an address
    /// </summary>
    /// <param name="address">The address to rotate keys for</param>
    /// <param name="category">The key category to rotate</param>
    /// <returns>The ID of the new key pair</returns>
    Task<string> RotateKeysAsync(string address, KeyCategory category);

    /// <summary>
    /// Gets the latest key pair ID for an address and category
    /// </summary>
    /// <param name="address">The address to get the latest key for</param>
    /// <param name="category">The key category</param>
    /// <returns>The key pair ID, or null if none exist</returns>
    Task<string?> GetLatestKeyPairAsync(string address, KeyCategory category);

    /// <summary>
    /// Gets all active keys for an address
    /// </summary>
    /// <param name="address">The address to get keys for</param>
    /// <returns>Collection of key entities</returns>
    Task<IEnumerable<KeyEntity>> GetAddressKeysAsync(string address);

    /// <summary>
    /// Gets the current active key for an address and category
    /// </summary>
    /// <param name="address">The address to get the key for</param>
    /// <param name="category">The key category</param>
    /// <returns>The key entity, or null if none exists</returns>
    Task<KeyEntity?> GetCurrentKeyAsync(string address, KeyCategory category);
}
