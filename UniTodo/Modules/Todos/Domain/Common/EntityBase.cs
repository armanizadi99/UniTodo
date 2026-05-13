namespace UniTodo.Modules.Todos.Domain.Common
{
    public abstract class EntityBase<TEntityId>
    {
        public DateTimeOffset CreatedAt { get; protected set; }
        public TEntityId Id { get; private set; }
        public DateTimeOffset UpdatedAt { get; protected set; }

        protected EntityBase() { }

    }       
public abstract class EntityBase : EntityBase<int>
{
    }
}