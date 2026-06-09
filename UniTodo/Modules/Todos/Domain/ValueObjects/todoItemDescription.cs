using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    public record TodoItemDescription
    {
        public string Value { get; init; }

        public TodoItemDescription(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(nameof(TodoItemDescription), "Description couldn't be null or empty.");
            if (value.Length > Constants.DescriptionMaxLength)
                throw new ArgumentException(nameof(TodoItemDescription), $"Description couldn't be longer than {Constants.DescriptionMaxLength} characters");
            Value = value;
        }
    }
}
