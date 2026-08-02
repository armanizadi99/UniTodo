using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class ValidationTests : IntegrationTestsBase
    {
        public ValidationTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task CreateRunAsync_WhenNameMissing_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.PostAsJsonAsync("api/runs", new { resetPolicy = "daily" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRunAsync_WhenResetPolicyMissing_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.PostAsJsonAsync("api/runs", new { name = "Test Run" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateRunAsync_WhenResetPolicyInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.PostAsJsonAsync("api/runs", new { name = "Test Run", resetPolicy = "fortnightly" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddRunItemAsync_WhenDescriptionMissing_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items", new { });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddRunItemAsync_WhenDescriptionEmpty_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items", new { description = "" });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRunSettingsAsync_WhenTimeZoneInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PutAsJsonAsync($"api/runs/{run.Id}/settings", new
            {
                timeZone = "Mars/Olympus",
                endOfWeekDay = "sunday",
                preserveHistory = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRunSettingsAsync_WhenEndOfWeekDayInvalid_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PutAsJsonAsync($"api/runs/{run.Id}/settings", new
            {
                timeZone = "UTC",
                endOfWeekDay = "funday",
                preserveHistory = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateTodoListTemplateAsync_WhenNameTooLong_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.PostAsJsonAsync("api/templates", new
            {
                name = new string('x', 101),
                resetPolicy = "daily"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddMemberToRunAsync_WhenUserIdMissing_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/members", new { });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
