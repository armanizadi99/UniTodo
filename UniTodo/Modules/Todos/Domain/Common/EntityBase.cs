namespace UniTodo.Modules.Todos.Domain.Common
{
    public abstract class EntityBase<TEntityId> : IAuditable
    {
        public DateTimeOffset CreatedAt { get; private protected set; }
        public TEntityId Id { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private protected set; }

        protected EntityBase() { }

    }       
public abstract class EntityBase : EntityBase<int>
{
    }
}