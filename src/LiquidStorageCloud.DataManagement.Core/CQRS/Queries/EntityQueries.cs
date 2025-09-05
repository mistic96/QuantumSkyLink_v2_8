using System.Linq.Expressions;
using LiquidStorageCloud.Core.Database;

namespace LiquidStorageCloud.DataManagement.Core.CQRS.Queries
{
    public class GetEntityByIdQuery<T> : BaseQuery<T?> where T : class, ISurrealEntity
    {
        public string Id { get; }
        public string TableName { get; }

        public GetEntityByIdQuery(string id, string tableName)
        {
            Id = id;
            TableName = tableName;
        }

        public GetEntityByIdQuery(T entity)
        {
            Id = entity.Id;
            TableName = entity.TableName;
        }
    }

    public class ListEntitiesQuery<T> : BaseQuery<IEnumerable<T>> where T : class, ISurrealEntity
    {
        public string TableName { get; }
        public Expression<Func<T, bool>>? Filter { get; }
        public Expression<Func<T, object>>? OrderBy { get; }
        public bool Ascending { get; }
        public int Skip { get; }
        public int Take { get; }
        public bool OnlySolidState { get; }

        public ListEntitiesQuery(
            string tableName,
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>? orderBy = null,
            bool ascending = true,
            int skip = 0,
            int take = int.MaxValue,
            bool onlySolidState = false)
        {
            TableName = tableName;
            Filter = filter;
            OrderBy = orderBy;
            Ascending = ascending;
            Skip = skip;
            Take = take;
            OnlySolidState = onlySolidState;
        }
    }

    public class QueryEntitiesQuery<T, TResult> : BaseQuery<IEnumerable<TResult>> where T : class, ISurrealEntity
    {
        public string Query { get; }
        public IDictionary<string, object>? Parameters { get; }

        public QueryEntitiesQuery(string query, IDictionary<string, object>? parameters = null)
        {
            Query = query;
            Parameters = parameters;
        }
    }
}
