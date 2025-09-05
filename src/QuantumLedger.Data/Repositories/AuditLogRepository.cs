using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuantumLedger.Data.Storage;
using QuantumLedger.Models;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Data.Repositories
{
    /// <summary>
    /// Repository implementation for AuditLog entities.
    /// </summary>
    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        private const string EntityType = "auditlog";

        /// <summary>
        /// Initializes a new instance of the AuditLogRepository class.
        /// </summary>
        /// <param name="dataStore">The data store to use for persistence.</param>
        public AuditLogRepository(IDataStore dataStore)
            : base(dataStore, EntityType)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AuditLog>> GetByRequestIdAsync(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
                throw new ArgumentException("Request ID cannot be null or whitespace", nameof(requestId));

            var allLogs = await GetAllAsync();
            return allLogs.Where(log => log.RequestId == requestId);
        }

        /// <summary>
        /// Gets the ID of the AuditLog entity.
        /// </summary>
        /// <param name="entity">The AuditLog entity.</param>
        /// <returns>The entity's ID.</returns>
        protected override string GetEntityId(AuditLog entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // For audit logs, we use a combination of request ID and timestamp to ensure uniqueness
            return $"{entity.RequestId}-{entity.Timestamp.Ticks}";
        }
    }
}
