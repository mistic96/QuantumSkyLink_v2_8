using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Constants;
using QuantumLedger.Cryptography.Exceptions;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Models;
using QuantumLedger.Cryptography.Utils;
using QuantumLedger.Data;
using QuantumLedger.Models.Account;
using System.Security.Cryptography;
using System.Text;

namespace QuantumLedger.Cryptography.Services;

/// <summary>
/// Production service for managing substitution keys (user-controlled delegation keys)
/// Integrates with AccountsContext database and multi-cloud key vault infrastructure
/// </summary>
public class SubstitutionKeyService : ISubstitutionKeyService
{
    private readonly AccountsContext _accountsContext;
    private readonly ICloudKeyVaultFactory _cloudKeyVaultFactory;
    private readonly ISignatureProvider _signatureProvider;
    private readonly ILogger<SubstitutionKeyService> _logger;

    public SubstitutionKeyService(
        AccountsContext accountsContext,
        ICloudKeyVaultFactory cloudKeyVaultFactory,
        ISignatureProvider signatureProvider,
        ILogger<SubstitutionKeyService> logger)
    {
        _accountsContext = accountsContext ?? throw new ArgumentNullException(nameof(accountsContext));
        _cloudKeyVaultFactory = cloudKeyVaultFactory ?? throw new ArgumentNullException(nameof(cloudKeyVaultFactory));
        _signatureProvider = signatureProvider ?? throw new ArgumentNullException(nameof(signatureProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<SubstitutionKeyPair> GenerateSubstitutionKeyAsync(string address, DateTime? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            // Generate EC-256 key pair for substitution key (simple and efficient)
            var (privateKey, publicKey) = KeyGenerationHelper.GenerateEC256KeyPair();

            // Get next version number from database by checking storage paths
            var existingKeys = await _accountsContext.AccountKeys
                .Include(k => k.Account)
                .Where(k => k.Account.InternalReferenceId == address && k.Algorithm == CryptoConstants.Algorithms.EC256)
                .Where(k => k.StoragePath.Contains("_substitution_"))
                .ToListAsync();

            var nextVersion = existingKeys.Any() ? 
                existingKeys.Max(k => ExtractVersionFromStoragePath(k.StoragePath)) + 1 : 1;

            // Create substitution key ID (used as storage path identifier)
            var substitutionKeyId = $"{address}_substitution_v{nextVersion}";

            // Set default expiration (1 year)
            var expiration = expiresAt ?? DateTime.UtcNow.AddYears(1);

            // Find or create account
            var account = await _accountsContext.Accounts
                .FirstOrDefaultAsync(a => a.InternalReferenceId == address);

            if (account == null)
            {
                account = new Account
                {
                    AccountId = Guid.NewGuid(),
                    ExternalOwnerId = address,
                    OwnerIdType = "Address",
                    VendorSystem = "QuantumLedger",
                    InternalReferenceId = address,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };
                _accountsContext.Accounts.Add(account);
                await _accountsContext.SaveChangesAsync();
            }

            // Get optimal cloud provider for cost efficiency
            var keyVault = _cloudKeyVaultFactory.GetOptimalProvider();
            var cloudProvider = "GoogleCloud"; // Default to optimal provider
            
            // Encrypt private key using cloud vault for secure storage
            var encryptedPrivateKey = await keyVault.EncryptAsync(privateKey, substitutionKeyId, CryptoConstants.Algorithms.EC256);
            var storagePath = $"substitution_keys/{substitutionKeyId}";

            // Store account key in database
            var accountKey = new AccountKey
            {
                KeyId = Guid.NewGuid(), // Generate new GUID for database
                AccountId = account.AccountId,
                Algorithm = CryptoConstants.Algorithms.EC256,
                PublicKey = Convert.ToBase64String(publicKey),
                CloudProvider = cloudProvider,
                StoragePath = storagePath, // Use structured storage path
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiration
            };

            _accountsContext.AccountKeys.Add(accountKey);

            // Store public key in registry for fast verification
            var publicKeyHash = Convert.ToHexString(SHA256.HashData(publicKey));
            var registryEntry = new PublicKeyRegistryEntry
            {
                PublicKeyHash = publicKeyHash,
                AccountId = account.AccountId,
                Algorithm = CryptoConstants.Algorithms.EC256,
                PublicKey = Convert.ToBase64String(publicKey),
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UsageCount = 0
            };

            _accountsContext.PublicKeyRegistry.Add(registryEntry);
            await _accountsContext.SaveChangesAsync();

            // Create substitution key pair result (give private key to user)
            var substitutionKeyPair = new SubstitutionKeyPair
            {
                SubstitutionKeyId = substitutionKeyId,
                PrivateKey = privateKey, // User gets this for complete control
                PublicKey = publicKey,
                LinkedAddress = address,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiration,
                IsActive = true
            };

            _logger.LogInformation(
                "Generated substitution key {SubstitutionKeyId} for address {Address} using {CloudProvider} (expires: {ExpiresAt})",
                substitutionKeyId, address, cloudProvider, expiration);

            return substitutionKeyPair;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to generate substitution key for address {Address}", address);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Generate,
                CryptoConstants.Algorithms.EC256,
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyRequestSignatureAsync(byte[] requestData, string signature, string substitutionKeyId)
    {
        ArgumentNullException.ThrowIfNull(requestData);
        ArgumentException.ThrowIfNullOrEmpty(signature);
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);

        try
        {
            // Get the public key for the substitution key
            var publicKey = await GetSubstitutionPublicKeyAsync(substitutionKeyId);
            if (publicKey == null)
            {
                _logger.LogWarning("Substitution key {SubstitutionKeyId} not found", substitutionKeyId);
                return false;
            }

            // Check if key is active
            if (!await IsSubstitutionKeyActiveAsync(substitutionKeyId))
            {
                _logger.LogWarning("Substitution key {SubstitutionKeyId} is not active", substitutionKeyId);
                return false;
            }

            // Verify the signature
            var signatureBytes = Convert.FromBase64String(signature);
            var isValid = await _signatureProvider.VerifyAsync(requestData, signatureBytes, publicKey);

            _logger.LogDebug(
                "Substitution key signature verification for {SubstitutionKeyId}: {IsValid}",
                substitutionKeyId, isValid);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error verifying substitution key signature for {SubstitutionKeyId}", 
                substitutionKeyId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> VerifyAuthorizationAsync(string substitutionKeyId, string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var linkedAddress = await GetLinkedAddressAsync(substitutionKeyId);
            var isAuthorized = linkedAddress == address;

            _logger.LogDebug(
                "Authorization check for substitution key {SubstitutionKeyId} and address {Address}: {IsAuthorized}",
                substitutionKeyId, address, isAuthorized);

            return isAuthorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error verifying authorization for substitution key {SubstitutionKeyId} and address {Address}",
                substitutionKeyId, address);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetLinkedAddressAsync(string substitutionKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);

        try
        {
            // Get from database using substitution key ID pattern in storage path
            var accountKey = await _accountsContext.AccountKeys
                .Include(k => k.Account)
                .FirstOrDefaultAsync(k => k.StoragePath.Contains(substitutionKeyId));

            if (accountKey?.Account != null)
            {
                return accountKey.Account.InternalReferenceId;
            }

            _logger.LogWarning("Could not determine linked address for substitution key {SubstitutionKeyId}", substitutionKeyId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting linked address for substitution key {SubstitutionKeyId}", substitutionKeyId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RevokeSubstitutionKeyAsync(string substitutionKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);

        try
        {
            // Update key status in database using substitution key ID pattern
            var accountKey = await _accountsContext.AccountKeys
                .FirstOrDefaultAsync(k => k.StoragePath.Contains(substitutionKeyId));

            if (accountKey == null)
            {
                _logger.LogWarning("Substitution key {SubstitutionKeyId} not found for revocation", substitutionKeyId);
                return false;
            }

            accountKey.Status = "Revoked";
            await _accountsContext.SaveChangesAsync();

            _logger.LogInformation("Revoked substitution key {SubstitutionKeyId}", substitutionKeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke substitution key {SubstitutionKeyId}", substitutionKeyId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<SubstitutionKeyPair> RotateSubstitutionKeyAsync(string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            // Get current substitution key
            var currentKey = await GetCurrentSubstitutionKeyAsync(address);
            
            // Generate new substitution key
            var newKey = await GenerateSubstitutionKeyAsync(address);

            // Revoke old key if it exists
            if (currentKey != null)
            {
                await RevokeSubstitutionKeyAsync(currentKey.SubstitutionKeyId);
                _logger.LogInformation(
                    "Rotated substitution key for address {Address}: {OldKeyId} -> {NewKeyId}",
                    address, currentKey.SubstitutionKeyId, newKey.SubstitutionKeyId);
            }
            else
            {
                _logger.LogInformation(
                    "Created initial substitution key for address {Address}: {NewKeyId}",
                    address, newKey.SubstitutionKeyId);
            }

            return newKey;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, "Failed to rotate substitution key for address {Address}", address);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Generate,
                CryptoConstants.Algorithms.EC256,
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SubstitutionKeyPair>> GetSubstitutionKeysAsync(string address, SubstitutionKeyCriteria? criteria = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            // Get all substitution keys for the address from database
            var query = _accountsContext.AccountKeys
                .Include(k => k.Account)
                .Where(k => k.Account.InternalReferenceId == address && k.StoragePath.Contains("_substitution_"));

            // Apply criteria filtering
            if (criteria != null)
            {
                if (!criteria.IncludeExpired)
                {
                    query = query.Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow);
                }

                if (!criteria.IncludeRevoked)
                {
                    query = query.Where(k => k.Status == "Active");
                }

                if (criteria.MaxAge.HasValue)
                {
                    var cutoffDate = DateTime.UtcNow - criteria.MaxAge.Value;
                    query = query.Where(k => k.CreatedAt >= cutoffDate);
                }

                if (criteria.MinVersion.HasValue)
                {
                    // This would require parsing the version from StoragePath
                    var keys = await query.ToListAsync();
                    keys = keys.Where(k => ExtractVersionFromStoragePath(k.StoragePath) >= criteria.MinVersion.Value).ToList();
                    return ConvertToSubstitutionKeyPairs(keys);
                }
            }

            var accountKeys = await query.ToListAsync();
            return ConvertToSubstitutionKeyPairs(accountKeys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get substitution keys for address {Address}", address);
            return Array.Empty<SubstitutionKeyPair>();
        }
    }

    /// <inheritdoc/>
    public async Task<SubstitutionKeyPair?> GetCurrentSubstitutionKeyAsync(string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var keys = await GetSubstitutionKeysAsync(address, new SubstitutionKeyCriteria
            {
                IncludeExpired = false,
                IncludeRevoked = false
            });

            // Return the latest version
            return keys.OrderByDescending(k => ExtractVersionFromStoragePath(k.SubstitutionKeyId)).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current substitution key for address {Address}", address);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<SubstitutionKeyVerificationResult> VerifySubstitutionKeyRequestAsync(
        byte[] requestData, 
        string signature, 
        string substitutionKeyId, 
        string? expectedAddress = null)
    {
        try
        {
            var result = new SubstitutionKeyVerificationResult();

            // Verify signature
            result.SignatureValid = await VerifyRequestSignatureAsync(requestData, signature, substitutionKeyId);
            
            // Get linked address
            result.AuthenticatedAddress = await GetLinkedAddressAsync(substitutionKeyId);

            // Verify authorization if expected address is provided
            if (!string.IsNullOrEmpty(expectedAddress))
            {
                result.AuthorizedForAddress = await VerifyAuthorizationAsync(substitutionKeyId, expectedAddress);
            }
            else
            {
                result.AuthorizedForAddress = result.AuthenticatedAddress != null;
            }

            // Overall success
            result.Success = result.SignatureValid && result.AuthorizedForAddress;

            // Add context information
            result.Context["SubstitutionKeyId"] = substitutionKeyId;
            result.Context["VerificationTime"] = DateTime.UtcNow;
            result.Context["ExpectedAddress"] = expectedAddress ?? "Not specified";

            if (!result.Success)
            {
                var errors = new List<string>();
                if (!result.SignatureValid) errors.Add("Invalid signature");
                if (!result.AuthorizedForAddress) errors.Add("Not authorized for address");
                result.ErrorMessage = string.Join(", ", errors);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during substitution key verification for {SubstitutionKeyId}", substitutionKeyId);
            return new SubstitutionKeyVerificationResult
            {
                Success = false,
                SignatureValid = false,
                AuthorizedForAddress = false,
                ErrorMessage = "Internal verification error",
                Context = { ["Error"] = ex.Message }
            };
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateExpirationAsync(string substitutionKeyId, DateTime newExpirationDate)
    {
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);

        try
        {
            var accountKey = await _accountsContext.AccountKeys
                .FirstOrDefaultAsync(k => k.StoragePath.Contains(substitutionKeyId));

            if (accountKey == null)
            {
                _logger.LogWarning("Substitution key {SubstitutionKeyId} not found for expiration update", substitutionKeyId);
                return false;
            }

            accountKey.ExpiresAt = newExpirationDate;
            await _accountsContext.SaveChangesAsync();

            _logger.LogInformation(
                "Updated expiration for substitution key {SubstitutionKeyId} to {NewExpiration}",
                substitutionKeyId, newExpirationDate);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update expiration for substitution key {SubstitutionKeyId}", substitutionKeyId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]?> GetSubstitutionPublicKeyAsync(string substitutionKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);

        try
        {
            // Get from AccountKeys table using substitution key ID pattern
            var accountKey = await _accountsContext.AccountKeys
                .FirstOrDefaultAsync(k => k.StoragePath.Contains(substitutionKeyId));

            if (accountKey != null)
            {
                return Convert.FromBase64String(accountKey.PublicKey);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get public key for substitution key {SubstitutionKeyId}", substitutionKeyId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsSubstitutionKeyActiveAsync(string substitutionKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);

        try
        {
            var accountKey = await _accountsContext.AccountKeys
                .FirstOrDefaultAsync(k => k.StoragePath.Contains(substitutionKeyId));
            
            return accountKey != null && 
                   accountKey.Status == "Active" && 
                   (accountKey.ExpiresAt == null || accountKey.ExpiresAt > DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if substitution key {SubstitutionKeyId} is active", substitutionKeyId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, object>> GetSubstitutionKeyStatsAsync(string substitutionKeyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(substitutionKeyId);

        try
        {
            var stats = new Dictionary<string, object>();
            
            // Get key information from database
            var accountKey = await _accountsContext.AccountKeys
                .Include(k => k.Account)
                .FirstOrDefaultAsync(k => k.StoragePath.Contains(substitutionKeyId));

            if (accountKey != null)
            {
                stats["KeyId"] = substitutionKeyId;
                stats["Address"] = accountKey.Account?.InternalReferenceId ?? "Unknown";
                stats["CreatedAt"] = accountKey.CreatedAt;
                stats["ExpiresAt"] = accountKey.ExpiresAt;
                stats["IsActive"] = accountKey.Status == "Active";
                stats["IsExpired"] = accountKey.ExpiresAt.HasValue && accountKey.ExpiresAt < DateTime.UtcNow;
                stats["Version"] = ExtractVersionFromStoragePath(accountKey.StoragePath);
                stats["Algorithm"] = accountKey.Algorithm;
                stats["CloudProvider"] = accountKey.CloudProvider;
                
                // Get usage statistics from public key registry
                var registryEntry = await _accountsContext.PublicKeyRegistry
                    .FirstOrDefaultAsync(r => r.AccountId == accountKey.AccountId && r.Algorithm == accountKey.Algorithm);
                
                if (registryEntry != null)
                {
                    stats["UsageCount"] = registryEntry.UsageCount;
                    stats["LastUsed"] = registryEntry.LastUsed;
                }
            }
            else
            {
                stats["Error"] = "Substitution key not found";
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats for substitution key {SubstitutionKeyId}", substitutionKeyId);
            return new Dictionary<string, object> { ["Error"] = ex.Message };
        }
    }

    /// <summary>
    /// Converts AccountKey entities to SubstitutionKeyPair objects
    /// </summary>
    private static IEnumerable<SubstitutionKeyPair> ConvertToSubstitutionKeyPairs(IEnumerable<AccountKey> accountKeys)
    {
        return accountKeys.Select(key => new SubstitutionKeyPair
        {
            SubstitutionKeyId = ExtractSubstitutionKeyIdFromStoragePath(key.StoragePath), // Extract substitution key ID from storage path
            PrivateKey = Array.Empty<byte>(), // Don't return private key in list operations
            PublicKey = Convert.FromBase64String(key.PublicKey),
            LinkedAddress = key.Account?.InternalReferenceId ?? ExtractAddressFromStoragePath(key.StoragePath),
            CreatedAt = key.CreatedAt,
            ExpiresAt = key.ExpiresAt,
            IsActive = key.Status == "Active"
        });
    }

    /// <summary>
    /// Extracts the version number from a substitution key storage path
    /// </summary>
    private static int ExtractVersionFromStoragePath(string storagePath)
    {
        // Extract from patterns like "substitution_keys/address_substitution_v1"
        var parts = storagePath.Split('/');
        if (parts.Length > 1)
        {
            var keyParts = parts[1].Split('_');
            if (keyParts.Length >= 3 && keyParts[2].StartsWith("v"))
            {
                if (int.TryParse(keyParts[2][1..], out var version))
                {
                    return version;
                }
            }
        }
        return 1; // Default version
    }

    /// <summary>
    /// Extracts the address from a substitution key storage path
    /// </summary>
    private static string ExtractAddressFromStoragePath(string storagePath)
    {
        // Extract from patterns like "substitution_keys/address_substitution_v1"
        var parts = storagePath.Split('/');
        if (parts.Length > 1)
        {
            var keyParts = parts[1].Split('_');
            return keyParts.Length > 0 ? keyParts[0] : storagePath;
        }
        return storagePath;
    }

    /// <summary>
    /// Extracts the substitution key ID from a storage path
    /// </summary>
    private static string ExtractSubstitutionKeyIdFromStoragePath(string storagePath)
    {
        // Extract from patterns like "substitution_keys/address_substitution_v1"
        var parts = storagePath.Split('/');
        return parts.Length > 1 ? parts[1] : storagePath;
    }
}
