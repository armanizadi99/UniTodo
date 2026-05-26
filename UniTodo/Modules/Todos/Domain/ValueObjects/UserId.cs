using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    internal readonly record struct UserId : IStronglyTypedId<Guid>
    {
        private readonly Guid _value;
        Guid IStronglyTypedId<Guid>.Value { get { return _value; } init { _value = value; } }
        public Guid Value { get { return _value; } }

        internal UserId(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainArgumentException(nameof(UserId), "UserId couldn't be empty.");
            _value = value;
        }
    }
}
