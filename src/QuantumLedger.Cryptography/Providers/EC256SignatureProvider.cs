extern alias BC;
using Microsoft.Extensions.Logging;
using BC::Org.BouncyCastle.Asn1;
using BC::Org.BouncyCastle.Asn1.Nist;
using BC::Org.BouncyCastle.Crypto;
using BC::Org.BouncyCastle.Crypto.Parameters;
using BC::Org.BouncyCastle.Crypto.Signers;
using BC::Org.BouncyCastle.Math;
using BC::Org.BouncyCastle.Security;
using QuantumLedger.Cryptography.Constants;
using QuantumLedger.Cryptography.Exceptions;
using QuantumLedger.Cryptography.Interfaces;

namespace QuantumLedger.Cryptography.Providers;

/// <summary>
/// Provides ECDSA signature operations using the P-256 curve and SHA-256 hash
/// </summary>
public class EC256SignatureProvider : ISignatureProvider
{
    private readonly ILogger<EC256SignatureProvider> _logger;
    private readonly SecureRandom _random;

    /// <summary>
    /// Gets the name of the signature algorithm
    /// </summary>
    public string Algorithm => CryptoConstants.Algorithms.EC256;

    /// <summary>
    /// Creates a new instance of EC256SignatureProvider
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public EC256SignatureProvider(ILogger<EC256SignatureProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new SecureRandom();
    }

    /// <inheritdoc/>
    public ValueTask<byte[]> SignAsync(byte[] message, byte[] privateKey)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(privateKey);

        try
        {
            // Create the signer
            var signer = new ECDsaSigner(new HMacDsaKCalculator(DigestUtilities.GetDigest(CryptoConstants.HashAlgorithms.SHA256)));

            // Load the private key
            var privateKeyParams = PrivateKeyFactory.CreateKey(privateKey);
            if (privateKeyParams is not ECPrivateKeyParameters ecPrivateKey)
            {
                throw new KeyOperationException(
                    CryptoConstants.KeyOperations.Validate,
                    Algorithm,
                    new ArgumentException("Invalid private key format"));
            }

            // Initialize the signer for signing
            signer.Init(true, ecPrivateKey);

            // Sign the message
            var signature = signer.GenerateSignature(message);

            // Encode the signature in ASN.1 format - execute synchronously for better performance
            var result = EncodeSignature(signature);
            return ValueTask.FromResult(result);
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            _logger.LogError(ex, "Error during signing operation");
            throw new SigningException(Algorithm, ex);
        }
    }

    /// <inheritdoc/>
    public ValueTask<bool> VerifyAsync(byte[] message, byte[] signature, byte[] publicKey)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(publicKey);

        try
        {
            // Create the signer
            var signer = new ECDsaSigner(new HMacDsaKCalculator(DigestUtilities.GetDigest(CryptoConstants.HashAlgorithms.SHA256)));

            // Load the public key
            var publicKeyParams = PublicKeyFactory.CreateKey(publicKey);
            if (publicKeyParams is not ECPublicKeyParameters ecPublicKey)
            {
                throw new KeyOperationException(
                    CryptoConstants.KeyOperations.Validate,
                    Algorithm,
                    new ArgumentException("Invalid public key format"));
            }

            // Initialize the signer for verification
            signer.Init(false, ecPublicKey);

            // Decode the signature from ASN.1 format - execute synchronously for better performance
            var signatureComponents = DecodeSignature(signature);

            // Verify the signature
            var result = signer.VerifySignature(message, signatureComponents.R, signatureComponents.S);
            return ValueTask.FromResult(result);
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            _logger.LogError(ex, "Error during signature verification");
            throw new SignatureVerificationException(Algorithm, ex);
        }
    }

    private byte[] EncodeSignature(BigInteger[] signature)
    {
        try
        {
            var derSequence = new DerSequence(
                new DerInteger(signature[0]),
                new DerInteger(signature[1]));
            return derSequence.GetEncoded();
        }
        catch (Exception ex)
        {
            throw new SigningException(Algorithm, ex);
        }
    }

    private (BigInteger R, BigInteger S) DecodeSignature(byte[] signature)
    {
        try
        {
            var sequence = (DerSequence)Asn1Object.FromByteArray(signature);
            var r = ((DerInteger)sequence[0]).Value;
            var s = ((DerInteger)sequence[1]).Value;
            return (r, s);
        }
        catch (Exception ex)
        {
            throw new SignatureVerificationException(Algorithm, ex);
        }
    }
}
