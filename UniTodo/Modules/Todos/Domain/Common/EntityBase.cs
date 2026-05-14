namespace UniTodo.Modules.Todos.Domain.Common
{
    internal abstract class EntityBase<TEntityId>
    {
        internal DateTimeOffset CreatedAt { get; private protected set; }
        internal TEntityId Id { get; private set; }
        internal DateTimeOffset UpdatedAt { get; private protected set; }

        protected EntityBase() { }

    }       
internal abstract class EntityBase : EntityBase<int>
{
    }
}