using FluentAssertions;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Domain.ValueObjects
{
    public class TodoItemNotesTests
    {
        [Fact]
        public void Constructor_WithValidValue_ShouldInitializeCorrectly()
        {
            // Arrange
            var value = "Test Notes";

            // Act
            var notes = new TodoItemNotes(value);

            // Assert
            notes.Value.Should().Be(value);
        }

        [Fact]
        public void Constructor_WithValueTooLong_ShouldThrowArgumentException()
        {
            // Arrange
            var longValue = new string('a', Constants.NotesMaxLength + 1);

            // Act
            Action act = () => new TodoItemNotes(longValue);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage($"*Notes couldn't be longer than {Constants.NotesMaxLength}*");
        }

        [Fact]
        public void Equality_WithSameValue_ShouldBeTrue()
        {
            // Arrange
            var notes1 = new TodoItemNotes("Value");
            var notes2 = new TodoItemNotes("Value");

            // Assert
            notes1.Should().Be(notes2);
            (notes1 == notes2).Should().BeTrue();
        }

        [Fact]
        public void Equality_WithDifferentValue_ShouldBeFalse()
        {
            // Arrange
            var notes1 = new TodoItemNotes("Value 1");
            var notes2 = new TodoItemNotes("Value 2");

            // Assert
            notes1.Should().NotBe(notes2);
            (notes1 == notes2).Should().BeFalse();
        }
    }
}
