namespace LiquidStorageCloud.DataManagement.Core.CQRS
{
    /// <summary>
    /// Represents a command in the CQRS pattern that doesn't return a result
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the unique identifier for the command
        /// </summary>
        Guid CommandId { get; }

        /// <summary>
        /// Gets the timestamp when the command was created
        /// </summary>
        DateTimeOffset Timestamp { get; }
    }

    /// <summary>
    /// Represents a command in the CQRS pattern that returns a result
    /// </summary>
    /// <typeparam name="TResult">The type of the command result</typeparam>
    public interface ICommand<TResult>
    {
        /// <summary>
        /// Gets the unique identifier for the command
        /// </summary>
        Guid CommandId { get; }

        /// <summary>
        /// Gets the timestamp when the command was created
        /// </summary>
        DateTimeOffset Timestamp { get; }
    }
}
