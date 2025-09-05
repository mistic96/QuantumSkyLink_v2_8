using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumLedger.Data.Entities;

[Table("ObjectStorageReferences")]
public class ObjectStorageReference
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(500)]
    public string S3Key { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? S3Etag { get; set; }
    
    [MaxLength(50)]
    public string BucketName { get; set; } = "quantumskylink";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
