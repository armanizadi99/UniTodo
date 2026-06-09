using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    public readonly record struct UserId : IStronglyTypedId<Guid>
    {
        public Guid Value { get; init; }

        public UserId(Guid value)
        {
            if (value == Guid.Empty)
                throw new ArgumentException(nameof(UserId), "UserId couldn't be empty.");
            Value = value;
        }
    }
}
