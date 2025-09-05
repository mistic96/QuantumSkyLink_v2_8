using LiquidStorageCloud.Core.Database;

namespace LiquidStorageCloud.DataManagement.Core.CQRS.Commands
{
    public class CreateEntityCommand<T> : BaseCommand<T> where T : class, ISurrealEntity
    {
        public T Entity { get; }

        public CreateEntityCommand(T entity)
        {
            Entity = entity;
        }
    }

    public class UpdateEntityCommand<T> : BaseCommand<T> where T : class, ISurrealEntity
    {
        public T Entity { get; }

        public UpdateEntityCommand(T entity)
        {
            Entity = entity;
        }
    }

    public class DeleteEntityCommand<T> : BaseCommand<bool> where T : class, ISurrealEntity
    {
        public string Id { get; }
        public string TableName { get; }

        public DeleteEntityCommand(string id, string tableName)
        {
            Id = id;
            TableName = tableName;
        }

        public DeleteEntityCommand(T entity)
        {
            Id = entity.Id;
            TableName = entity.TableName;
        }
    }

    public class SetSolidStateCommand<T> : BaseCommand<bool> where T : class, ISurrealEntity
    {
        public string Id { get; }
        public string TableName { get; }
        public bool SolidState { get; }

        public SetSolidStateCommand(string id, string tableName, bool solidState)
        {
            Id = id;
            TableName = tableName;
            SolidState = solidState;
        }

        public SetSolidStateCommand(T entity, bool solidState)
        {
            Id = entity.Id;
            TableName = entity.TableName;
            SolidState = solidState;
        }
    }
}
