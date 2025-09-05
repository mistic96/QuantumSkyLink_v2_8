namespace NotificationService.Data.Entities;

public interface ITimestampEntity
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
