using System.Linq.Expressions;
using LiquidStorageCloud.Core.Database;

namespace LiquidStorageCloud.DataManagement.Core.Repository
{
    /// <summary>
    /// Repository interface for SurrealDB operations
    /// </summary>
    /// <typeparam name="T">The type of entity to manage</typeparam>
    public interface ISurrealRepository<T> where T : ISurrealEntity
    {
        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created entity</returns>
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="id">The ID of the entity to delete</param>
        /// <param name="cancellationToken">The cancellation token</param>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an entity by ID
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists entities with optional filtering, ordering, and pagination
        /// </summary>
        /// <param name="filter">Optional filter predicate</param>
        /// <param name="orderBy">Optional order by expression</param>
        /// <param name="ascending">Whether to order in ascending order</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="onlySolidState">Whether to only return solid state records</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The matching entities</returns>
        Task<IEnumerable<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true,
            int skip = 0,
            int take = int.MaxValue,
            bool onlySolidState = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a raw SurrealQL query
        /// </summary>
        /// <typeparam name="TResult">The type of result to return</typeparam>
        /// <param name="query">The query to execute</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The query results</returns>
        Task<IEnumerable<TResult>> QueryAsync<TResult>(
            string query,
            IDictionary<string, object>? parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the solid state of an entity
        /// </summary>
        /// <param name="id">The ID of the entity</param>
        /// <param name="solidState">Whether to set as solid state</param>
        /// <param name="cancellationToken">The cancellation token</param>
        Task SetSolidStateAsync(string id, bool solidState, CancellationToken cancellationToken = default);
    }
}
