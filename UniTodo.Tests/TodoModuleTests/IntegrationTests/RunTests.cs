using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunTests : IntegrationTestsBase
    {
        public RunTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task CreateRunAsync_ShouldReturnOkWithRunDto()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var createResponse = await _client.PostAsJsonAsync("api/runs", new
            {
                name = "Test Run",
                resetPolicy = "daily"
            });

            // Assert
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var created = await createResponse.Content.ReadFromJsonAsync<RunDto>(IntegrationTestHelpers.JsonOptions);
            created!.Id.Should().BeGreaterThan(0);
            created.Name.Should().Be("Test Run");
            created.ResetPolicy.Should().Be(ResetPolicy.Daily);
            created.Status.Should().Be(TodoListRunStatus.Active);
            created.IsShared.Should().BeFalse();
        }

        [Fact]
        public async Task GetRunByIdAsync_WhenRunExists_ShouldReturnTheRun()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync("Test Run");

            // Act
            var response = await _client.GetAsync($"api/runs/{created.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var fetched = await response.Content.ReadFromJsonAsync<RunDto>(IntegrationTestHelpers.JsonOptions);
            fetched!.Name.Should().Be("Test Run");
            fetched.Id.Should().Be(created.Id);
        }

        [Fact]
        public async Task GetRunByIdAsync_WhenRunDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync("api/runs/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetRunByIdAsync_WhenUserIsNotAMember_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync("Owned by someone else");
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync($"api/runs/{created.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
