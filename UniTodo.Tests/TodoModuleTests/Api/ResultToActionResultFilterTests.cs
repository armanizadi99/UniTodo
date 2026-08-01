using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using UniTodo.Modules.Todos.Api.Attributes;
using UniTodo.Modules.Todos.Api.Filters;
using UniTodo.Modules.Todos.Domain.Common;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Api
{
    public class ResultToActionResultFilterTests
    {
        private readonly ResultToActionResultFilter _filter;

        public ResultToActionResultFilterTests()
        {
            _filter = new ResultToActionResultFilter();
        }

        #region Helpers
        private sealed record TestDto(int Id, string Name);

        private static async Task<IActionResult> ExecuteAsync(
            ResultToActionResultFilter filter,
            IActionResult actionResult,
            params object[] endpointMetadata)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor
                {
                    EndpointMetadata = endpointMetadata.ToList()
                });

            var executingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller: new object());

            var executedContext = new ActionExecutedContext(
                executingContext,
                executingContext.Filters,
                executingContext.Controller)
            {
                Result = actionResult
            };

            await filter.OnActionExecutionAsync(
                executingContext,
                () => Task.FromResult(executedContext));

            return executedContext.Result;
        }
        #endregion

        #region Success mapping
        [Fact]
        public async Task OnActionExecutionAsync_WhenSuccessfulGenericResult_ShouldMapToOkObjectResult()
        {
            // Arrange
            var dto = new TestDto(1, "Test");

            // Act
            var result = await ExecuteAsync(_filter, new ObjectResult(Result<TestDto>.Success(dto)));

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result).Value.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenSuccessfulUnitResult_ShouldMapToNoContentResult()
        {
            // Act
            var result = await ExecuteAsync(_filter, new ObjectResult(Result.Success()));

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenSuccessfulGenericResultWithCreatedAtRouteAttribute_ShouldMapToCreatedAtRouteResult()
        {
            // Arrange
            var dto = new TestDto(5, "Test");
            var attribute = new CreatedAtRouteResultAttribute("GetRunById") { RouteValueName = "runId" };

            // Act
            var result = await ExecuteAsync(_filter, new ObjectResult(Result<TestDto>.Success(dto)), attribute);

            // Assert
            var createdAtRoute = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
            createdAtRoute.StatusCode.Should().Be(StatusCodes.Status201Created);
            createdAtRoute.RouteName.Should().Be("GetRunById");
            createdAtRoute.RouteValues!["runId"].Should().Be(5);
            createdAtRoute.Value.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenSuccessfulGenericResultWithCreatedAtRouteAttributeAndDefaultRouteValueName_ShouldUseIdRouteValue()
        {
            // Arrange
            var dto = new TestDto(9, "Test");
            var attribute = new CreatedAtRouteResultAttribute("GetTodoListTemplateById");

            // Act
            var result = await ExecuteAsync(_filter, new ObjectResult(Result<TestDto>.Success(dto)), attribute);

            // Assert
            var createdAtRoute = result.Should().BeOfType<CreatedAtRouteResult>().Subject;
            createdAtRoute.RouteValues!["id"].Should().Be(9);
        }
        #endregion

        #region Failure mapping
        [Theory]
        [InlineData(DomainErrorCodes.EntityNotFound, StatusCodes.Status404NotFound)]
        [InlineData(DomainErrorCodes.InvalidOperation, StatusCodes.Status400BadRequest)]
        [InlineData(DomainErrorCodes.DuplicateEntities, StatusCodes.Status409Conflict)]
        public async Task OnActionExecutionAsync_WhenFailedGenericResult_ShouldMapErrorToProblemDetailsResult(DomainErrorCodes errorCode, int expectedStatus)
        {
            // Arrange
            var error = new DomainError(errorCode, "Test message");

            // Act
            var result = await ExecuteAsync(_filter, new ObjectResult(Result<TestDto>.Failure(error)));

            // Assert
            var objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
            var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problemDetails.Status.Should().Be(expectedStatus);
            problemDetails.Detail.Should().Be("Test message");
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenFailedWithNotAuthorized_ShouldMapToForbidResult()
        {
            // Act
            var result = await ExecuteAsync(_filter, new ObjectResult(Result<TestDto>.Failure(DomainError.NotAuthorized())));

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenFailedUnitResult_ShouldMapErrorToHttpResult()
        {
            // Arrange
            var error = DomainError.EntityNotFound(nameof(TestDto), 42);

            // Act
            var result = await ExecuteAsync(_filter, new ObjectResult(Result.Failure(error)));

            // Assert
            var objectResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
            problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        }
        #endregion

        #region Pass-through
        [Fact]
        public async Task OnActionExecutionAsync_WhenResultIsNotResult_ShouldPassThroughUnchanged()
        {
            // Arrange
            var originalResult = new BadRequestObjectResult("validation error");

            // Act
            var result = await ExecuteAsync(_filter, originalResult);

            // Assert
            result.Should().BeSameAs(originalResult);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenObjectResultValueIsNotResult_ShouldPassThroughUnchanged()
        {
            // Arrange
            var originalResult = new ObjectResult("plain value");

            // Act
            var result = await ExecuteAsync(_filter, originalResult);

            // Assert
            result.Should().BeSameAs(originalResult);
        }
        #endregion
    }
}
