using FluentAssertions;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Domain.ValueObjects
{
    public class UserIdTests
    {
        [Fact]
        public void Constructor_WithValidGuid_ShouldInitializeCorrectly()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var userId = new UserId(guid);

            // Assert
            userId.Value.Should().Be(guid);
        }

        [Fact]
        public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
        {
            // Arrange
            var guid = Guid.Empty;

            // Act
            Action act = () => new UserId(guid);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*UserId couldn't be empty.*");
        }

        [Fact]
        public void Equality_WithSameGuid_ShouldBeTrue()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var userId1 = new UserId(guid);
            var userId2 = new UserId(guid);

            // Act
            var equalsResult = userId1.Equals(userId2);
            var operatorResult = userId1 == userId2;

            // Assert
            equalsResult.Should().BeTrue();
            operatorResult.Should().BeTrue();
        }

        [Fact]
        public void Equality_WithDifferentGuid_ShouldBeFalse()
        {
            // Arrange
            var userId1 = new UserId(Guid.NewGuid());
            var userId2 = new UserId(Guid.NewGuid());

            // Act
            var equalsResult = userId1.Equals(userId2);
            var operatorResult = userId1 == userId2;

            // Assert
            equalsResult.Should().BeFalse();
            operatorResult.Should().BeFalse();
        }
    }
}
