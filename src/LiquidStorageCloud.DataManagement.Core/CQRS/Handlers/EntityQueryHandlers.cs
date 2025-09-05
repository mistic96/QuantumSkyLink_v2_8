using LiquidStorageCloud.Core.Database;
using LiquidStorageCloud.DataManagement.Core.CQRS.Queries;
using LiquidStorageCloud.DataManagement.Core.Repository;

namespace LiquidStorageCloud.DataManagement.Core.CQRS.Handlers
{
    public class GetEntityByIdHandler<T> : IQueryHandler<GetEntityByIdQuery<T>, T?>
        where T : class, ISurrealEntity
    {
        private readonly ISurrealRepository<T> _repository;

        public GetEntityByIdHandler(ISurrealRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T?> HandleAsync(GetEntityByIdQuery<T> query, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(query.Id, cancellationToken);
        }
    }

    public class ListEntitiesHandler<T> : IQueryHandler<ListEntitiesQuery<T>, IEnumerable<T>>
        where T : class, ISurrealEntity
    {
        private readonly ISurrealRepository<T> _repository;

        public ListEntitiesHandler(ISurrealRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<T>> HandleAsync(ListEntitiesQuery<T> query, CancellationToken cancellationToken = default)
        {
            return await _repository.ListAsync(
                query.Filter,
                query.OrderBy,
                query.Ascending,
                query.Skip,
                query.Take,
                query.OnlySolidState,
                cancellationToken);
        }
    }

    public class QueryEntitiesHandler<T, TResult> : IQueryHandler<QueryEntitiesQuery<T, TResult>, IEnumerable<TResult>>
        where T : class, ISurrealEntity
    {
        private readonly ISurrealRepository<T> _repository;

        public QueryEntitiesHandler(ISurrealRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TResult>> HandleAsync(QueryEntitiesQuery<T, TResult> query, CancellationToken cancellationToken = default)
        {
            return await _repository.QueryAsync<TResult>(query.Query, query.Parameters, cancellationToken);
        }
    }
}
