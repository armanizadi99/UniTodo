using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    public readonly record struct TodoItemTemplateId : IStronglyTypedId<int>
{
public int Value { get; init; }

public TodoItemTemplateId(int value)
{
        IdHelper.GuardAgainstInvalid(value, nameof(TodoItemTemplateId));
        Value = value;
        }
    }
}
