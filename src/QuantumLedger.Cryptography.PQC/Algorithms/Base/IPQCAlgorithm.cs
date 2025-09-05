namespace QuantumLedger.Cryptography.PQC.Algorithms.Base;

/// <summary>
/// Interface for post-quantum cryptography algorithms
/// </summary>
public interface IPQCAlgorithm
{
    /// <summary>
    /// Gets the name of the algorithm
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the security level of the algorithm
    /// </summary>
    int SecurityLevel { get; }

    /// <summary>
    /// Generates a new key pair
    /// </summary>
    /// <returns>A tuple containing the public and private keys</returns>
    (byte[] publicKey, byte[] privateKey) GenerateKeyPair();

    /// <summary>
    /// Signs a message using the private key
    /// </summary>
    /// <param name="message">The message to sign</param>
    /// <param name="privateKey">The private key to use for signing</param>
    /// <returns>The signature bytes</returns>
    byte[] Sign(byte[] message, byte[] privateKey);

    /// <summary>
    /// Verifies a signature using the public key
    /// </summary>
    /// <param name="message">The original message</param>
    /// <param name="signature">The signature to verify</param>
    /// <param name="publicKey">The public key to use for verification</param>
    /// <returns>True if the signature is valid, false otherwise</returns>
    bool Verify(byte[] message, byte[] signature, byte[] publicKey);

    /// <summary>
    /// Gets the size of the public key in bytes
    /// </summary>
    int PublicKeySize { get; }

    /// <summary>
    /// Gets the size of the private key in bytes
    /// </summary>
    int PrivateKeySize { get; }

    /// <summary>
    /// Gets the size of signatures in bytes
    /// </summary>
    int SignatureSize { get; }
}
