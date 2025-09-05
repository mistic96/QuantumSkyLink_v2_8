using Microsoft.Extensions.DependencyInjection;

namespace LiquidStorageCloud.DataManagement.Core.CQRS.Factory
{
    /// <summary>
    /// Factory implementation for creating CQRS handlers using dependency injection
    /// </summary>
    public class CqrsHandlerFactory : ICqrsHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CqrsHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public ICommandHandler<TCommand, TResult> GetCommandHandler<TCommand, TResult>() where TCommand : ICommand<TResult>
        {
            var handler = _serviceProvider.GetService<ICommandHandler<TCommand, TResult>>();
            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for command type {typeof(TCommand).Name} with result type {typeof(TResult).Name}");
            }
            return handler;
        }

        /// <inheritdoc/>
        public ICommandHandler<TCommand> GetCommandHandler<TCommand>() where TCommand : ICommand
        {
            var handler = _serviceProvider.GetService<ICommandHandler<TCommand>>();
            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for command type {typeof(TCommand).Name}");
            }
            return handler;
        }

        /// <inheritdoc/>
        public IQueryHandler<TQuery, TResult> GetQueryHandler<TQuery, TResult>() where TQuery : IQuery<TResult>
        {
            var handler = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"No handler registered for query type {typeof(TQuery).Name} with result type {typeof(TResult).Name}");
            }
            return handler;
        }
    }
}
