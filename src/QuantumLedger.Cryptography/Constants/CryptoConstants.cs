namespace QuantumLedger.Cryptography.Constants;

/// <summary>
/// Constants used throughout the cryptography implementation
/// </summary>
public static class CryptoConstants
{
    /// <summary>
    /// Algorithm names
    /// </summary>
    public static class Algorithms
    {
        /// <summary>
        /// ECDSA with P-256 curve and SHA-256
        /// </summary>
        public const string EC256 = "EC-256";

        /// <summary>
        /// Dilithium quantum-resistant signature algorithm
        /// </summary>
        public const string Dilithium = "DILITHIUM";

        /// <summary>
        /// Falcon quantum-resistant signature algorithm
        /// </summary>
        public const string Falcon = "FALCON";
    }

    /// <summary>
    /// Curve parameters
    /// </summary>
    public static class Curves
    {
        /// <summary>
        /// NIST P-256 curve name
        /// </summary>
        public const string P256 = "P-256";
    }

    /// <summary>
    /// Hash algorithms
    /// </summary>
    public static class HashAlgorithms
    {
        /// <summary>
        /// SHA-256 hash algorithm
        /// </summary>
        public const string SHA256 = "SHA-256";
    }

    /// <summary>
    /// Key operation names
    /// </summary>
    public static class KeyOperations
    {
        /// <summary>
        /// Key generation operation
        /// </summary>
        public const string Generate = "Generate";

        /// <summary>
        /// Key validation operation
        /// </summary>
        public const string Validate = "Validate";

        /// <summary>
        /// Key retrieval operation
        /// </summary>
        public const string Retrieve = "Retrieve";
    }

    /// <summary>
    /// Signature operations
    /// </summary>
    public static class SignatureOperations
    {
        /// <summary>
        /// Signature creation operation
        /// </summary>
        public const string Sign = "Sign";

        /// <summary>
        /// Signature verification operation
        /// </summary>
        public const string Verify = "Verify";
    }
}
