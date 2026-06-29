using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;
using FluentAssertions;

namespace UniTodo.Tests.TodoModuleTests.Domain
{
    public class RunIterationTests
    {
        [Fact]
        public void Constructor_ShouldStartWithNoItems()
        {
            // Act
            var iteration = new RunIteration();

            // Assert
            iteration.RunItems.Should().BeEmpty();
        }

        [Fact]
        public void AddItem_ShouldAddItemToCollection()
        {
            // Arrange
            var iteration = new RunIteration();
            var item = new RunItem(new TodoItemDescription("Item 1"));

            // Act
            iteration.AddItem(item);

            // Assert
            iteration.RunItems.Should().ContainSingle().Which.Should().Be(item);
        }

        [Fact]
        public void RemoveItem_ShouldRemoveItemFromCollection()
        {
            // Arrange
            var iteration = new RunIteration();
            var item = new RunItem(new TodoItemDescription("Item 1"));
            iteration.AddItem(item);

            // Act
            iteration.RemoveItem(item);

            // Assert
            iteration.RunItems.Should().BeEmpty();
        }
    }
}
