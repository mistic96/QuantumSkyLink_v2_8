using LiquidStorageCloud.Core.Database;
using LiquidStorageCloud.DataManagement.Core.CQRS.Commands;
using LiquidStorageCloud.Services.EventPublishing.Publishers;

namespace LiquidStorageCloud.DataManagement.Core.CQRS.Handlers
{
    /// <summary>
    /// Decorator that adds event publishing capabilities to entity command handlers
    /// </summary>
    public class EventPublishingDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
        where TResult : class, ISurrealEntity
    {
        private readonly ICommandHandler<TCommand, TResult> _decorated;
        private readonly IEventPublisherFactory? _publisherFactory;

        public EventPublishingDecorator(
            ICommandHandler<TCommand, TResult> decorated,
            IEventPublisherFactory? publisherFactory = null)
        {
            _decorated = decorated;
            _publisherFactory = publisherFactory;
        }

        public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
        {
            var result = await _decorated.HandleAsync(command, cancellationToken);

            if (_publisherFactory != null)
            {
                var publisher = _publisherFactory.CreatePublisher<TResult>();

                // Publish appropriate event based on command type
                if (command is CreateEntityCommand<TResult>)
                {
                    await publisher.PublishCreatedAsync(result);
                }
                else if (command is UpdateEntityCommand<TResult>)
                {
                    await publisher.PublishUpdatedAsync(result);
                }
                else if (command is DeleteEntityCommand<TResult> deleteCommand)
                {
                    var entity = result as ISurrealEntity;
                    if (entity != null)
                    {
                        await publisher.PublishDeletedAsync(deleteCommand.Id, entity.TableName, entity.Namespace);
                    }
                }
                else if (command is SetSolidStateCommand<TResult> solidStateCommand)
                {
                    var entity = result as ISurrealEntity;
                    if (entity != null)
                    {
                        await publisher.PublishSolidStateChangedAsync(
                            entity.Id, 
                            entity.TableName, 
                            entity.Namespace, 
                            solidStateCommand.SolidState);
                    }
                }
            }

            return result;
        }
    }
}
