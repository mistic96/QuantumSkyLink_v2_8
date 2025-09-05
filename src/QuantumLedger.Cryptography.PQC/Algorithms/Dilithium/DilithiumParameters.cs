namespace QuantumLedger.Cryptography.PQC.Algorithms.Dilithium;

/// <summary>
/// Parameters for the Dilithium post-quantum signature algorithm
/// </summary>
public class DilithiumParameters
{
    /// <summary>
    /// Gets the security level
    /// </summary>
    public int SecurityLevel { get; }

    /// <summary>
    /// Gets the K parameter
    /// </summary>
    public int K { get; }

    /// <summary>
    /// Gets the L parameter
    /// </summary>
    public int L { get; }

    private DilithiumParameters(int securityLevel, int k, int l)
    {
        SecurityLevel = securityLevel;
        K = k;
        L = l;
    }

    /// <summary>
    /// Gets the parameters for a given security level
    /// </summary>
    public static DilithiumParameters GetBySecurityLevel(int securityLevel) => securityLevel switch
    {
        2 => new DilithiumParameters(2, 4, 4),   // Dilithium2
        3 => new DilithiumParameters(3, 6, 5),   // Dilithium3
        5 => new DilithiumParameters(5, 8, 7),   // Dilithium5
        _ => throw new ArgumentException($"Invalid security level: {securityLevel}. Must be 2, 3, or 5.")
    };
}
