namespace LiquidStorageCloud.DataManagement.Core.CQRS
{
    /// <summary>
    /// Base class for commands that don't return a result
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        protected BaseCommand()
        {
            CommandId = Guid.NewGuid();
            Timestamp = DateTimeOffset.UtcNow;
        }

        /// <inheritdoc/>
        public Guid CommandId { get; }

        /// <inheritdoc/>
        public DateTimeOffset Timestamp { get; }
    }

    /// <summary>
    /// Base class for commands that return a result
    /// </summary>
    /// <typeparam name="TResult">The type of the result</typeparam>
    public abstract class BaseCommand<TResult> : ICommand<TResult>
    {
        protected BaseCommand()
        {
            CommandId = Guid.NewGuid();
            Timestamp = DateTimeOffset.UtcNow;
        }

        /// <inheritdoc/>
        public Guid CommandId { get; }

        /// <inheritdoc/>
        public DateTimeOffset Timestamp { get; }
    }
}
