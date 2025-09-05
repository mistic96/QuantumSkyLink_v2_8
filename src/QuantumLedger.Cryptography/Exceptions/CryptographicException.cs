namespace QuantumLedger.Cryptography.Exceptions;

/// <summary>
/// Base exception for cryptographic operations in the Quantum Ledger system
/// </summary>
public class CryptographicException : Exception
{
    /// <summary>
    /// Gets the operation that failed
    /// </summary>
    public string Operation { get; }

    /// <summary>
    /// Gets the algorithm that was being used
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// Creates a new instance of CryptographicException
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="operation">The operation that failed</param>
    /// <param name="algorithm">The algorithm that was being used</param>
    /// <param name="innerException">The inner exception</param>
    public CryptographicException(string message, string operation, string algorithm, Exception? innerException = null)
        : base(message, innerException)
    {
        Operation = operation;
        Algorithm = algorithm;
    }

    /// <summary>
    /// Creates a new instance of CryptographicException
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="operation">The operation that failed</param>
    /// <param name="innerException">The inner exception</param>
    public CryptographicException(string message, string operation, Exception? innerException = null)
        : this(message, operation, "Unknown", innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a signature verification fails
/// </summary>
public class SignatureVerificationException : CryptographicException
{
    /// <summary>
    /// Creates a new instance of SignatureVerificationException
    /// </summary>
    /// <param name="algorithm">The algorithm that was being used</param>
    /// <param name="innerException">The inner exception</param>
    public SignatureVerificationException(string algorithm, Exception? innerException = null)
        : base("Signature verification failed", "Verify", algorithm, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a signing operation fails
/// </summary>
public class SigningException : CryptographicException
{
    /// <summary>
    /// Creates a new instance of SigningException
    /// </summary>
    /// <param name="algorithm">The algorithm that was being used</param>
    /// <param name="innerException">The inner exception</param>
    public SigningException(string algorithm, Exception? innerException = null)
        : base("Signing operation failed", "Sign", algorithm, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a key operation fails
/// </summary>
public class KeyOperationException : CryptographicException
{
    /// <summary>
    /// Creates a new instance of KeyOperationException
    /// </summary>
    /// <param name="operation">The key operation that failed</param>
    /// <param name="algorithm">The algorithm that was being used</param>
    /// <param name="innerException">The inner exception</param>
    public KeyOperationException(string operation, string algorithm, Exception? innerException = null)
        : base($"Key operation failed: {operation}", operation, algorithm, innerException)
    {
    }
}
