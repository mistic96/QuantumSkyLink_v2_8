using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.PQC.Algorithms.Dilithium;

namespace QuantumLedger.Cryptography.PQC.Providers;

/// <summary>
/// Provides Dilithium signature operations implementing the ISignatureProvider interface
/// </summary>
public class DilithiumProvider : ISignatureProvider
{
    private readonly DilithiumAlgorithm _algorithm;
    private readonly ILogger<DilithiumProvider> _logger;

    /// <summary>
    /// Gets the name of the signature algorithm
    /// </summary>
    public string Algorithm => "DILITHIUM";

    /// <summary>
    /// Creates a new instance of DilithiumProvider
    /// </summary>
    /// <param name="securityLevel">NIST security level (2, 3, or 5)</param>
    /// <param name="logger">Logger instance</param>
    public DilithiumProvider(int securityLevel, ILogger<DilithiumProvider> logger)
    {
        _algorithm = new DilithiumAlgorithm(securityLevel);
        _logger = logger;
    }

    /// <inheritdoc/>
    public ValueTask<byte[]> SignAsync(byte[] message, byte[] privateKey)
    {
        try
        {
            // For CPU-bound crypto operations, execute synchronously to avoid thread pool overhead
            // This is more efficient than Task.Run() for ASP.NET Core applications
            var result = _algorithm.Sign(message, privateKey);
            return ValueTask.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Dilithium signing operation");
            throw;
        }
    }

    /// <inheritdoc/>
    public ValueTask<bool> VerifyAsync(byte[] message, byte[] signature, byte[] publicKey)
    {
        try
        {
            // For CPU-bound crypto operations, execute synchronously to avoid thread pool overhead
            // This is more efficient than Task.Run() for ASP.NET Core applications
            var result = _algorithm.Verify(message, signature, publicKey);
            return ValueTask.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Dilithium signature verification");
            throw;
        }
    }
}
