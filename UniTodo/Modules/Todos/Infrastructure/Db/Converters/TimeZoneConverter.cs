using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Converters
{
    public class TimeZoneConverter : ValueConverter<TimeZoneInfo, string>
    {
        public TimeZoneConverter() : base(
            tz => tz.Id,
            s => TimeZoneInfo.FindSystemTimeZoneById(s)
        )
        { }
    }
}
