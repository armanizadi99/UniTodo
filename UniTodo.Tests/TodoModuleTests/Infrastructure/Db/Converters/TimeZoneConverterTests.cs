using FluentAssertions;
using TimeZoneConverter;
using TZConverter = UniTodo.Modules.Todos.Infrastructure.Db.Converters.TimeZoneConverter;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db
{
    public class TimeZoneConverterTests
    {
        private readonly TZConverter _converter = new();

        [Fact]
        public void ConvertToProvider_ShouldReturnTimeZoneId()
        {
            // Act
            var stored = _converter.ConvertToProvider(TimeZoneInfo.Utc);

            // Assert
            stored.Should().Be("UTC");
        }

        [Fact]
        public void ConvertFromProvider_ShouldReturnUtcForUtcId()
        {
            // Act
            var timeZone = _converter.ConvertFromProvider("UTC") as TimeZoneInfo;

            // Assert
            timeZone!.Id.Should().Be("UTC");
        }

        [Fact]
        public void RoundTrip_ShouldPreserveTimeZone()
        {
            // Arrange
            var original = TZConvert.GetTimeZoneInfo("America/New_York");

            // Act
            var stored = (string?)_converter.ConvertToProvider(original);
            var restored = (TimeZoneInfo?)_converter.ConvertFromProvider(stored!);

            // Assert
            restored.Should().NotBeNull();
            restored!.Id.Should().Be(original.Id);
            restored.GetUtcOffset(DateTime.UtcNow).Should().Be(original.GetUtcOffset(DateTime.UtcNow));
        }
    }
}
