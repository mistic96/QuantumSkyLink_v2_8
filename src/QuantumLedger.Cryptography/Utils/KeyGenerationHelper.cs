extern alias BC;
using BC::Org.BouncyCastle.Asn1.Nist;
using BC::Org.BouncyCastle.Crypto;
using BC::Org.BouncyCastle.Crypto.Generators;
using BC::Org.BouncyCastle.Crypto.Parameters;
using BC::Org.BouncyCastle.Pkcs;
using BC::Org.BouncyCastle.Security;
using BC::Org.BouncyCastle.X509;
using QuantumLedger.Cryptography.Constants;
using QuantumLedger.Cryptography.Exceptions;

namespace QuantumLedger.Cryptography.Utils;

/// <summary>
/// Helper class for cryptographic key generation
/// </summary>
public static class KeyGenerationHelper
{
    private static readonly SecureRandom Random = new();

    /// <summary>
    /// Generates a new EC-256 key pair
    /// </summary>
    /// <returns>A tuple containing the encoded private and public keys</returns>
    /// <exception cref="KeyOperationException">Thrown when key generation fails</exception>
    public static (byte[] privateKey, byte[] publicKey) GenerateEC256KeyPair()
    {
        try
        {
            // Create the key generator
            var keyGen = new ECKeyPairGenerator();
            var keyGenParams = new ECKeyGenerationParameters(
                new ECDomainParameters(NistNamedCurves.GetByName(CryptoConstants.Curves.P256)),
                Random);

            // Initialize and generate the key pair
            keyGen.Init(keyGenParams);
            var keyPair = keyGen.GenerateKeyPair();

            // Extract and encode the keys
            var privateKey = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private).GetDerEncoded();
            var publicKey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public).GetDerEncoded();

            return (privateKey, publicKey);
        }
        catch (Exception ex)
        {
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Generate,
                CryptoConstants.Algorithms.EC256,
                ex);
        }
    }

    /// <summary>
    /// Validates that a private key and public key form a valid key pair
    /// </summary>
    /// <param name="privateKey">The encoded private key</param>
    /// <param name="publicKey">The encoded public key</param>
    /// <param name="algorithm">The algorithm the keys are for</param>
    /// <returns>True if the keys form a valid pair, false otherwise</returns>
    /// <exception cref="KeyOperationException">Thrown when validation fails due to an error</exception>
    public static bool ValidateKeyPair(byte[] privateKey, byte[] publicKey, string algorithm)
    {
        try
        {
            // Load the keys
            var privateKeyParams = PrivateKeyFactory.CreateKey(privateKey);
            var publicKeyParams = PublicKeyFactory.CreateKey(publicKey);

            // For EC keys, check they're using the same curve
            if (privateKeyParams is ECPrivateKeyParameters ecPrivate &&
                publicKeyParams is ECPublicKeyParameters ecPublic)
            {
                return ecPrivate.Parameters.Curve.Equals(ecPublic.Parameters.Curve) &&
                       ecPrivate.Parameters.G.Equals(ecPublic.Parameters.G);
            }

            // For other key types, implement specific validation
            throw new NotImplementedException($"Key validation not implemented for algorithm: {algorithm}");
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Validate,
                algorithm,
                ex);
        }
    }

    /// <summary>
    /// Generates a random seed of the specified length
    /// </summary>
    /// <param name="length">The length of the seed in bytes</param>
    /// <returns>The random seed</returns>
    public static byte[] GenerateRandomSeed(int length)
    {
        var seed = new byte[length];
        Random.NextBytes(seed);
        return seed;
    }

    /// <summary>
    /// Generates a new Dilithium key pair
    /// </summary>
    /// <returns>A tuple containing the encoded private and public keys</returns>
    /// <exception cref="NotImplementedException">Thrown as this is a placeholder for future implementation</exception>
    public static (byte[] privateKey, byte[] publicKey) GenerateDilithiumKeyPair()
    {
        throw new NotImplementedException(
            "Dilithium key generation not yet implemented. This will be implemented when the PQC library is integrated.");
    }

    /// <summary>
    /// Generates a new Falcon key pair
    /// </summary>
    /// <returns>A tuple containing the encoded private and public keys</returns>
    /// <exception cref="NotImplementedException">Thrown as this is a placeholder for future implementation</exception>
    public static (byte[] privateKey, byte[] publicKey) GenerateFalconKeyPair()
    {
        throw new NotImplementedException(
            "Falcon key generation not yet implemented. This will be implemented when the PQC library is integrated.");
    }
}
