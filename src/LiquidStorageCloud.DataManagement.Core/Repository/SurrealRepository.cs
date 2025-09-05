using System.Linq.Expressions;
using LiquidStorageCloud.Core.Database;
using SurrealDb.Net;

namespace LiquidStorageCloud.DataManagement.Core.Repository
{
    /// <summary>
    /// Repository implementation for SurrealDB operations
    /// </summary>
    /// <typeparam name="T">The type of entity to manage</typeparam>
    public class SurrealRepository<T> : ISurrealRepository<T> where T : class, ISurrealEntity
    {
        private readonly ISurrealDbClient _client;
        private readonly string _tableName;

        public SurrealRepository(ISurrealDbClient client, T entityTemplate)
        {
            _client = client;
            _tableName = entityTemplate.TableName;
        }

        /// <inheritdoc/>
        public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                entity.LastModified = DateTimeOffset.UtcNow;
                entity.SolidState = false;

                var response = await _client.Create(_tableName, entity, cancellationToken);
                
                try
                {
                    var created = response;
                    if (created == null)
                        throw new Exception("No result returned");
                    return created;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to process response: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create entity: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                entity.LastModified = DateTimeOffset.UtcNow;
                
                var response = await _client.Update(_tableName, entity);
                
                try
                {
                    var updated = response.FirstOrDefault();
                    if (updated == null)
                        throw new Exception("No result returned");
                    return updated;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to process response: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update entity: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.Delete<T>(_tableName, f => f.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete entity: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var results = await _client.Select<T>(_tableName, f => f.Id == id);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get entity: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> ListAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true,
            int skip = 0,
            int take = int.MaxValue,
            bool onlySolidState = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _client.Select<T>(_tableName, cancellationToken);

                if (onlySolidState)
                {
                    result = result.Where(x => x.SolidState);
                }

                if (orderBy != null)
                {
                    var orderedResult = ascending ? 
                        result.OrderBy(orderBy.Compile()) : 
                        result.OrderByDescending(orderBy.Compile());
                    result = orderedResult;
                }

                if (skip > 0)
                {
                    result = result.Skip(skip);
                }
                if (take < int.MaxValue)
                {
                    result = result.Take(take);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to list entities: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TResult>> QueryAsync<TResult>(
            string query,
            IDictionary<string, object>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.Query($"{query}", cancellationToken);
                
                try
                {
                    return response.GetValue<IEnumerable<TResult>>(0);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to process response: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute query: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task SetSolidStateAsync(string id, bool solidState, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await GetByIdAsync(id, cancellationToken);
                if (entity == null)
                    return;

                entity.SolidState = solidState;
                entity.LastModified = DateTimeOffset.UtcNow;

                await UpdateAsync(entity, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set solid state: {ex.Message}", ex);
            }
        }

        private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name.ToLower();
            }

            throw new ArgumentException("Expression must be a member access", nameof(expression));
        }
    }
}
