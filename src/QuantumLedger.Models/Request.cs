using System.Text;
using System.Text.Json;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Models;

/// <summary>
/// Represents a request in the system
/// </summary>
public class Request : ISurrealEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this request
    /// </summary>
    public string Id { get; set; } = string.Empty;



    public string TableName => "requests";
    
    public string Namespace => "ledger";

    /// <summary>
    /// Gets or sets the address associated with this request
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of request
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request data
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this request was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the classical signature as a base64 string
    /// </summary>
    public string ClassicSignature { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantum-resistant signature as a base64 string
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
    /// Gets the classical signature as a byte array
    /// </summary>

    public bool SolidState { get; set; }
    public DateTimeOffset LastModified { get; set; }
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

    /// <summary>
    /// Gets the content that is signed
    /// </summary>
    /// <returns>The signed content as bytes</returns>
    public byte[] GetSignedContent()
    {
        // Create a canonical representation of the request
        var content = new
        {
            Id,
            Address,
            Type,
            Data,
            CreatedAt = CreatedAt.ToString("O") // ISO 8601 format
        };

        // Serialize to JSON with consistent formatting
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions
        {
            WriteIndented = false // No whitespace for consistent bytes
        });

        // Convert to UTF-8 bytes
        return Encoding.UTF8.GetBytes(json);
    }
}
