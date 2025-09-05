using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceService.Data.Entities;

/// <summary>
/// Represents the history of changes to escrow accounts
/// </summary>
[Table("EscrowHistory")]
[Index(nameof(EscrowAccountId))]
[Index(nameof(CreatedAt))]
[Index(nameof(Status))]
public class EscrowHistory : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the escrow history record
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the escrow account this history record is for
    /// </summary>
    [Required]
    [ForeignKey("EscrowAccount")]
    public Guid EscrowAccountId { get; set; }

    /// <summary>
    /// Gets or sets the status at this point in the escrow lifecycle
    /// </summary>
    [Required]
    public EscrowStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the previous status (if this is a status change)
    /// </summary>
    public EscrowStatus? PreviousStatus { get; set; }

    /// <summary>
    /// Gets or sets the action that was performed
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of what happened
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the user ID who performed the action (if applicable)
    /// </summary>
    [ForeignKey("PerformedByUser")]
    public Guid? PerformedByUserId { get; set; }

    /// <summary>
    /// Gets or sets whether this was an automatic system action
    /// </summary>
    public bool IsSystemAction { get; set; } = false;

    /// <summary>
    /// Gets or sets the system component that performed the action (if system action)
    /// </summary>
    [MaxLength(100)]
    public string? SystemComponent { get; set; }

    /// <summary>
    /// Gets or sets the transaction hash associated with this action (if applicable)
    /// </summary>
    [MaxLength(255)]
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Gets or sets the amount involved in this action (if applicable)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency for the amount (if applicable)
    /// </summary>
    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets additional details about the action (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string ActionDetails { get; set; } = "{}";

    /// <summary>
    /// Gets or sets any error message associated with this action
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the client IP address (if user action)
    /// </summary>
    [MaxLength(45)]
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent (if user action)
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the history record (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Gets or sets the escrow account for this history record
    /// </summary>
    public EscrowAccount EscrowAccount { get; set; } = null!;
}
