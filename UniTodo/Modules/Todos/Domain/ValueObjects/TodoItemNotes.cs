using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    internal record TodoItemNotes
    {
        internal string Value { get; init; }

        internal TodoItemNotes(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new DomainArgumentException(nameof(TodoItemNotes), "Notes couldn't be empty or whitespace.");
            if (value.Length > Constants.NotesMaxLength)
                throw new DomainArgumentException(nameof(TodoItemNotes), $"Notes couldn't be longer than {Constants.NotesMaxLength} characters");

            Value = value;
        }
    }
}
