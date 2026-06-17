using FluentAssertions;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Domain.ValueObjects
{
    public class TodoItemDescriptionTests
    {
        [Fact]
        public void Constructor_WithValidValue_ShouldInitializeCorrectly()
        {
            // Arrange
            var value = "Test Description";

            // Act
            var description = new TodoItemDescription(value);

            // Assert
            description.Value.Should().Be(value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_WithNullOrEmptyValue_ShouldThrowArgumentException(string? value)
        {
            // Act
            Action act = () => new TodoItemDescription(value!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Description couldn't be null or empty.*");
        }

        [Fact]
        public void Constructor_WithValueTooLong_ShouldThrowArgumentException()
        {
            // Arrange
            var longValue = new string('a', Constants.DescriptionMaxLength + 1);

            // Act
            Action act = () => new TodoItemDescription(longValue);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage($"*Description couldn't be longer than {Constants.DescriptionMaxLength}*");
        }

        [Fact]
        public void Equality_WithSameValue_ShouldBeTrue()
        {
            // Arrange
            var desc1 = new TodoItemDescription("Value");
            var desc2 = new TodoItemDescription("Value");

            // Assert
            desc1.Should().Be(desc2);
            (desc1 == desc2).Should().BeTrue();
        }

        [Fact]
        public void Equality_WithDifferentValue_ShouldBeFalse()
        {
            // Arrange
            var desc1 = new TodoItemDescription("Value 1");
            var desc2 = new TodoItemDescription("Value 2");

            // Assert
            desc1.Should().NotBe(desc2);
            (desc1 == desc2).Should().BeFalse();
        }
    }
}
