namespace UniTodo.Modules.Todos.Domain.Common
{
    internal interface IStronglyTypedId<TId>
    {
internal TId Value { get; init; } 
    }
}
