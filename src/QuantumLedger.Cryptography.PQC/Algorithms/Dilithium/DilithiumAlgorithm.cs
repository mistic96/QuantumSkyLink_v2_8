extern alias BC;
using BC::Org.BouncyCastle.Asn1;
using BC::Org.BouncyCastle.Crypto;
using BC::Org.BouncyCastle.Crypto.Parameters;
using BC::Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium;
using BC::Org.BouncyCastle.Security;
using QuantumLedger.Cryptography.PQC.Algorithms.Base;

namespace QuantumLedger.Cryptography.PQC.Algorithms.Dilithium;

/// <summary>
/// Implementation of the Dilithium post-quantum signature algorithm using BouncyCastle
/// </summary>
public class DilithiumAlgorithm : IPQCAlgorithm
{
    private readonly DilithiumParameters _params;
    private readonly SecureRandom _random;
    private readonly DilithiumKeyGenerationParameters _keyGenParams;
    private readonly BC::Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters _bcParams;

    /// <summary>
    /// Gets the name of the algorithm
    /// </summary>
    public string Name => "DILITHIUM";

    /// <summary>
    /// Gets the security level of this instance
    /// </summary>
    public int SecurityLevel => _params.SecurityLevel;

    /// <summary>
    /// Gets the size of public keys in bytes
    /// </summary>
    public int PublicKeySize => 32 * (_params.K + _params.L);

    /// <summary>
    /// Gets the size of private keys in bytes
    /// </summary>
    public int PrivateKeySize => 32 * (2 * _params.L + _params.K);

    /// <summary>
    /// Gets the size of signatures in bytes
    /// </summary>
    public int SignatureSize => 32 + ((_params.L + _params.K) * 32);

    /// <summary>
    /// Creates a new instance of DilithiumAlgorithm
    /// </summary>
    /// <param name="securityLevel">NIST security level (2, 3, or 5)</param>
    public DilithiumAlgorithm(int securityLevel = 3)
    {
        _params = DilithiumParameters.GetBySecurityLevel(securityLevel);
        _random = new SecureRandom();

        // Get BouncyCastle parameters based on security level
        _bcParams = securityLevel switch
        {
            2 => BC::Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium2,
            3 => BC::Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium3,
            5 => BC::Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium.DilithiumParameters.Dilithium5,
            _ => throw new ArgumentException($"Invalid security level: {securityLevel}. Must be 2, 3, or 5.")
        };

        _keyGenParams = new DilithiumKeyGenerationParameters(_random, _bcParams);
    }

    /// <inheritdoc/>
    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        var keyGen = new DilithiumKeyPairGenerator();
        keyGen.Init(_keyGenParams);
        var keyPair = keyGen.GenerateKeyPair();

        var privateKey = ((DilithiumPrivateKeyParameters)keyPair.Private).GetEncoded();
        var publicKey = ((DilithiumPublicKeyParameters)keyPair.Public).GetEncoded();

        return (publicKey, privateKey);
    }

    /// <inheritdoc/>
    public byte[] Sign(byte[] message, byte[] privateKey)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(privateKey);

        if (privateKey.Length != PrivateKeySize)
        {
            throw new ArgumentException($"Invalid private key size. Expected {PrivateKeySize} bytes, got {privateKey.Length}");
        }

        var signer = new DilithiumSigner();
        var s1 = new byte[_params.L * 32];
        var s2 = new byte[_params.K * 32];
        var t0 = new byte[_params.K * 32];
        var tr = new byte[32];
        var rho = new byte[32];
        var key = new byte[32];
        var t1 = new byte[_params.K * 32];

        // Extract key components from encoded private key
        Buffer.BlockCopy(privateKey, 0, s1, 0, s1.Length);
        Buffer.BlockCopy(privateKey, s1.Length, s2, 0, s2.Length);
        Buffer.BlockCopy(privateKey, s1.Length + s2.Length, t0, 0, t0.Length);
        Buffer.BlockCopy(privateKey, s1.Length + s2.Length + t0.Length, tr, 0, tr.Length);
        Buffer.BlockCopy(privateKey, s1.Length + s2.Length + t0.Length + tr.Length, rho, 0, rho.Length);
        Buffer.BlockCopy(privateKey, s1.Length + s2.Length + t0.Length + tr.Length + rho.Length, key, 0, key.Length);
        Buffer.BlockCopy(privateKey, s1.Length + s2.Length + t0.Length + tr.Length + rho.Length + key.Length, t1, 0, t1.Length);

        var privateKeyParams = new DilithiumPrivateKeyParameters(_bcParams, rho, key, tr, s1, s2, t0, t1);
        
        signer.Init(true, new ParametersWithRandom(privateKeyParams, _random));
        return signer.GenerateSignature(message);
    }

    /// <inheritdoc/>
    public bool Verify(byte[] message, byte[] signature, byte[] publicKey)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(publicKey);

        if (publicKey.Length != PublicKeySize)
        {
            throw new ArgumentException($"Invalid public key size. Expected {PublicKeySize} bytes, got {publicKey.Length}");
        }

        if (signature.Length != SignatureSize)
        {
            throw new ArgumentException($"Invalid signature size. Expected {SignatureSize} bytes, got {signature.Length}");
        }

        var signer = new DilithiumSigner();
        var rho = new byte[32];
        var t1 = new byte[_params.K * 32];

        // Extract key components from encoded public key
        Buffer.BlockCopy(publicKey, 0, rho, 0, rho.Length);
        Buffer.BlockCopy(publicKey, rho.Length, t1, 0, t1.Length);

        var publicKeyParams = new DilithiumPublicKeyParameters(_bcParams, rho, t1);
        
        signer.Init(false, publicKeyParams);
        return signer.VerifySignature(message, signature);
    }
}
