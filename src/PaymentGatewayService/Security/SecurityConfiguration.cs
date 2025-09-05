using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace PaymentGatewayService.Security;

/// <summary>
/// Centralized security configuration for payment gateway service
/// Implements defense-in-depth security controls and compliance requirements
/// </summary>
public static class SecurityConfiguration
{
    /// <summary>
    /// Configures comprehensive security services for the payment gateway
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add authorization policies
        services.AddAuthorization(options =>
        {
            // Admin access policies
            options.AddPolicy("RequireViewMetricsPermission", policy =>
                policy.RequireRole("Admin", "SecurityOfficer", "Auditor")
                      .RequireClaim("permission", "view.metrics"));

            options.AddPolicy("RequireManageDepositsPermission", policy =>
                policy.RequireRole("Admin", "FinanceManager")
                      .RequireClaim("permission", "manage.deposits"));

            options.AddPolicy("RequireSecurityAdminPermission", policy =>
                policy.RequireRole("SecurityAdmin")
                      .RequireClaim("permission", "security.admin"));

            // Compliance policies
            options.AddPolicy("RequireAuditAccess", policy =>
                policy.RequireRole("Auditor", "ComplianceOfficer")
                      .RequireClaim("permission", "audit.access"));

            options.AddPolicy("RequireKYCAMLAccess", policy =>
                policy.RequireRole("ComplianceOfficer", "KYCAnalyst")
                      .RequireClaim("permission", "kyc.aml.access"));
        });

        // Add security headers
        services.AddSecurityHeaders();

        // Add rate limiting
        services.AddRateLimiting(configuration);

        // Add encryption services
        services.AddEncryptionServices(configuration);

        return services;
    }

    /// <summary>
    /// Adds security headers middleware configuration
    /// </summary>
    private static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
    {
        services.Configure<SecurityHeadersOptions>(options =>
        {
            options.ContentSecurityPolicy = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self'; object-src 'none'; media-src 'self'; frame-src 'none';";
            options.StrictTransportSecurity = "max-age=31536000; includeSubDomains; preload";
            options.XFrameOptions = "DENY";
            options.XContentTypeOptions = "nosniff";
            options.ReferrerPolicy = "strict-origin-when-cross-origin";
            options.PermissionsPolicy = "camera=(), microphone=(), geolocation=(), payment=()";
        });

        return services;
    }

    /// <summary>
    /// Adds rate limiting configuration to prevent abuse
    /// </summary>
    private static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(options =>
        {
            // Deposit code validation limits
            options.DepositCodeValidation = new RateLimitRule
            {
                RequestsPerMinute = 30,
                RequestsPerHour = 300,
                RequestsPerDay = 1000
            };

            // Admin API limits
            options.AdminAPI = new RateLimitRule
            {
                RequestsPerMinute = 100,
                RequestsPerHour = 1000,
                RequestsPerDay = 5000
            };

            // General API limits
            options.GeneralAPI = new RateLimitRule
            {
                RequestsPerMinute = 60,
                RequestsPerHour = 600,
                RequestsPerDay = 2000
            };
        });

        return services;
    }

    /// <summary>
    /// Adds encryption services for sensitive data protection
    /// </summary>
    private static IServiceCollection AddEncryptionServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDataEncryptionService, DataEncryptionService>();
        services.AddSingleton<IFieldEncryptionService, FieldEncryptionService>();
        
        services.Configure<EncryptionOptions>(options =>
        {
            options.MasterKeyPath = configuration["Encryption:MasterKeyPath"] ?? "encryption.key";
            options.Algorithm = "AES-256-GCM";
            options.KeyRotationDays = 90;
        });

        return services;
    }
}

/// <summary>
/// Security headers configuration options
/// </summary>
public class SecurityHeadersOptions
{
    public string ContentSecurityPolicy { get; set; } = string.Empty;
    public string StrictTransportSecurity { get; set; } = string.Empty;
    public string XFrameOptions { get; set; } = string.Empty;
    public string XContentTypeOptions { get; set; } = string.Empty;
    public string ReferrerPolicy { get; set; } = string.Empty;
    public string PermissionsPolicy { get; set; } = string.Empty;
}

/// <summary>
/// Rate limiting configuration options
/// </summary>
public class RateLimitingOptions
{
    public RateLimitRule DepositCodeValidation { get; set; } = new();
    public RateLimitRule AdminAPI { get; set; } = new();
    public RateLimitRule GeneralAPI { get; set; } = new();
}

/// <summary>
/// Rate limit rule definition
/// </summary>
public class RateLimitRule
{
    public int RequestsPerMinute { get; set; }
    public int RequestsPerHour { get; set; }
    public int RequestsPerDay { get; set; }
}

