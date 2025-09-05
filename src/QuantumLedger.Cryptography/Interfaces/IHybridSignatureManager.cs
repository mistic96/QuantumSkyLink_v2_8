namespace QuantumLedger.Cryptography.Interfaces;

/// <summary>
/// Manages hybrid signatures combining both classical and quantum-resistant algorithms
/// </summary>
public interface IHybridSignatureManager
{
    /// <summary>
    /// Gets the classical signature algorithm name
    /// </summary>
    string ClassicAlgorithm { get; }

    /// <summary>
    /// Gets the quantum-resistant signature algorithm name
    /// </summary>
    string QuantumAlgorithm { get; }

    /// <summary>
    /// Signs a message using both classical and quantum-resistant algorithms
    /// </summary>
    /// <param name="message">The message to sign</param>
    /// <returns>A tuple containing both the classical and quantum signatures</returns>
    /// <exception cref="CryptographicException">Thrown when signing fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when message is null</exception>
    Task<(byte[] classicSig, byte[] quantumSig)> SignDualAsync(byte[] message);

    /// <summary>
    /// Verifies both classical and quantum signatures
    /// </summary>
    /// <param name="message">The original message</param>
    /// <param name="classicSig">The classical signature to verify</param>
    /// <param name="quantumSig">The quantum-resistant signature to verify</param>
    /// <returns>True if both signatures are valid, false otherwise</returns>
    /// <exception cref="CryptographicException">Thrown when verification fails due to an error</exception>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    Task<bool> VerifyDualAsync(byte[] message, byte[] classicSig, byte[] quantumSig);

    /// <summary>
    /// Gets the current key status for both algorithms
    /// </summary>
    /// <returns>A tuple indicating if keys are available for both algorithms</returns>
    Task<(bool classicKeyAvailable, bool quantumKeyAvailable)> GetKeyStatusAsync();
}
