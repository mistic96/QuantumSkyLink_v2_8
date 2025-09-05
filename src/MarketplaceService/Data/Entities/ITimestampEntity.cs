namespace MarketplaceService.Data.Entities;

/// <summary>
/// Interface for entities that track creation and update timestamps
/// </summary>
public interface ITimestampEntity
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
