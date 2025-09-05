using LiquidStorageCloud.Core.Database;

namespace LiquidStorageCloud.DataManagement.Core.Events
{
    /// <summary>
    /// Base class for all entity events
    /// </summary>
    public abstract class BaseEntityEvent
    {
        /// <summary>
        /// Gets or sets the timestamp when the event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Event published when an entity is created
    /// </summary>
    /// <typeparam name="T">The type of entity that was created</typeparam>
    public class EntityCreatedEvent<T> : BaseEntityEvent where T : ISurrealEntity
    {
        /// <summary>
        /// Gets or sets the created entity
        /// </summary>
        public T Entity { get; set; } = default!;
    }

    /// <summary>
    /// Event published when an entity is updated
    /// </summary>
    /// <typeparam name="T">The type of entity that was updated</typeparam>
    public class EntityUpdatedEvent<T> : BaseEntityEvent where T : ISurrealEntity
    {
        /// <summary>
        /// Gets or sets the updated entity
        /// </summary>
        public T Entity { get; set; } = default!;
    }

    /// <summary>
    /// Event published when an entity is deleted
    /// </summary>
    /// <typeparam name="T">The type of entity that was deleted</typeparam>
    public class EntityDeletedEvent<T> : BaseEntityEvent where T : ISurrealEntity
    {
        /// <summary>
        /// Gets or sets the ID of the deleted entity
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the namespace of the deleted entity
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the table name of the deleted entity
        /// </summary>
        public string TableName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event published when an entity's solid state changes
    /// </summary>
    /// <typeparam name="T">The type of entity whose solid state changed</typeparam>
    public class EntitySolidStateChangedEvent<T> : BaseEntityEvent where T : ISurrealEntity
    {
        /// <summary>
        /// Gets or sets the ID of the entity
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the namespace of the entity
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the table name of the entity
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the entity is in solid state
        /// </summary>
        public bool SolidState { get; set; }
    }
}
