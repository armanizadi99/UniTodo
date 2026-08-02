using FluentAssertions;
using UniTodo.Modules.Todos.Api.Attributes;

namespace UniTodo.Tests.TodoModuleTests.Api
{
    public class CreatedAtRouteResultAttributeTests
    {
        [Fact]
        public void Constructor_ShouldSetRouteName()
        {
            // Act
            var attribute = new CreatedAtRouteResultAttribute("GetRunById");

            // Assert
            attribute.RouteName.Should().Be("GetRunById");
        }

        [Fact]
        public void RouteValueName_ShouldDefaultToId()
        {
            // Act
            var attribute = new CreatedAtRouteResultAttribute("GetRunById");

            // Assert
            attribute.RouteValueName.Should().Be("id");
        }

        [Fact]
        public void RouteValueName_ShouldBeSettable()
        {
            // Arrange
            var attribute = new CreatedAtRouteResultAttribute("GetRunById");

            // Act
            attribute.RouteValueName = "runId";

            // Assert
            attribute.RouteValueName.Should().Be("runId");
        }
    }
}
