using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    internal record TodoItemDescription
    {
        internal string Value { get; init; }

        internal TodoItemDescription(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new DomainArgumentException(nameof(TodoItemDescription), "Description couldn't be null or empty.");
            if (value.Length > Constants.DescriptionMaxLength)
                throw new DomainArgumentException(nameof(TodoItemDescription), $"Description couldn't be longer than {Constants.DescriptionMaxLength} characters");
            Value = value;
        }
    }
}
