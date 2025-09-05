using Microsoft.Extensions.Logging;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.PQC.Algorithms.Falcon;

namespace QuantumLedger.Cryptography.PQC.Providers;

/// <summary>
/// Provides Falcon signature operations implementing the ISignatureProvider interface
/// </summary>
public class FalconProvider : ISignatureProvider
{
    private readonly FalconAlgorithm _algorithm;
    private readonly ILogger<FalconProvider> _logger;

    /// <summary>
    /// Gets the name of the signature algorithm
    /// </summary>
    public string Algorithm => "FALCON";

    /// <summary>
    /// Creates a new instance of FalconProvider
    /// </summary>
    /// <param name="treeHeight">Height of the Falcon tree (8, 9, or 10)</param>
    /// <param name="logger">Logger instance</param>
    public FalconProvider(int treeHeight, ILogger<FalconProvider> logger)
    {
        _algorithm = new FalconAlgorithm(treeHeight);
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
            _logger.LogError(ex, "Error during Falcon signing operation");
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
            _logger.LogError(ex, "Error during Falcon signature verification");
            throw;
        }
    }
}
