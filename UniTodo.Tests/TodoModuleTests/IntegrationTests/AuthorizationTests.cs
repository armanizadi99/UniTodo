using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class AuthorizationTests : IntegrationTestsBase
    {
        public AuthorizationTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task Unauthenticated_GetRuns_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("api/runs");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Unauthenticated_CreateRun_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.PostAsJsonAsync("api/runs", new { name = "No auth", resetPolicy = "daily" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Unauthenticated_GetTemplates_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("api/templates");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UserCannotReadAnotherUsersRun_ShouldReturnForbidden()
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

        [Fact]
        public async Task UserCannotDeleteAnotherUsersRun_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.DeleteAsync($"api/runs/{run.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UserCannotReadAnotherUsersTemplate_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync();
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync($"api/templates/{template.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UserCannotCreateRunFromAnotherUsersTemplate_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync();
            await _client.AddTemplateItemAsync(template.Id, "Buy milk");
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.PostAsync($"api/runs/from-template/{template.Id}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
