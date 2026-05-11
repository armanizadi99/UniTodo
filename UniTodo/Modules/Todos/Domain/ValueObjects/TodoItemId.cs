using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    public readonly record struct TodoItemId : IStronglyTypedId<int>
    {
        public int Value { get; init; }

        public TodoItemId( int value )
        {
        IdHelper.GuardAgainstInvalid(value, nameof(TodoItemId));
        Value = value;
        }
    }
}