using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Constants;
using QuantumLedger.Cryptography.Exceptions;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Models;

namespace QuantumLedger.Cryptography.Pipeline;

/// <summary>
/// Implements the signature pipeline for dual signature operations
/// </summary>
public class SignaturePipeline : ISignaturePipeline
{
    private readonly IKeyManager _keyManager;
    private readonly ISignatureProvider _classicProvider;
    private readonly ISignatureProvider _quantumProvider;
    private readonly ILogger<SignaturePipeline> _logger;

    /// <summary>
    /// Creates a new instance of SignaturePipeline
    /// </summary>
    /// <param name="keyManager">Key manager instance</param>
    /// <param name="classicProvider">Classical signature provider</param>
    /// <param name="quantumProvider">Quantum signature provider</param>
    /// <param name="logger">Logger instance</param>
    public SignaturePipeline(
        IKeyManager keyManager,
        ISignatureProvider classicProvider,
        ISignatureProvider quantumProvider,
        ILogger<SignaturePipeline> logger)
    {
        _keyManager = keyManager;
        _classicProvider = classicProvider;
        _quantumProvider = quantumProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SignatureResult> SignAsync(byte[] message, string address)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            // Get current keys
            var classicKeyId = await GetLatestKeyIdAsync(address, _classicProvider.Algorithm, KeyCategory.Traditional);
            var quantumKeyId = await GetLatestKeyIdAsync(address, _quantumProvider.Algorithm, KeyCategory.PostQuantum);

            if (classicKeyId == null || quantumKeyId == null)
            {
                return new SignatureResult
                {
                    Success = false,
                    ErrorMessage = "No valid keys available"
                };
            }

            // Get private keys
            var classicPrivateKey = await _keyManager.GetPrivateKeyAsync(classicKeyId);
            var quantumPrivateKey = await _keyManager.GetPrivateKeyAsync(quantumKeyId);

            // Generate signatures
            var classicSignature = await _classicProvider.SignAsync(message, classicPrivateKey);
            var quantumSignature = await _quantumProvider.SignAsync(message, quantumPrivateKey);

            return new SignatureResult
            {
                Success = true,
                Signature = new DualSignature
                {
                    ClassicSignature = Convert.ToBase64String(classicSignature),
                    QuantumSignature = Convert.ToBase64String(quantumSignature),
                    ClassicKeyId = classicKeyId,
                    QuantumKeyId = quantumKeyId,
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            _logger.LogError(ex, "Failed to generate signatures for address {Address}", address);
            return new SignatureResult
            {
                Success = false,
                ErrorMessage = "Internal error during signing"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<VerificationResult> VerifyAsync(byte[] message, DualSignature signature)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(signature);

        try
        {
            // Get public keys
            var classicPublicKey = await _keyManager.GetPublicKeyAsync(signature.ClassicKeyId);
            var quantumPublicKey = await _keyManager.GetPublicKeyAsync(signature.QuantumKeyId);

            // Get signature bytes
            var classicSignatureBytes = signature.GetClassicSignatureBytes();
            var quantumSignatureBytes = signature.GetQuantumSignatureBytes();

            // Verify signatures
            var classicValid = await _classicProvider.VerifyAsync(
                message,
                classicSignatureBytes,
                classicPublicKey);

            var quantumValid = await _quantumProvider.VerifyAsync(
                message,
                quantumSignatureBytes,
                quantumPublicKey);

            return new VerificationResult
            {
                Success = classicValid && quantumValid,
                ClassicValid = classicValid,
                QuantumValid = quantumValid
            };
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            _logger.LogError(ex, "Failed to verify signatures");
            return new VerificationResult
            {
                Success = false,
                ErrorMessage = "Internal error during verification"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<KeyStatus> GetKeyStatusAsync(string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var classicKeyId = await GetLatestKeyIdAsync(address, _classicProvider.Algorithm, KeyCategory.Traditional);
            var quantumKeyId = await GetLatestKeyIdAsync(address, _quantumProvider.Algorithm, KeyCategory.PostQuantum);

            var status = new KeyStatus
            {
                ClassicKeysAvailable = classicKeyId != null,
                QuantumKeysAvailable = quantumKeyId != null
            };

            if (classicKeyId != null)
            {
                status.ClassicKeyVersion = await _keyManager.GetVersionAsync(classicKeyId);
            }

            if (quantumKeyId != null)
            {
                status.QuantumKeyVersion = await _keyManager.GetVersionAsync(quantumKeyId);
            }

            return status;
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            _logger.LogError(ex, "Failed to get key status for address {Address}", address);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Validate,
                "Unknown",
                ex);
        }
    }

    /// <inheritdoc/>
    public async Task<(string classicKeyId, string quantumKeyId)> RotateKeysAsync(string address)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);

        try
        {
            var classicKeyId = await _keyManager.RotateKeysAsync(address, KeyCategory.Traditional);
            var quantumKeyId = await _keyManager.RotateKeysAsync(address, KeyCategory.PostQuantum);

            _logger.LogInformation(
                "Rotated keys for address {Address}: Classic {ClassicKeyId}, Quantum {QuantumKeyId}",
                address, classicKeyId, quantumKeyId);

            return (classicKeyId, quantumKeyId);
        }
        catch (Exception ex) when (ex is not CryptographicException)
        {
            _logger.LogError(ex, "Failed to rotate keys for address {Address}", address);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Generate,
                "Unknown",
                ex);
        }
    }

    private async Task<string?> GetLatestKeyIdAsync(string address, string algorithm, KeyCategory category)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);
        ArgumentException.ThrowIfNullOrEmpty(algorithm);

        try
        {
            // Get the latest key ID
            var keyId = await _keyManager.GetLatestKeyPairAsync(address, category);
            if (keyId == null)
            {
                _logger.LogInformation(
                    "No keys found for address {Address} algorithm {Algorithm}, generating initial key pair",
                    address, algorithm);
                return await _keyManager.GenerateKeyPairAsync(address, algorithm, category, 1);
            }

            return keyId;
        }
        catch (Exception ex) when (ex is not KeyOperationException)
        {
            _logger.LogError(ex, 
                "Failed to get latest key ID for address {Address} algorithm {Algorithm}",
                address, algorithm);
            throw new KeyOperationException(
                CryptoConstants.KeyOperations.Retrieve,
                algorithm,
                ex);
        }
    }
}
