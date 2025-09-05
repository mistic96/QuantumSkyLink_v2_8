using System.Security.Cryptography;

namespace TokenService.Services.Interfaces;

public interface IQuantumLedgerSignatureService
{
    /// <summary>
    /// Signs a request body with the substitution key for QuantumLedger authentication
    /// </summary>
    Task<(string signature, string keyId)> SignRequestAsync(object requestBody, string substitutionKeyId, string privateKeyPem);
    
    /// <summary>
    /// Verifies a signature against the request body
    /// </summary>
    Task<bool> VerifySignatureAsync(object requestBody, string signature, string publicKeyPem);
    
    /// <summary>
    /// Generates a new ECDSA key pair for substitution keys
    /// </summary>
    Task<(string privateKeyPem, string publicKeyPem)> GenerateKeyPairAsync();
    
    /// <summary>
    /// Stores substitution key securely (encrypted)
    /// </summary>
    Task<bool> StoreSubstitutionKeyAsync(Guid tokenId, string substitutionKeyId, string privateKeyPem);
    
    /// <summary>
    /// Retrieves substitution key for a token (decrypted)
    /// </summary>
    Task<(string substitutionKeyId, string privateKeyPem)?> GetSubstitutionKeyAsync(Guid tokenId);
    
    /// <summary>
    /// Rotates substitution key for a token
    /// </summary>
    Task<(string newSubstitutionKeyId, string newPrivateKeyPem)> RotateSubstitutionKeyAsync(Guid tokenId);
    
    /// <summary>
    /// Revokes a substitution key
    /// </summary>
    Task<bool> RevokeSubstitutionKeyAsync(string substitutionKeyId);
    
    /// <summary>
    /// Creates signed HTTP headers for QuantumLedger requests
    /// </summary>
    Task<Dictionary<string, string>> CreateSignedHeadersAsync(object requestBody, Guid tokenId);
    
    /// <summary>
    /// Validates substitution key expiration
    /// </summary>
    Task<bool> IsSubstitutionKeyValidAsync(string substitutionKeyId);
}
