using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    public record TodoItemNotes
    {
        public string Value { get; init; }

        public TodoItemNotes(string value)
        {
            if (value.Length > Constants.NotesMaxLength)
                throw new ArgumentException(nameof(TodoItemNotes), $"Notes couldn't be longer than {Constants.NotesMaxLength} characters");

            Value = value;
        }
    }
}
