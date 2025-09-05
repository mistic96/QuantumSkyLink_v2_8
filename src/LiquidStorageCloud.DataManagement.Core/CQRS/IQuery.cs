namespace LiquidStorageCloud.DataManagement.Core.CQRS
{
    /// <summary>
    /// Represents a query in the CQRS pattern
    /// </summary>
    /// <typeparam name="TResult">The type of the query result</typeparam>
    public interface IQuery<TResult>
    {
        /// <summary>
        /// Gets the unique identifier for the query
        /// </summary>
        Guid QueryId { get; }

        /// <summary>
        /// Gets the timestamp when the query was created
        /// </summary>
        DateTimeOffset Timestamp { get; }
    }
}
