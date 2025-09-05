namespace LiquidStorageCloud.DataManagement.Core.CQRS
{
    /// <summary>
    /// Base class for queries
    /// </summary>
    /// <typeparam name="TResult">The type of the query result</typeparam>
    public abstract class BaseQuery<TResult> : IQuery<TResult>
    {
        protected BaseQuery()
        {
            QueryId = Guid.NewGuid();
            Timestamp = DateTimeOffset.UtcNow;
        }

        /// <inheritdoc/>
        public Guid QueryId { get; }

        /// <inheritdoc/>
        public DateTimeOffset Timestamp { get; }
    }
}
