using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.Validation;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Application.Validation
{
    public class ValidTimeZoneIdAttributeTests
    {
        private readonly ValidTimeZoneIdAttribute _attribute = new();
        private readonly ValidationContext _context = new(new object());

        [Theory]
        [InlineData("UTC")]
        [InlineData("utc")]
        [InlineData("Tokyo Standard Time")]
        [InlineData("India Standard Time")]
        [InlineData("Eastern Standard Time")]
        [InlineData("GMT Standard Time")]
        [InlineData("New Zealand Standard Time")]
        public void IsValid_ValidTimeZone_ShouldReturnSuccess(string timeZoneId)
        {
            // Act
            var result = _attribute.GetValidationResult(timeZoneId, _context);

            // Assert
            result.Should().Be(ValidationResult.Success);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("FakeZone")]
        [InlineData("Invalid/Timezone/Id")]
        public void IsValid_InvalidTimeZone_ShouldReturnError(object? value)
        {
            // Act
            var result = _attribute.GetValidationResult(value, _context);

            // Assert
            result.Should().NotBe(ValidationResult.Success);
            result!.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        }
    }
}
