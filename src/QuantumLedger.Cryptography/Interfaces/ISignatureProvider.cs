

using System.Security.Cryptography;

namespace QuantumLedger.Cryptography.Interfaces;

/// <summary>
/// Provides cryptographic signature operations for a specific algorithm
/// </summary>
public interface ISignatureProvider
{
    /// <summary>
    /// Gets the name of the signature algorithm
    /// </summary>
    string Algorithm { get; }

    /// <summary>
    /// Signs a message using the specified private key
    /// </summary>
    /// <param name="message">The message to sign</param>
    /// <param name="privateKey">The private key to use for signing</param>
    /// <returns>The signature bytes</returns>
    /// <exception cref="CryptographicException">Thrown when signing fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when message or privateKey is null</exception>
    ValueTask<byte[]> SignAsync(byte[] message, byte[] privateKey);

    /// <summary>
    /// Verifies a signature using the specified public key
    /// </summary>
    /// <param name="message">The original message</param>
    /// <param name="signature">The signature to verify</param>
    /// <param name="publicKey">The public key to use for verification</param>
    /// <returns>True if the signature is valid, false otherwise</returns>
    /// <exception cref="CryptographicException">Thrown when verification fails due to an error</exception>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    ValueTask<bool> VerifyAsync(byte[] message, byte[] signature, byte[] publicKey);
}
