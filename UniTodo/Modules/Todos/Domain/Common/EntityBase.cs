namespace UniTodo.Modules.Todos.Domain.Common
{
    public class EntityBase
    {
public DateTimeOffset CreatedAt { get; protected  set; }
        public int Id { get; private set; }
public DateTimeOffset UpdatedAt { get; protected  set; }

        protected EntityBase() { }

        protected EntityBase( int id = default )
        {
        Id = id;
        }
    }
}