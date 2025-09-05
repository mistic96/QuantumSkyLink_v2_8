using System;

namespace QuantumLedger.Models.Interfaces
{
    /// <summary>
    /// Temporary interface to replace LiquidStorageCloud.Core.Database.ISurrealEntity
    /// until the dependency is properly resolved.
    /// </summary>
    public interface ISurrealEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entity.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets the table name for the database.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Gets the namespace for the database.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets or sets whether the entity is in a solid state.
        /// </summary>
        bool SolidState { get; set; }

        /// <summary>
        /// Gets or sets the last modified timestamp.
        /// </summary>
        DateTimeOffset LastModified { get; set; }
    }
}
