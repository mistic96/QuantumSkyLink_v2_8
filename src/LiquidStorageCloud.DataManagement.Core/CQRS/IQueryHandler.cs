namespace LiquidStorageCloud.DataManagement.Core.CQRS
{
    /// <summary>
    /// Represents a handler for queries
    /// </summary>
    /// <typeparam name="TQuery">The type of query to handle</typeparam>
    /// <typeparam name="TResult">The type of result returned by the query</typeparam>
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Handles the specified query and returns a result
        /// </summary>
        /// <param name="query">The query to handle</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of handling the query</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
