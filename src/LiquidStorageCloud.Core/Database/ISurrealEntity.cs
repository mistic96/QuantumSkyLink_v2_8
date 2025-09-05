namespace LiquidStorageCloud.Core.Database
{
    /// <summary>
    /// Base interface for all SurrealDB entities
    /// </summary>
    public interface ISurrealEntity
    {
        /// <summary>
        /// Gets the namespace for the entity in SurrealDB
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the table name for the entity in SurrealDB
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Gets or sets the unique identifier for the entity
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets whether the entity is in a solid state
        /// Solid state indicates the entity has been successfully processed by all downstream systems
        /// </summary>
        bool SolidState { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the entity was last modified
        /// </summary>
        DateTimeOffset LastModified { get; set; }
    }
}
