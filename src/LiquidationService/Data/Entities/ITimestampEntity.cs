namespace LiquidationService.Data.Entities;

/// <summary>
/// Interface for entities that track creation and modification timestamps
/// </summary>
public interface ITimestampEntity
{
    /// <summary>
    /// The date and time when the entity was created
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the entity was last updated
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