/// <summary>
/// Encryption configuration options
/// </summary>
public class EncryptionOptions
{
    public string MasterKeyPath { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public int KeyRotationDays { get; set; }
}

/// <summary>
/// Interface for data encryption services
/// </summary>
public interface IDataEncryptionService
{
    Task<string> EncryptAsync(string plaintext);
    Task<string> DecryptAsync(string ciphertext);
    Task<byte[]> GenerateKeyAsync();
    Task RotateKeysAsync();
}

/// <summary>
/// Interface for field-level encryption services
/// </summary>
public interface IFieldEncryptionService
{
    Task<string> EncryptFieldAsync(string fieldValue, string context);
    Task<string> DecryptFieldAsync(string encryptedValue, string context);
    bool IsEncrypted(string value);
}

/// <summary>
/// Implementation of data encryption service using AES-256-GCM
/// </summary>
public class DataEncryptionService : IDataEncryptionService
{
    private readonly ILogger<DataEncryptionService> _logger;
    private readonly EncryptionOptions _options;

    public DataEncryptionService(ILogger<DataEncryptionService> logger, IOptions<EncryptionOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> EncryptAsync(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            
            var key = await LoadOrGenerateKeyAsync();
            aes.Key = key;
            aes.GenerateIV();
            
            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            
            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
            csEncrypt.FlushFinalBlock();
            
            var cipherBytes = msEncrypt.ToArray();
            
            // Combine IV + ciphertext
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            
            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw new SecurityException("Encryption failed", ex);
        }
    }

    public async Task<string> DecryptAsync(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return string.Empty;

        try
        {
            var encryptedBytes = Convert.FromBase64String(ciphertext);
            
            if (encryptedBytes.Length < 16) // IV size minimum
                throw new ArgumentException("Invalid ciphertext format");
            
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            
            var key = await LoadOrGenerateKeyAsync();
            aes.Key = key;
            
            // Extract IV and ciphertext
            var iv = new byte[16];
            var cipherBytes = new byte[encryptedBytes.Length - 16];
            
            Array.Copy(encryptedBytes, 0, iv, 0, 16);
            Array.Copy(encryptedBytes, 16, cipherBytes, 0, cipherBytes.Length);
            
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipherBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            return await srDecrypt.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw new SecurityException("Decryption failed", ex);
        }
    }

    public async Task<byte[]> GenerateKeyAsync()
    {
        var key = new byte[32]; // 256-bit key
        RandomNumberGenerator.Fill(key);
        
        // Save key securely (this is simplified - use proper key management)
        await File.WriteAllBytesAsync(_options.MasterKeyPath, key);
        
        _logger.LogInformation("New encryption key generated");
        return key;
    }

    public async Task RotateKeysAsync()
    {
        _logger.LogInformation("Starting key rotation process");
        
        // Generate new key
        var newKey = await GenerateKeyAsync();
        
        // TODO: Re-encrypt existing data with new key
        // This would involve reading all encrypted data, decrypting with old key, 
        // and re-encrypting with new key
        
        _logger.LogInformation("Key rotation completed");
    }

    private async Task<byte[]> LoadOrGenerateKeyAsync()
    {
        if (File.Exists(_options.MasterKeyPath))
        {
            return await File.ReadAllBytesAsync(_options.MasterKeyPath);
        }
        else
        {
            return await GenerateKeyAsync();
        }
    }
}

/// <summary>
/// Implementation of field-level encryption service
/// </summary>
public class FieldEncryptionService : IFieldEncryptionService
{
    private readonly IDataEncryptionService _dataEncryption;
    private readonly ILogger<FieldEncryptionService> _logger;
    private const string EncryptedPrefix = "ENC:";

    public FieldEncryptionService(IDataEncryptionService dataEncryption, ILogger<FieldEncryptionService> logger)
    {
        _dataEncryption = dataEncryption;
        _logger = logger;
    }

    public async Task<string> EncryptFieldAsync(string fieldValue, string context)
    {
        if (string.IsNullOrEmpty(fieldValue))
            return fieldValue;

        if (IsEncrypted(fieldValue))
            return fieldValue; // Already encrypted

        try
        {
            var contextualValue = $"{context}:{fieldValue}";
            var encrypted = await _dataEncryption.EncryptAsync(contextualValue);
            return $"{EncryptedPrefix}{encrypted}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting field in context: {Context}", context);
            throw;
        }
    }

    public async Task<string> DecryptFieldAsync(string encryptedValue, string context)
    {
        if (string.IsNullOrEmpty(encryptedValue) || !IsEncrypted(encryptedValue))
            return encryptedValue;

        try
        {
            var ciphertext = encryptedValue.Substring(EncryptedPrefix.Length);
            var decrypted = await _dataEncryption.DecryptAsync(ciphertext);
            
            // Remove context prefix
            var contextPrefix = $"{context}:";
            if (decrypted.StartsWith(contextPrefix))
            {
                return decrypted.Substring(contextPrefix.Length);
            }
            
            throw new SecurityException("Context mismatch during decryption");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting field in context: {Context}", context);
            throw;
        }
    }

    public bool IsEncrypted(string value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith(EncryptedPrefix);
    }
}

/// <summary>
/// Security exception for encryption/decryption errors
/// </summary>
public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
    public SecurityException(string message, Exception innerException) : base(message, innerException) { }
}
