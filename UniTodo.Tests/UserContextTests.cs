using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Infrastructure;
using Xunit;

namespace UniTodo.Tests
{
    public class UserContextTests
    {
        [Fact]
        public void UserId_ShouldReturnUserIdFromClaims()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            context.User = new ClaimsPrincipal(identity);
            httpContextAccessor.HttpContext.Returns(context);

            var userContext = new UserContext(httpContextAccessor);

            // Act
            var result = ((UniTodo.Modules.Todos.Application.Interfaces.IUserContext)userContext).UserId;

            // Assert
            result.Value.Should().Be(userId);
        }

        [Fact]
        public void UserId_ShouldThrowException_WhenClaimIsMissing()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Returns(context);

            // Act
            var act = () => new UserContext(httpContextAccessor);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
    }
}
