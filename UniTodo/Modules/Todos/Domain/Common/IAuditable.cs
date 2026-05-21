namespace UniTodo.Modules.Todos.Domain.Common
{
    public interface IAuditable
    {
DateTimeOffset CreatedAt { get; }
DateTimeOffset? UpdatedAt { get; }
    }
}
