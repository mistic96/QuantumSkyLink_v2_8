using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuantumLedger.Models.Interfaces
{
    /// <summary>
    /// Defines the base repository operations for Quantum Ledger entities.
    /// </summary>
    /// <typeparam name="T">The type of entity this repository handles.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the entity to retrieve.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        Task<T> GetByIdAsync(string id);

        /// <summary>
        /// Gets all entities of type T.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The added entity.</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateAsync(T entity);
    }

    /// <summary>
    /// Defines specific repository operations for Request entities.
    /// </summary>
    public interface IRequestRepository : IRepository<Request>
    {
        /// <summary>
        /// Gets requests within a specific time range.
        /// </summary>
        /// <param name="startTime">The start of the time range.</param>
        /// <param name="endTime">The end of the time range.</param>
        /// <returns>A collection of requests within the specified time range.</returns>
        Task<IEnumerable<Request>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);
    }

    /// <summary>
    /// Defines specific repository operations for AuditLog entities.
    /// </summary>
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        /// <summary>
        /// Gets all audit logs for a specific request.
        /// </summary>
        /// <param name="requestId">The ID of the request.</param>
        /// <returns>A collection of audit logs for the specified request.</returns>
        Task<IEnumerable<AuditLog>> GetByRequestIdAsync(string requestId);
    }

    /// <summary>
    /// Defines specific repository operations for AccountBalance entities.
    /// </summary>
    public interface IAccountBalanceRepository : IRepository<AccountBalance>
    {
        /// <summary>
        /// Gets an account balance by its blockchain address.
        /// </summary>
        /// <param name="address">The blockchain address.</param>
        /// <returns>The account balance if found; otherwise, null.</returns>
        Task<AccountBalance> GetByAddressAsync(string address);

        /// <summary>
        /// Updates the balance for a specific account.
        /// </summary>
        /// <param name="address">The blockchain address.</param>
        /// <param name="amount">The amount to add (or subtract if negative).</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateBalanceAsync(string address, decimal amount);
    }
}
