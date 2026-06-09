using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Converters
{
    public class UserIdConverter : ValueConverter<UserId, Guid>
    {
        public UserIdConverter() : base(
userId => userId.Value,
value => new UserId(value)
        )
        { }
    }
}
