namespace QuantumLedger.Cryptography.Models;

/// <summary>
/// Represents a dual signature with both classical and quantum signatures
/// </summary>
public class DualSignature
{
    /// <summary>
    /// Gets or sets the classical signature as a base64 string
    /// </summary>
    public string ClassicSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantum signature as a base64 string
    /// </summary>
    public string QuantumSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the classical key used
    /// </summary>
    public string ClassicKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the quantum key used
    /// </summary>
    public string QuantumKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this signature was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the classical signature as a byte array
    /// </summary>
    public byte[] GetClassicSignatureBytes()
    {
        if (string.IsNullOrWhiteSpace(ClassicSignature))
            return Array.Empty<byte>();
        
        try
        {
            return Convert.FromBase64String(ClassicSignature);
        }
        catch (FormatException)
        {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Gets the quantum signature as a byte array
    /// </summary>
    public byte[] GetQuantumSignatureBytes()
    {
        if (string.IsNullOrWhiteSpace(QuantumSignature))
            return Array.Empty<byte>();
        
        try
        {
            return Convert.FromBase64String(QuantumSignature);
        }
        catch (FormatException)
        {
            return Array.Empty<byte>();
        }
    }
}

/// <summary>
/// Represents the result of a signature operation
/// </summary>
public class SignatureResult
{
    /// <summary>
    /// Gets or sets whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if unsuccessful
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the generated signature
    /// </summary>
    public DualSignature? Signature { get; set; }
}

/// <summary>
/// Represents the result of a signature verification
/// </summary>
public class VerificationResult
{
    /// <summary>
    /// Gets or sets whether both signatures are valid
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets whether the classical signature is valid
    /// </summary>
    public bool ClassicValid { get; set; }

    /// <summary>
    /// Gets or sets whether the quantum signature is valid
    /// </summary>
    public bool QuantumValid { get; set; }

    /// <summary>
    /// Gets or sets the error message if unsuccessful
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the status of cryptographic keys
/// </summary>
public class KeyStatus
{
    /// <summary>
    /// Gets or sets whether classical keys are available
    /// </summary>
    public bool ClassicKeysAvailable { get; set; }

    /// <summary>
    /// Gets or sets whether quantum keys are available
    /// </summary>
    public bool QuantumKeysAvailable { get; set; }

    /// <summary>
    /// Gets or sets the current version of the classical key
    /// </summary>
    public int? ClassicKeyVersion { get; set; }

    /// <summary>
    /// Gets or sets the current version of the quantum key
    /// </summary>
    public int? QuantumKeyVersion { get; set; }

    /// <summary>
    /// Gets or sets whether substitution keys are available
    /// </summary>
    public bool SubstitutionKeysAvailable { get; set; }

    /// <summary>
    /// Gets or sets the current version of the substitution key
    /// </summary>
    public int? SubstitutionKeyVersion { get; set; }
}

/// <summary>
/// Represents a substitution key pair for user-controlled delegation
/// </summary>
public class SubstitutionKeyPair
{
    /// <summary>
    /// Gets or sets the substitution key identifier
    /// </summary>
    public required string SubstitutionKeyId { get; set; }

    /// <summary>
    /// Gets or sets the private key (given to user)
    /// </summary>
    public required byte[] PrivateKey { get; set; }

    /// <summary>
    /// Gets or sets the public key (stored by system)
    /// </summary>
    public required byte[] PublicKey { get; set; }

    /// <summary>
    /// Gets or sets the linked main account address
    /// </summary>
    public required string LinkedAddress { get; set; }

    /// <summary>
    /// Gets or sets when this key was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when this key expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets whether this key is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets the private key as a base64 string for API responses
    /// </summary>
    public string GetPrivateKeyBase64()
    {
        return Convert.ToBase64String(PrivateKey);
    }

    /// <summary>
    /// Gets the public key as a base64 string
    /// </summary>
    public string GetPublicKeyBase64()
    {
        return Convert.ToBase64String(PublicKey);
    }
}

/// <summary>
/// Represents the result of account creation with substitution keys
/// </summary>
public class AccountCreationResult
{
    /// <summary>
    /// Gets or sets the created account address
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// Gets or sets the substitution key pair (given to user)
    /// </summary>
    public required SubstitutionKeyPair SubstitutionKey { get; set; }

    /// <summary>
    /// Gets or sets the classical key ID (kept by system)
    /// </summary>
    public required string ClassicKeyId { get; set; }

    /// <summary>
    /// Gets or sets the quantum key ID (kept by system)
    /// </summary>
    public required string QuantumKeyId { get; set; }

    /// <summary>
    /// Gets or sets when the account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of substitution key verification
/// </summary>
public class SubstitutionKeyVerificationResult
{
    /// <summary>
    /// Gets or sets whether the verification was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets whether the signature is valid
    /// </summary>
    public bool SignatureValid { get; set; }

    /// <summary>
    /// Gets or sets whether the key is authorized for the address
    /// </summary>
    public bool AuthorizedForAddress { get; set; }

    /// <summary>
    /// Gets or sets the authenticated address
    /// </summary>
    public string? AuthenticatedAddress { get; set; }

    /// <summary>
    /// Gets or sets the error message if unsuccessful
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets additional context information
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Represents criteria for substitution key operations
/// </summary>
public class SubstitutionKeyCriteria
{
    /// <summary>
    /// Gets or sets the address to filter by
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets whether to include expired keys
    /// </summary>
    public bool IncludeExpired { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include revoked keys
    /// </summary>
    public bool IncludeRevoked { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum age of keys to include
    /// </summary>
    public TimeSpan? MaxAge { get; set; }

    /// <summary>
    /// Gets or sets the minimum version to include
    /// </summary>
    public int? MinVersion { get; set; }
}
