using FluentAssertions;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Tests.TodoModuleTests.Domain.Common
{
    public class ResultTests
    {
       #region Result (void)

        [Fact]
        public void Success_ShouldReturnSuccessfulResult()
        {
            // Act
            var result = Result.Success();

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Failure_ShouldReturnFailedResultWithError()
        {
            // Arrange
            var error = new DomainError(DomainErrorCodes.EntityNotFound, "missing");

            // Act
            var result = Result.Failure(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void AccessingErrorOnSuccess_ShouldThrowInvalidOperationException()
        {
            // Act
            var result = Result.Success();

            // Assert
            var act = () => result.Error;
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ImplicitConversionFromDomainError_ShouldCreateFailureResult()
        {
            // Arrange
            var error = DomainError.NotAuthorized();

            // Act
            Result result = error;

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }
        #endregion

       #region Result<T>
        [Fact]
        public void Success_ShouldReturnSuccessfulResultWithValue()
        {
            // Act
            var result = Result<int>.Success(42);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
        }

        [Fact]
        public void Failure_ForGenericResult_ShouldReturnFailedResultWithError()
        {
            // Arrange
            var error = new DomainError(DomainErrorCodes.InvalidOperation, "invalid");

            // Act
            var result = Result<int>.Failure(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void AccessingValueOnFailure_ShouldThrowInvalidOperationException()
        {
            // Act
            var result = Result<int>.Failure(DomainError.NotAuthorized());

            // Assert
            var act = () => result.Value;
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GenericResult_AccessingErrorOnSuccess_ShouldThrowInvalidOperationException()
        {
            // Act
            var result = Result<int>.Success(42);

            // Assert
            var act = () => result.Error;
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ImplicitConversionFromValue_ShouldCreateSuccessResult()
        {
            // Act
            Result<int> result = 7;

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(7);
        }

        [Fact]
        public void GenericResult_ImplicitConversionFromDomainError_ShouldCreateFailureResult()
        {
            // Arrange
            var error = DomainError.EntityNotFound("Widget", 1);

            // Act
            Result<int> result = error;

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be(error);
        }
        #endregion
    }
}
