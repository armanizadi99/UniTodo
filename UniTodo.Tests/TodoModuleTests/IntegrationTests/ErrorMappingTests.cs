using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class ErrorMappingTests : IntegrationTestsBase
    {
        public ErrorMappingTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task EntityNotFound_ShouldMapTo404()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync("api/runs/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task InvalidOperation_ShouldMapTo400()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            (await _client.PostAsync($"api/runs/{run.Id}/close", null)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items", new { description = "Too late" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DuplicateEntities_ShouldMapTo409()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items", new { description = "Buy milk" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task NotAuthorized_ShouldMapTo403()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
