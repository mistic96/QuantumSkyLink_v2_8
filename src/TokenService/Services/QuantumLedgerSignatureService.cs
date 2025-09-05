using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TokenService.Services.Interfaces;

namespace TokenService.Services;

public class QuantumLedgerSignatureService : IQuantumLedgerSignatureService
{
    private readonly ILogger<QuantumLedgerSignatureService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<Guid, (string substitutionKeyId, string privateKeyPem)> _keyStorage;

    public QuantumLedgerSignatureService(ILogger<QuantumLedgerSignatureService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _keyStorage = new Dictionary<Guid, (string, string)>();
    }

    public async Task<(string signature, string keyId)> SignRequestAsync(object requestBody, string substitutionKeyId, string privateKeyPem)
    {
        _logger.LogInformation("Signing request with substitution key {KeyId}", substitutionKeyId);

        try
        {
            await Task.Delay(10); // Simulate cryptographic processing time

            // Serialize the request body to JSON
            var jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Create the signature using ECDSA
            var signature = await CreateEcdsaSignatureAsync(jsonBody, privateKeyPem);

            _logger.LogInformation("Request signed successfully with key {KeyId}", substitutionKeyId);
            return (signature, substitutionKeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing request with substitution key {KeyId}", substitutionKeyId);
            throw;
        }
    }

    public async Task<bool> VerifySignatureAsync(object requestBody, string signature, string publicKeyPem)
    {
        _logger.LogInformation("Verifying signature");

        try
        {
            await Task.Delay(10); // Simulate cryptographic processing time

            // Serialize the request body to JSON
            var jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Verify the signature using ECDSA
            var isValid = await VerifyEcdsaSignatureAsync(jsonBody, signature, publicKeyPem);

            _logger.LogInformation("Signature verification result: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying signature");
            return false;
        }
    }

    public async Task<(string privateKeyPem, string publicKeyPem)> GenerateKeyPairAsync()
    {
        _logger.LogInformation("Generating new ECDSA key pair");

        try
        {
            await Task.Delay(50); // Simulate key generation time

            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            
            var privateKeyPem = Convert.ToBase64String(ecdsa.ExportECPrivateKey());
            var publicKeyPem = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo());

            _logger.LogInformation("ECDSA key pair generated successfully");
            return (privateKeyPem, publicKeyPem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ECDSA key pair");
            throw;
        }
    }

    public async Task<bool> StoreSubstitutionKeyAsync(Guid tokenId, string substitutionKeyId, string privateKeyPem)
    {
        _logger.LogInformation("Storing substitution key for token {TokenId}", tokenId);

        try
        {
            await Task.Delay(25); // Simulate storage time

            // In production, this would encrypt the private key and store it securely
            // For now, we'll use in-memory storage (this is not production-ready)
            var encryptedKey = await EncryptPrivateKeyAsync(privateKeyPem);
            _keyStorage[tokenId] = (substitutionKeyId, encryptedKey);

            _logger.LogInformation("Substitution key stored successfully for token {TokenId}", tokenId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing substitution key for token {TokenId}", tokenId);
            return false;
        }
    }

    public async Task<(string substitutionKeyId, string privateKeyPem)?> GetSubstitutionKeyAsync(Guid tokenId)
    {
        _logger.LogInformation("Retrieving substitution key for token {TokenId}", tokenId);

        try
        {
            await Task.Delay(25); // Simulate retrieval time

            if (_keyStorage.TryGetValue(tokenId, out var keyData))
            {
                var decryptedKey = await DecryptPrivateKeyAsync(keyData.privateKeyPem);
                _logger.LogInformation("Substitution key retrieved successfully for token {TokenId}", tokenId);
                return (keyData.substitutionKeyId, decryptedKey);
            }

            _logger.LogWarning("No substitution key found for token {TokenId}", tokenId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving substitution key for token {TokenId}", tokenId);
            return null;
        }
    }

    public async Task<(string newSubstitutionKeyId, string newPrivateKeyPem)> RotateSubstitutionKeyAsync(Guid tokenId)
    {
        _logger.LogInformation("Rotating substitution key for token {TokenId}", tokenId);

        try
        {
            // Generate new key pair
            var (newPrivateKey, newPublicKey) = await GenerateKeyPairAsync();
            var newSubstitutionKeyId = Guid.NewGuid().ToString();

            // Store the new key
            await StoreSubstitutionKeyAsync(tokenId, newSubstitutionKeyId, newPrivateKey);

            _logger.LogInformation("Substitution key rotated successfully for token {TokenId}", tokenId);
            return (newSubstitutionKeyId, newPrivateKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating substitution key for token {TokenId}", tokenId);
            throw;
        }
    }

    public async Task<bool> RevokeSubstitutionKeyAsync(string substitutionKeyId)
    {
        _logger.LogInformation("Revoking substitution key {KeyId}", substitutionKeyId);

        try
        {
            await Task.Delay(25); // Simulate revocation time

            // Find and remove the key from storage
            var tokenToRemove = _keyStorage.FirstOrDefault(kvp => kvp.Value.substitutionKeyId == substitutionKeyId);
            if (!tokenToRemove.Equals(default(KeyValuePair<Guid, (string, string)>)))
            {
                _keyStorage.Remove(tokenToRemove.Key);
                _logger.LogInformation("Substitution key {KeyId} revoked successfully", substitutionKeyId);
                return true;
            }

            _logger.LogWarning("Substitution key {KeyId} not found for revocation", substitutionKeyId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking substitution key {KeyId}", substitutionKeyId);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> CreateSignedHeadersAsync(object requestBody, Guid tokenId)
    {
        _logger.LogInformation("Creating signed headers for token {TokenId}", tokenId);

        try
        {
            var keyInfo = await GetSubstitutionKeyAsync(tokenId);
            if (keyInfo == null)
            {
                throw new InvalidOperationException($"No substitution key found for token {tokenId}");
            }

            var (signature, keyId) = await SignRequestAsync(requestBody, keyInfo.Value.substitutionKeyId, keyInfo.Value.privateKeyPem);

            var headers = new Dictionary<string, string>
            {
                ["X-QuantumLedger-Signature"] = signature,
                ["X-QuantumLedger-KeyId"] = keyId,
                ["X-QuantumLedger-Timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ["X-QuantumLedger-Version"] = "1.0"
            };

            _logger.LogInformation("Signed headers created successfully for token {TokenId}", tokenId);
            return headers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating signed headers for token {TokenId}", tokenId);
            throw;
        }
    }

    public async Task<bool> IsSubstitutionKeyValidAsync(string substitutionKeyId)
    {
        _logger.LogInformation("Validating substitution key {KeyId}", substitutionKeyId);

        try
        {
            await Task.Delay(10); // Simulate validation time

            // Check if key exists in storage
            var keyExists = _keyStorage.Values.Any(v => v.substitutionKeyId == substitutionKeyId);
            
            if (!keyExists)
            {
                _logger.LogWarning("Substitution key {KeyId} not found", substitutionKeyId);
                return false;
            }

            // Check key expiration
            var expirationPeriod = _configuration.GetValue<TimeSpan>("Security:SubstitutionKeyExpiration", TimeSpan.FromDays(365));
            
            // In production, you would store the creation date and check against it
            // For now, we'll assume all keys are valid if they exist
            _logger.LogInformation("Substitution key {KeyId} is valid", substitutionKeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating substitution key {KeyId}", substitutionKeyId);
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<string> CreateEcdsaSignatureAsync(string data, string privateKeyPem)
    {
        await Task.Delay(5); // Simulate cryptographic processing

        try
        {
            var privateKeyBytes = Convert.FromBase64String(privateKeyPem);
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportECPrivateKey(privateKeyBytes, out _);

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = ecdsa.SignData(dataBytes, HashAlgorithmName.SHA256);

            return Convert.ToBase64String(signatureBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ECDSA signature");
            throw;
        }
    }

    private async Task<bool> VerifyEcdsaSignatureAsync(string data, string signature, string publicKeyPem)
    {
        await Task.Delay(5); // Simulate cryptographic processing

        try
        {
            var publicKeyBytes = Convert.FromBase64String(publicKeyPem);
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromBase64String(signature);

            return ecdsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying ECDSA signature");
            return false;
        }
    }

    private async Task<string> EncryptPrivateKeyAsync(string privateKeyPem)
    {
        await Task.Delay(5); // Simulate encryption processing

        // In production, this would use proper encryption with a secure key
        // For now, we'll use a simple Base64 encoding (NOT SECURE)
        var encryptionKey = _configuration.GetValue<string>("Security:EncryptionKey", "default-key");
        
        // Simple XOR encryption for demo purposes (NOT PRODUCTION READY)
        var keyBytes = Encoding.UTF8.GetBytes(privateKeyPem);
        var encryptionKeyBytes = Encoding.UTF8.GetBytes(encryptionKey);
        
        for (int i = 0; i < keyBytes.Length; i++)
        {
            keyBytes[i] ^= encryptionKeyBytes[i % encryptionKeyBytes.Length];
        }

        return Convert.ToBase64String(keyBytes);
    }

    private async Task<string> DecryptPrivateKeyAsync(string encryptedPrivateKeyPem)
    {
        await Task.Delay(5); // Simulate decryption processing

        // In production, this would use proper decryption
        // For now, we'll reverse the simple XOR encryption
        var encryptionKey = _configuration.GetValue<string>("Security:EncryptionKey", "default-key");
        
        var encryptedBytes = Convert.FromBase64String(encryptedPrivateKeyPem);
        var encryptionKeyBytes = Encoding.UTF8.GetBytes(encryptionKey);
        
        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            encryptedBytes[i] ^= encryptionKeyBytes[i % encryptionKeyBytes.Length];
        }

        return Encoding.UTF8.GetString(encryptedBytes);
    }

    #endregion
}
