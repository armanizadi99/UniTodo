using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunMembersTests : IntegrationTestsBase
    {
        public RunMembersTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetRunMembersAsync_ShouldReturnOwnerAsMember()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            AuthenticateClient(ownerId.ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}/members");

            // Assert
            response.EnsureSuccessStatusCode();
            var members = await response.Content.ReadFromJsonAsync<List<RunMemberDto>>(IntegrationTestHelpers.JsonOptions);
            members!.Single().UserId.Should().Be(ownerId);
        }

        [Fact]
        public async Task AddMemberToRunAsync_WhenRunIsPrivate_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/members", new
            {
                userId = Guid.NewGuid()
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AddMemberToRunAsync_WhenRunIsShared_ShouldAddMember()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var memberId = Guid.NewGuid();

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/members", new
            {
                userId = memberId
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var member = await response.Content.ReadFromJsonAsync<RunMemberDto>(IntegrationTestHelpers.JsonOptions);
            member!.UserId.Should().Be(memberId);
        }

        [Fact]
        public async Task AddMemberToRunAsync_WhenAlreadyAMember_ShouldReturnConflict()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(run.Id, memberId);

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/members", new
            {
                userId = memberId
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task RemoveMemberFromRunAsync_ShouldRemoveMember()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(run.Id, memberId);

            // Act
            var response = await _client.DeleteAsync($"api/runs/{run.Id}/members/{memberId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var membersResponse = await _client.GetAsync($"api/runs/{run.Id}/members");
            var members = await membersResponse.Content.ReadFromJsonAsync<List<RunMemberDto>>(IntegrationTestHelpers.JsonOptions);
            members!.Select(m => m.UserId).Should().NotContain(memberId);
        }

        [Fact]
        public async Task RemoveMemberFromRunAsync_WhenRemovingOwner_ShouldReturnBadRequest()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            AuthenticateClient(ownerId.ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);

            // Act
            var response = await _client.DeleteAsync($"api/runs/{run.Id}/members/{ownerId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task RemoveMemberFromRunAsync_WhenUserIsNotAMember_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);

            // Act
            var response = await _client.DeleteAsync($"api/runs/{run.Id}/members/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetRunMembersAsync_WhenUserIsNotAMember_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}/members");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
