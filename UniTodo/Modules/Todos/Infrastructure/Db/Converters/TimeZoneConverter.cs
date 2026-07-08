using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeZoneConverter;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Converters
{
    public class TimeZoneConverter : ValueConverter<TimeZoneInfo, string>
    {
        public TimeZoneConverter() : base(
            tz => tz.Id,
            s => TZConvert.GetTimeZoneInfo(s)
        )
        { }
    }
}
