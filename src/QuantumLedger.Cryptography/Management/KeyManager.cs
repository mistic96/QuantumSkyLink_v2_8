using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Constants;
using QuantumLedger.Cryptography.Exceptions;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Models;
using QuantumLedger.Cryptography.Storage;
using QuantumLedger.Cryptography.Utils;

namespace QuantumLedger.Cryptography.Management;

/// <summary>
/// Manages cryptographic keys including generation, storage, and lifecycle
/// </summary>
public class KeyManager : IKeyManager
{
    private readonly IKeyStorage _storage;
    private readonly ILogger<KeyManager> _logger;
    private readonly Dictionary<string, Func<(byte[], byte[])>> _keyGenerators;

    /// <summary>
    /// Creates a new instance of KeyManager
    /// </summary>
    /// <param name="storage">Key storage implementation</param>
    /// <param name="logger">Logger instance</param>
    public KeyManager(IKeyStorage storage, ILogger<KeyManager> logger)
    {
        _storage = storage;
        _logger = logger;

        // Register key generators for supported algorithms
        _keyGenerators = new Dictionary<string, Func<(byte[], byte[])>>
        {
            [CryptoConstants.Algorithms.EC256] = KeyGenerationHelper.GenerateEC256KeyPair,
            [CryptoConstants.Algorithms.Dilithium] = KeyGenerationHelper.GenerateDilithiumKeyPair,
            [CryptoConstants.Algorithms.Falcon] = KeyGenerationHelper.GenerateFalconKeyPair
        };
    }

    /// <inheritdoc/>
    public async Task<string> GenerateKeyPairAsync(string address, string algorithm, KeyCategory category, int? version = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);
        ArgumentException.ThrowIfNullOrEmpty(algorithm);

        try
        {
            // Check if algorithm is supported
            if (!_keyGenerators.TryGetValue(algorithm, out var generator))
            {
                throw new ArgumentException($"Unsupported algorithm: {algorithm}");
            }

            // Get next version if not specified
            if (!version.HasValue)
            {
                var activeKeys = await _storage.ListActiveKeysAsync(algorithm, category);
                version = activeKeys.Where(k => k.Address == address)
                                  .Select(k => k.Version)
                                  .DefaultIfEmpty(0)
                                  .Max() + 1;
            }

            // Generate key pair
            var (privateKey, publicKey) = generator();

            // Create key identifier
            var keyId = $"{address}_{algorithm}_v{version}";

            // Store key data
            await _storage.StoreKeyAsync(keyId, address, privateKey, algorithm, category);

            _logger.LogInformation(
                "Generated new key pair: {KeyId} (Address: {Address}, Algorithm: {Algorithm}, Category: {Category}, Version: {Version})",
                keyId, address, algorithm, category, version);

            return keyId;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to generate key pair for address {Address} using algorithm {Algorithm}", address, algorithm);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Generate,
                algorithm,
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetPublicKeyAsync(string keyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        try
        {
            var keyData = await _storage.RetrieveKeyAsync(keyId);
            if (keyData == null)
            {
                throw new ArgumentException($"Key not found: {keyId}");
            }

            // TODO: Extract public key from private key data
            return keyData;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to retrieve public key {KeyId}", keyId);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetPrivateKeyAsync(string keyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        try
        {
            var keyData = await _storage.RetrieveKeyAsync(keyId);
            if (keyData == null)
            {
                throw new ArgumentException($"Key not found: {keyId}");
            }

            return keyData;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to retrieve private key {KeyId}", keyId);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetAlgorithmAsync(string keyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        try
        {
            var activeKeys = await _storage.ListActiveKeysAsync();
            var key = activeKeys.FirstOrDefault(k => k.Id == keyId);
            if (key == null)
            {
                throw new ArgumentException($"Key not found: {keyId}");
            }

            return key.Algorithm;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to retrieve algorithm for key {KeyId}", keyId);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string> RotateKeysAsync(string address, KeyCategory category)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            // Get current key
            var currentKey = await GetCurrentKeyAsync(address, category);
            if (currentKey == null)
            {
                throw new ArgumentException($"No existing keys for address {address} with category {category}");
            }

            // Generate new key with next version
            var nextVersion = currentKey.Version + 1;
            var newKeyId = await GenerateKeyPairAsync(address, currentKey.Algorithm, category, nextVersion);

            // Mark current key as rotated
            await _storage.RevokeKeyAsync(currentKey.Id);

            _logger.LogInformation(
                "Rotated keys for address {Address} category {Category}: {OldKeyId} -> {NewKeyId}",
                address, category, currentKey.Id, newKeyId);

            return newKeyId;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to rotate keys for address {Address} category {Category}", address, category);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Generate,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int?> GetVersionAsync(string keyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        try
        {
            var activeKeys = await _storage.ListActiveKeysAsync();
            var key = activeKeys.FirstOrDefault(k => k.Id == keyId);
            if (key == null)
            {
                throw new ArgumentException($"Key not found: {keyId}");
            }

            return key.Version;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to retrieve version for key {KeyId}", keyId);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int?> GetLatestVersionAsync(string address, KeyCategory category)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var activeKeys = await _storage.ListActiveKeysAsync(null, category);
            return activeKeys.Where(k => k.Address == address)
                           .Select(k => k.Version)
                           .DefaultIfEmpty()
                           .Max();
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to get latest version for address {Address} category {Category}", address, category);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetLatestKeyPairAsync(string address, KeyCategory category)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var activeKeys = await _storage.ListActiveKeysAsync(null, category);
            var latestKey = activeKeys.Where(k => k.Address == address)
                                    .OrderByDescending(k => k.Version)
                                    .FirstOrDefault();
            return latestKey?.Id;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to get latest key pair for address {Address} category {Category}", address, category);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<KeyEntity>> GetAddressKeysAsync(string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var activeKeys = await _storage.ListActiveKeysAsync();
            return activeKeys.Where(k => k.Address == address);
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to get keys for address {Address}", address);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<KeyEntity?> GetCurrentKeyAsync(string address, KeyCategory category)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var activeKeys = await _storage.ListActiveKeysAsync(null, category);
            return activeKeys.Where(k => k.Address == address)
                           .OrderByDescending(k => k.Version)
                           .FirstOrDefault();
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to get current key for address {Address} category {Category}", address, category);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                "Unknown",
                ex);
        }
    }
}
