namespace UniTodo.Modules.Todos.Domain.Common
{
    public interface IStronglyTypedId<TId>
    {
public TId Value { get; init; } 
    }
}
