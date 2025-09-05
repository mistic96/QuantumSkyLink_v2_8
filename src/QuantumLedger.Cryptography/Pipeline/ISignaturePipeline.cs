using System.ComponentModel;
using QuantumLedger.Cryptography.Exceptions;
using QuantumLedger.Cryptography.Models;

namespace QuantumLedger.Cryptography.Pipeline;

/// <summary>
/// Represents a pipeline for signature verification and management
/// </summary>
public interface ISignaturePipeline
{
    /// <summary>
    /// Signs a message using both classical and quantum algorithms
    /// </summary>
    /// <param name="message">The message to sign</param>
    /// <param name="address">The address to use for signing</param>
    /// <returns>The signature result containing both signatures</returns>
    /// <exception cref="SigningException">Thrown when signing fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when message is null</exception>
    Task<SignatureResult> SignAsync(byte[] message, string address);

    /// <summary>
    /// Signs a message using both classical and quantum algorithms
    /// </summary>
    /// <param name="message">The message to sign</param>
    /// <returns>The signature result containing both signatures</returns>
    [Obsolete("Use SignAsync(byte[] message, string address) instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<SignatureResult> SignAsync(byte[] message) => throw new NotImplementedException("Use SignAsync(byte[] message, string address) instead");

    /// <summary>
    /// Verifies a dual signature (classical and quantum)
    /// </summary>
    /// <param name="message">The original message</param>
    /// <param name="signature">The signature to verify</param>
    /// <returns>The verification result</returns>
    /// <exception cref="SignatureVerificationException">Thrown when verification fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    Task<VerificationResult> VerifyAsync(byte[] message, DualSignature signature);

    /// <summary>
    /// Gets the current key status for both algorithms
    /// </summary>
    /// <param name="address">The address to get status for</param>
    /// <returns>The key status for both algorithms</returns>
    Task<KeyStatus> GetKeyStatusAsync(string address);

    /// <summary>
    /// Gets the current key status for both algorithms
    /// </summary>
    /// <returns>The key status for both algorithms</returns>
    [Obsolete("Use GetKeyStatusAsync(string address) instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<KeyStatus> GetKeyStatusAsync() => throw new NotImplementedException("Use GetKeyStatusAsync(string address) instead");

    /// <summary>
    /// Rotates keys for both algorithms
    /// </summary>
    /// <param name="address">The address to rotate keys for</param>
    /// <returns>The new key IDs for both algorithms</returns>
    /// <exception cref="KeyOperationException">Thrown when rotation fails</exception>
    Task<(string classicKeyId, string quantumKeyId)> RotateKeysAsync(string address);

    /// <summary>
    /// Rotates keys for both algorithms
    /// </summary>
    /// <returns>The new key IDs for both algorithms</returns>
    [Obsolete("Use RotateKeysAsync(string address) instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<(string classicKeyId, string quantumKeyId)> RotateKeysAsync() => throw new NotImplementedException("Use RotateKeysAsync(string address) instead");
}

/// <summary>
/// Represents a dual signature containing both classical and quantum signatures
/// </summary>
public class DualSignature
{
    /// <summary>
    /// Gets or sets the classical signature as a base64 string
    /// </summary>
    public string ClassicSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantum-resistant signature as a base64 string
    /// </summary>
    public string QuantumSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the classical key used
    /// </summary>
    public string ClassicKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the quantum key used
    /// </summary>
    public string QuantumKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this signature was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the classical signature as a byte array
    /// </summary>
    public byte[] GetClassicSignatureBytes()
    {
        if (string.IsNullOrWhiteSpace(ClassicSignature))
            return Array.Empty<byte>();
        
        try
        {
            return Convert.FromBase64String(ClassicSignature);
        }
        catch (FormatException)
        {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Gets the quantum signature as a byte array
    /// </summary>
    public byte[] GetQuantumSignatureBytes()
    {
        if (string.IsNullOrWhiteSpace(QuantumSignature))
            return Array.Empty<byte>();
        
        try
        {
            return Convert.FromBase64String(QuantumSignature);
        }
        catch (FormatException)
        {
            return Array.Empty<byte>();
        }
    }
}

/// <summary>
/// Represents the result of a signing operation
/// </summary>
public class SignatureResult
{
    /// <summary>
    /// Gets or sets whether the signing was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the dual signature if successful
    /// </summary>
    public DualSignature? Signature { get; set; }

    /// <summary>
    /// Gets or sets the error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the result of a verification operation
/// </summary>
public class VerificationResult
{
    /// <summary>
    /// Gets or sets whether the verification was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets whether the classical signature was valid
    /// </summary>
    public bool ClassicValid { get; set; }

    /// <summary>
    /// Gets or sets whether the quantum signature was valid
    /// </summary>
    public bool QuantumValid { get; set; }

    /// <summary>
    /// Gets or sets the error message if not successful
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the current status of cryptographic keys
/// </summary>
public class KeyStatus
{
    /// <summary>
    /// Gets or sets whether classical keys are available
    /// </summary>
    public bool ClassicKeysAvailable { get; set; }

    /// <summary>
    /// Gets or sets whether quantum keys are available
    /// </summary>
    public bool QuantumKeysAvailable { get; set; }

    /// <summary>
    /// Gets or sets the current classical key version
    /// </summary>
    public int? ClassicKeyVersion { get; set; }

    /// <summary>
    /// Gets or sets the current quantum key version
    /// </summary>
    public int? QuantumKeyVersion { get; set; }
}
