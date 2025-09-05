using LiquidStorageCloud.DataManagement.Core.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace QuantumLedger.Cryptography.Storage
{
    /// <summary>
    /// Implements hybrid key storage using both SurrealDB and S3
    /// </summary>
    public class HybridKeyStorage : IKeyStorage
    {
        private readonly ISurrealRepository<KeyEntity> _repository;
        private readonly KeyStorageS3Service _s3Service;
        private readonly ILogger<HybridKeyStorage> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _defaultKeyExpirationDays;
        private readonly bool _automaticKeyRotationEnabled;

        public HybridKeyStorage(
            ISurrealRepository<KeyEntity> repository,
            KeyStorageS3Service s3Service,
            IConfiguration configuration,
            ILogger<HybridKeyStorage> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _defaultKeyExpirationDays = _configuration.GetValue<int>("KeyStorage:DefaultKeyExpirationDays", 90);
            _automaticKeyRotationEnabled = _configuration.GetValue<bool>("KeyStorage:AutomaticKeyRotationEnabled", true);
        }

        /// <inheritdoc />
        public async Task StoreKeyAsync(string identifier, string address, byte[] keyData, string algorithm, KeyCategory category, DateTime? expiresAt = null)
        {
            try
            {
                // Store encrypted key data in S3
                var s3Reference = await _s3Service.StoreKeyAsync(identifier, keyData);

                // Create key entity
                var keyEntity = new KeyEntity
                {
                    Id = identifier,
                    Address = address,
                    Algorithm = algorithm,
                    Category = category,
                    KeyIdentifier = identifier,
                    EncryptedKeyData = Convert.ToBase64String(keyData),
                    S3Reference = s3Reference,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow,
                    Version = 1,
                    ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(_defaultKeyExpirationDays),
                    Metadata = new Dictionary<string, string>
                    {
                        { "CreatedBy", "HybridKeyStorage" },
                        { "StorageType", "S3AndSurrealDB" }
                    },
                    KmsKeyId = _configuration["AWS:KMS:MasterKeyId"] ?? throw new InvalidOperationException("AWS:KMS:MasterKeyId is required"),
                    Checksum = ComputeChecksum(keyData)
                };

                // Store metadata in SurrealDB
                await _repository.CreateAsync(keyEntity);

                _logger.LogInformation("Successfully stored key {Identifier} for address {Address} using algorithm {Algorithm}", 
                    identifier, address, algorithm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing key {Identifier} for address {Address} using algorithm {Algorithm}", 
                    identifier, address, algorithm);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<byte[]> RetrieveKeyAsync(string identifier)
        {
            try
            {
                var keyEntity = await GetAndValidateKeyAsync(identifier);

                // Check if key is approaching expiration
                if (_automaticKeyRotationEnabled && IsKeyApproachingExpiration(keyEntity))
                {
                    // Start key rotation in background
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await RotateKeyAsync(identifier);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Background key rotation failed for {Identifier}", identifier);
                        }
                    });
                }

                // Retrieve encrypted key data from S3
                var keyData = await _s3Service.RetrieveKeyAsync(keyEntity.S3Reference);

                // Update last accessed timestamp and record usage
                keyEntity.LastAccessedAt = DateTime.UtcNow;
                keyEntity.Metadata["LastOperation"] = "Retrieve";
                await _repository.UpdateAsync(keyEntity);

                // Record usage in metadata
                if (!keyEntity.Metadata.ContainsKey("UsageCount"))
                {
                    keyEntity.Metadata["UsageCount"] = "0";
                }
                keyEntity.Metadata["UsageCount"] = (int.Parse(keyEntity.Metadata["UsageCount"]) + 1).ToString();

                _logger.LogInformation("Successfully retrieved key {Identifier}", identifier);
                return keyData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving key {Identifier}", identifier);
                throw;
            }
        }

        private async Task<KeyEntity> GetAndValidateKeyAsync(string identifier)
        {
            var keyEntity = await _repository.GetByIdAsync(identifier) 
                ?? throw new KeyNotFoundException($"Key {identifier} not found");

            if (!keyEntity.IsActive)
            {
                throw new InvalidOperationException($"Key {identifier} has been revoked");
            }

            if (keyEntity.IsExpired)
            {
                if (_automaticKeyRotationEnabled)
                {
                    _logger.LogWarning("Key {Identifier} has expired, attempting automatic rotation", identifier);
                    var newKeyId = await RotateKeyAsync(identifier);
                    return await _repository.GetByIdAsync(newKeyId);
                }
                throw new InvalidOperationException($"Key {identifier} has expired");
            }

            return keyEntity;
        }

        /// <inheritdoc />
        public async Task RevokeKeyAsync(string identifier)
        {
            try
            {
                // Get key metadata from SurrealDB
                var keyEntity = await _repository.GetByIdAsync(identifier);
                if (keyEntity == null)
                {
                    throw new KeyNotFoundException($"Key {identifier} not found");
                }

                // Mark as revoked
                keyEntity.RevokedAt = DateTime.UtcNow;
                keyEntity.Metadata["RevokedBy"] = "HybridKeyStorage";
                keyEntity.Metadata["RevocationReason"] = "Administrative revocation";

                // Update in SurrealDB
                await _repository.UpdateAsync(keyEntity);

                // Delete from S3
                await _s3Service.DeleteKeyAsync(keyEntity.S3Reference);

                _logger.LogInformation("Successfully revoked key {Identifier}", identifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking key {Identifier}", identifier);
                throw;
            }
        }

        /// <summary>
        /// Creates a new version of an existing key
        /// </summary>
        public async Task<string> RotateKeyAsync(string identifier, byte[] newKeyData = null)
        {
            try
            {
                // Get current key metadata
                var currentKey = await _repository.GetByIdAsync(identifier);
                if (currentKey == null)
                {
                    throw new KeyNotFoundException($"Key {identifier} not found");
                }

                // Create new key identifier
                var newIdentifier = $"{identifier}_v{currentKey.Version + 1}";

                // Generate or use provided new key data
                var keyDataToStore = newKeyData ?? await _s3Service.RetrieveKeyAsync(currentKey.S3Reference);

                // Store new key with updated expiration
                var newExpiration = DateTime.UtcNow.AddDays(_defaultKeyExpirationDays);
                await StoreKeyAsync(newIdentifier, currentKey.Address, keyDataToStore, currentKey.Algorithm, currentKey.Category, newExpiration);

                // Update new key with version info
                var newKey = await _repository.GetByIdAsync(newIdentifier) 
                    ?? throw new InvalidOperationException($"Failed to retrieve newly created key {newIdentifier}");
                newKey.Version = currentKey.Version + 1;
                newKey.PreviousVersionId = identifier;
                await _repository.UpdateAsync(newKey);

                // Mark old key as rotated
                currentKey.Metadata["RotatedTo"] = newIdentifier;
                currentKey.Metadata["RotatedAt"] = DateTime.UtcNow.ToString("O");
                await _repository.UpdateAsync(currentKey);

                _logger.LogInformation("Successfully rotated key {OldId} to {NewId}", identifier, newIdentifier);
                return newIdentifier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rotating key {Identifier}", identifier);
                throw;
            }
        }

        private bool IsKeyApproachingExpiration(KeyEntity key)
        {
            if (!key.ExpiresAt.HasValue) return false;

            // Consider key as approaching expiration if it's within 25% of its total lifetime
            var totalLifetime = key.ExpiresAt.Value - key.CreatedAt;
            var warningThreshold = key.ExpiresAt.Value - (totalLifetime * 0.25);
            
            return DateTime.UtcNow >= warningThreshold;
        }

        /// <summary>
        /// Updates the expiration date of a key
        /// </summary>
        public async Task UpdateExpirationAsync(string identifier, DateTime newExpirationDate)
        {
            try
            {
                var keyEntity = await GetAndValidateKeyAsync(identifier);
                
                keyEntity.ExpiresAt = newExpirationDate;
                keyEntity.Metadata["ExpirationUpdated"] = DateTime.UtcNow.ToString("O");
                
                await _repository.UpdateAsync(keyEntity);
                
                _logger.LogInformation("Updated expiration for key {Identifier} to {NewExpiration}", 
                    identifier, newExpirationDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expiration for key {Identifier}", identifier);
                throw;
            }
        }

        /// <summary>
        /// Lists all active keys matching the specified criteria
        /// </summary>
        public async Task<IEnumerable<KeyEntity>> ListActiveKeysAsync(string algorithm = null, KeyCategory? category = null)
        {
            // Note: We'll need to implement query functionality in SurrealRepository
            // For now, return empty list as this is not critical for key operations
            _logger.LogWarning("ListActiveKeysAsync not fully implemented - waiting for query functionality in SurrealRepository");
            return Array.Empty<KeyEntity>();

            // Once query functionality is implemented, it would look like:
            // var query = "SELECT * FROM keys WHERE active = true";
            // if (algorithm != null) query += " AND algorithm = $algorithm";
            // if (category.HasValue) query += " AND category = $category";
            // return await _repository.QueryAsync(query, new { algorithm, category });
        }

        private static string ComputeChecksum(byte[] data)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }
    }
}
