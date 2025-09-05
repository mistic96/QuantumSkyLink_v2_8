namespace LiquidStorageCloud.DataManagement.Core.CQRS
{
    /// <summary>
    /// Represents a handler for commands that don't return a result
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle</typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Handles the specified command
        /// </summary>
        /// <param name="command">The command to handle</param>
        /// <param name="cancellationToken">The cancellation token</param>
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a handler for commands that return a result
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle</typeparam>
    /// <typeparam name="TResult">The type of result returned by the command</typeparam>
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Handles the specified command and returns a result
        /// </summary>
        /// <param name="command">The command to handle</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of handling the command</returns>
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
