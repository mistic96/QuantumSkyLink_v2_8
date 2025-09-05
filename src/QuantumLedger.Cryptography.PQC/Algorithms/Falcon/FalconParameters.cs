namespace QuantumLedger.Cryptography.PQC.Algorithms.Falcon;

/// <summary>
/// Parameters for the Falcon post-quantum signature algorithm
/// </summary>
public class FalconParameters
{
    /// <summary>
    /// Degree of polynomials (n = 2^LogN)
    /// </summary>
    public int N { get; }

    /// <summary>
    /// Log2 of degree (height of the tree)
    /// </summary>
    public int LogN { get; }

    /// <summary>
    /// Signature compression level in bits
    /// </summary>
    public int SignatureBits { get; }

    /// <summary>
    /// Size of the nonce in bytes
    /// </summary>
    public int NonceSize { get; }

    /// <summary>
    /// Security level based on tree height
    /// </summary>
    public int SecurityLevel { get; }

    private FalconParameters(int n, int logN, int signatureBits, int nonceSize, int securityLevel)
    {
        N = n;
        LogN = logN;
        SignatureBits = signatureBits;
        NonceSize = nonceSize;
        SecurityLevel = securityLevel;
    }

    /// <summary>
    /// Height 8 parameters (~123-bit classical security)
    /// </summary>
    public static readonly FalconParameters Height8 = new(
        n: 256,          // 2^8
        logN: 8,
        signatureBits: 8,
        nonceSize: 40,
        securityLevel: 123
    );

    /// <summary>
    /// Height 9 parameters (~147-bit classical security)
    /// </summary>
    public static readonly FalconParameters Height9 = new(
        n: 512,          // 2^9
        logN: 9,
        signatureBits: 9,
        nonceSize: 40,
        securityLevel: 147
    );

    /// <summary>
    /// Height 10 parameters (~172-bit classical security)
    /// </summary>
    public static readonly FalconParameters Height10 = new(
        n: 1024,         // 2^10
        logN: 10,
        signatureBits: 10,
        nonceSize: 40,
        securityLevel: 172
    );

    /// <summary>
    /// Gets parameters for a specific tree height
    /// </summary>
    public static FalconParameters GetByHeight(int height) => height switch
    {
        8 => Height8,
        9 => Height9,
        10 => Height10,
        _ => throw new ArgumentException($"Invalid tree height: {height}. Must be 8, 9, or 10.")
    };

    /// <summary>
    /// Gets the size of public keys in bytes for this parameter set
    /// </summary>
    public int PublicKeySize => (N * 12) / 8;

    /// <summary>
    /// Gets the size of private keys in bytes for this parameter set
    /// </summary>
    public int PrivateKeySize => N * 2;

    /// <summary>
    /// Gets the maximum size of signatures in bytes for this parameter set
    /// </summary>
    public int MaxSignatureSize => (N * SignatureBits) / 8 + NonceSize + 2;

    /// <summary>
    /// Gets the modulus used in Falcon (q = 12289)
    /// </summary>
    public const int Q = 12289;
}
