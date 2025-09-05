using QuantumLedger.Cryptography.Models;

namespace QuantumLedger.Cryptography.Interfaces;

/// <summary>
/// Interface for managing substitution keys (user-controlled delegation keys)
/// </summary>
public interface ISubstitutionKeyService
{
    /// <summary>
    /// Generates a new substitution key pair for an address
    /// </summary>
    /// <param name="address">The address to generate substitution key for</param>
    /// <param name="expiresAt">Optional expiration date (defaults to 1 year)</param>
    /// <returns>The generated substitution key pair</returns>
    Task<SubstitutionKeyPair> GenerateSubstitutionKeyAsync(string address, DateTime? expiresAt = null);

    /// <summary>
    /// Verifies a request signature using a substitution key
    /// </summary>
    /// <param name="requestData">The request data that was signed</param>
    /// <param name="signature">The signature to verify (base64 encoded)</param>
    /// <param name="substitutionKeyId">The substitution key ID used for signing</param>
    /// <returns>True if the signature is valid</returns>
    Task<bool> VerifyRequestSignatureAsync(byte[] requestData, string signature, string substitutionKeyId);

    /// <summary>
    /// Verifies that a substitution key is authorized for a specific address
    /// </summary>
    /// <param name="substitutionKeyId">The substitution key ID to verify</param>
    /// <param name="address">The address to check authorization for</param>
    /// <returns>True if the substitution key is authorized for the address</returns>
    Task<bool> VerifyAuthorizationAsync(string substitutionKeyId, string address);

    /// <summary>
    /// Gets the linked main account address for a substitution key
    /// </summary>
    /// <param name="substitutionKeyId">The substitution key ID</param>
    /// <returns>The linked address, or null if not found</returns>
    Task<string?> GetLinkedAddressAsync(string substitutionKeyId);

    /// <summary>
    /// Revokes a substitution key
    /// </summary>
    /// <param name="substitutionKeyId">The substitution key ID to revoke</param>
    /// <returns>True if the key was successfully revoked</returns>
    Task<bool> RevokeSubstitutionKeyAsync(string substitutionKeyId);

    /// <summary>
    /// Rotates (creates a new version of) a substitution key for an address
    /// </summary>
    /// <param name="address">The address to rotate substitution key for</param>
    /// <returns>The new substitution key pair</returns>
    Task<SubstitutionKeyPair> RotateSubstitutionKeyAsync(string address);

    /// <summary>
    /// Gets all active substitution keys for an address
    /// </summary>
    /// <param name="address">The address to get substitution keys for</param>
    /// <param name="criteria">Optional criteria for filtering keys</param>
    /// <returns>Collection of substitution key pairs</returns>
    Task<IEnumerable<SubstitutionKeyPair>> GetSubstitutionKeysAsync(string address, SubstitutionKeyCriteria? criteria = null);

    /// <summary>
    /// Gets the current active substitution key for an address
    /// </summary>
    /// <param name="address">The address to get the current substitution key for</param>
    /// <returns>The current substitution key pair, or null if none exists</returns>
    Task<SubstitutionKeyPair?> GetCurrentSubstitutionKeyAsync(string address);

    /// <summary>
    /// Verifies a complete substitution key request with detailed results
    /// </summary>
    /// <param name="requestData">The request data that was signed</param>
    /// <param name="signature">The signature to verify (base64 encoded)</param>
    /// <param name="substitutionKeyId">The substitution key ID used for signing</param>
    /// <param name="expectedAddress">The expected address for authorization</param>
    /// <returns>Detailed verification result</returns>
    Task<SubstitutionKeyVerificationResult> VerifySubstitutionKeyRequestAsync(
        byte[] requestData, 
        string signature, 
        string substitutionKeyId, 
        string? expectedAddress = null);

    /// <summary>
    /// Updates the expiration date of a substitution key
    /// </summary>
    /// <param name="substitutionKeyId">The substitution key ID to update</param>
    /// <param name="newExpirationDate">The new expiration date</param>
    /// <returns>True if the expiration was successfully updated</returns>
    Task<bool> UpdateExpirationAsync(string substitutionKeyId, DateTime newExpirationDate);

    /// <summary>
    /// Gets the public key for a substitution key (for verification purposes)
    /// </summary>
    /// <param name="substitutionKeyId">The substitution key ID</param>
    /// <returns>The public key bytes, or null if not found</returns>
    Task<byte[]?> GetSubstitutionPublicKeyAsync(string substitutionKeyId);

    /// <summary>
    /// Checks if a substitution key exists and is active
    /// </summary>
    /// <param name="substitutionKeyId">The substitution key ID to check</param>
    /// <returns>True if the key exists and is active</returns>
    Task<bool> IsSubstitutionKeyActiveAsync(string substitutionKeyId);

    /// <summary>
    /// Gets usage statistics for a substitution key
    /// </summary>
    /// <param name="substitutionKeyId">The substitution key ID</param>
    /// <returns>Dictionary containing usage statistics</returns>
    Task<Dictionary<string, object>> GetSubstitutionKeyStatsAsync(string substitutionKeyId);
}
