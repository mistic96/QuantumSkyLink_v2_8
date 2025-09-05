namespace LiquidStorageCloud.DataManagement.Core.CQRS.Factory
{
    /// <summary>
    /// Factory interface for creating CQRS handlers
    /// </summary>
    public interface ICqrsHandlerFactory
    {
        /// <summary>
        /// Gets a command handler for the specified command and result type
        /// </summary>
        ICommandHandler<TCommand, TResult> GetCommandHandler<TCommand, TResult>() where TCommand : ICommand<TResult>;

        /// <summary>
        /// Gets a command handler for the specified command type
        /// </summary>
        ICommandHandler<TCommand> GetCommandHandler<TCommand>() where TCommand : ICommand;

        /// <summary>
        /// Gets a query handler for the specified query and result type
        /// </summary>
        IQueryHandler<TQuery, TResult> GetQueryHandler<TQuery, TResult>() where TQuery : IQuery<TResult>;
    }
}
