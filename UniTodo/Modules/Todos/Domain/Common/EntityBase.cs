namespace UniTodo.Modules.Todos.Domain.Common
{
    public class EntityBase<TEntityIdType>
    {
public DateTimeOffset CreatedAt { get; protected  set; }
        public TEntityIdType Id { get; private set; }
public DateTimeOffset UpdatedAt { get; protected  set; }

        protected EntityBase() { }

        protected EntityBase( TEntityIdType id = default )
        {
        Id = id;
        }
    }
}