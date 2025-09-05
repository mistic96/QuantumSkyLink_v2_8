using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Service for generating Coinbase API request signatures using ECDSA
/// </summary>
public class CoinbaseSignatureService : ICoinbaseSignatureService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CoinbaseSignatureService> _logger;
    private readonly string _apiKey;
    private readonly string _privateKey;
    private ECDsa? _ecDsa;

    public CoinbaseSignatureService(
        IConfiguration configuration,
        ILogger<CoinbaseSignatureService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["Coinbase:ApiKey"] ?? string.Empty;
        _privateKey = configuration["Coinbase:PrivateKey"] ?? string.Empty;
        
        InitializeECDsa();
    }

    /// <summary>
    /// Generates a signature for a Coinbase API request
    /// </summary>
    public string GenerateSignature(string method, string path, string body, string timestamp)
    {
        try
        {
            if (_ecDsa == null)
            {
                throw new InvalidOperationException("ECDSA not initialized");
            }

            // Create the message to sign: timestamp + method + path + body
            var message = $"{timestamp}{method.ToUpper()}{path}{body}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            // Sign the message
            var signature = _ecDsa.SignData(messageBytes, HashAlgorithmName.SHA256);

            // Return base64 encoded signature
            return Convert.ToBase64String(signature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Coinbase signature");
            throw;
        }
    }

    /// <summary>
    /// Generates JWT token for WebSocket authentication
    /// </summary>
    public string GenerateWebSocketJWT()
    {
        try
        {
            if (_ecDsa == null)
            {
                throw new InvalidOperationException("ECDSA not initialized");
            }

            var now = DateTimeOffset.UtcNow;
            var exp = now.AddMinutes(120); // 2 hour expiration

            var header = new Dictionary<string, object>
            {
                ["alg"] = "ES256",
                ["kid"] = _apiKey,
                ["nonce"] = Guid.NewGuid().ToString()
            };

            var payload = new Dictionary<string, object>
            {
                ["sub"] = _apiKey,
                ["iss"] = "coinbase-cloud",
                ["nbf"] = now.ToUnixTimeSeconds(),
                ["exp"] = exp.ToUnixTimeSeconds(),
                ["aud"] = new[] { "retail_rest_api_proxy" }
            };

            var securityKey = new ECDsaSecurityKey(_ecDsa);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(),
                Expires = exp.DateTime,
                SigningCredentials = credentials,
                Claims = payload.Select(kvp => new System.Security.Claims.Claim(kvp.Key, kvp.Value.ToString())).ToDictionary(c => c.Type, c => (object)c.Value),
                AdditionalHeaderClaims = header
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating WebSocket JWT");
            throw;
        }
    }

    /// <summary>
    /// Validates that the signature service is properly configured
    /// </summary>
    public bool ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("Coinbase API key is not configured");
            return false;
        }

        if (string.IsNullOrEmpty(_privateKey))
        {
            _logger.LogError("Coinbase private key is not configured");
            return false;
        }

        if (_ecDsa == null)
        {
            _logger.LogError("ECDSA algorithm could not be initialized");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Initializes the ECDSA algorithm with the private key
    /// </summary>
    private void InitializeECDsa()
    {
        try
        {
            if (string.IsNullOrEmpty(_privateKey))
            {
                _logger.LogWarning("Coinbase private key not configured");
                return;
            }

            // Decode base64 private key if needed
            string pemKey;
            if (_privateKey.StartsWith("-----BEGIN"))
            {
                pemKey = _privateKey;
            }
            else
            {
                // Assume it's base64 encoded
                var keyBytes = Convert.FromBase64String(_privateKey);
                pemKey = Encoding.UTF8.GetString(keyBytes);
            }

            // Parse the PEM private key using BouncyCastle
            using var reader = new StringReader(pemKey);
            var pemReader = new PemReader(reader);
            var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;

            if (keyPair == null)
            {
                // Try reading as private key directly
                var privateKeyParam = pemReader.ReadObject() as ECPrivateKeyParameters;
                if (privateKeyParam == null)
                {
                    throw new InvalidOperationException("Failed to parse private key");
                }
                keyPair = new AsymmetricCipherKeyPair(null, privateKeyParam);
            }

            var privateKeyParams = keyPair.Private as ECPrivateKeyParameters;
            if (privateKeyParams == null)
            {
                throw new InvalidOperationException("Invalid EC private key");
            }

            // Convert to .NET ECDsa
            _ecDsa = ConvertToECDsa(privateKeyParams);

            _logger.LogInformation("Coinbase ECDSA signature service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ECDSA with private key");
            _ecDsa = null;
        }
    }

    /// <summary>
    /// Converts BouncyCastle EC parameters to .NET ECDsa
    /// </summary>
    private ECDsa ConvertToECDsa(ECPrivateKeyParameters privateKeyParams)
    {
        var domainParams = privateKeyParams.Parameters;
        var d = privateKeyParams.D.ToByteArrayUnsigned();
        var ecPoint = domainParams.G.Multiply(privateKeyParams.D);
        var x = ecPoint.Normalize().XCoord.ToBigInteger().ToByteArrayUnsigned();
        var y = ecPoint.Normalize().YCoord.ToBigInteger().ToByteArrayUnsigned();

        var ecParams = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            D = d,
            Q = new ECPoint
            {
                X = x,
                Y = y
            }
        };

        var ecdsa = ECDsa.Create();
        ecdsa.ImportParameters(ecParams);
        return ecdsa;
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        _ecDsa?.Dispose();
    }
}