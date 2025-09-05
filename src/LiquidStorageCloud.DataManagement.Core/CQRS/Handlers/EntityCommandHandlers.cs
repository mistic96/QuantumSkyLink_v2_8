using LiquidStorageCloud.Core.Database;
using LiquidStorageCloud.DataManagement.Core.CQRS.Commands;
using LiquidStorageCloud.DataManagement.Core.Repository;

namespace LiquidStorageCloud.DataManagement.Core.CQRS.Handlers
{
    public class CreateEntityHandler<T> : ICommandHandler<CreateEntityCommand<T>, T> 
        where T : class, ISurrealEntity
    {
        private readonly ISurrealRepository<T> _repository;

        public CreateEntityHandler(ISurrealRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T> HandleAsync(CreateEntityCommand<T> command, CancellationToken cancellationToken = default)
        {
            return await _repository.CreateAsync(command.Entity, cancellationToken);
        }
    }

    public class UpdateEntityHandler<T> : ICommandHandler<UpdateEntityCommand<T>, T>
        where T : class, ISurrealEntity
    {
        private readonly ISurrealRepository<T> _repository;

        public UpdateEntityHandler(ISurrealRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<T> HandleAsync(UpdateEntityCommand<T> command, CancellationToken cancellationToken = default)
        {
            return await _repository.UpdateAsync(command.Entity, cancellationToken);
        }
    }

    public class DeleteEntityHandler<T> : ICommandHandler<DeleteEntityCommand<T>, bool>
        where T : class, ISurrealEntity
    {
        private readonly ISurrealRepository<T> _repository;

        public DeleteEntityHandler(ISurrealRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<bool> HandleAsync(DeleteEntityCommand<T> command, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(command.Id, cancellationToken);
            return true;
        }
    }

    public class SetSolidStateHandler<T> : ICommandHandler<SetSolidStateCommand<T>, bool>
        where T : class, ISurrealEntity
    {
        private readonly ISurrealRepository<T> _repository;

        public SetSolidStateHandler(ISurrealRepository<T> repository)
        {
            _repository = repository;
        }

        public async Task<bool> HandleAsync(SetSolidStateCommand<T> command, CancellationToken cancellationToken = default)
        {
            await _repository.SetSolidStateAsync(command.Id, command.SolidState, cancellationToken);
            return true;
        }
    }
}
