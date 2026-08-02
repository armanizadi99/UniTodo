using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunPermissionsTests : IntegrationTestsBase
    {
        public RunPermissionsTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetRunPermissionsAsync_ShouldReturnDefaults()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}/permissions");

            // Assert
            response.EnsureSuccessStatusCode();
            var permissions = await response.Content.ReadFromJsonAsync<RunPermissionsDto>(IntegrationTestHelpers.JsonOptions);
            permissions!.MemberAllowedToAddItems.Should().BeFalse();
            permissions.MemberAllowedToCompleteUnassignedItems.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateRunPermissionsAsync_ShouldUpdatePermissions()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PutAsJsonAsync($"api/runs/{run.Id}/permissions", new
            {
                memberAllowedToCompleteUnassignedItems = true,
                memberAllowedToMarkIncompleteUnassignedItems = false,
                memberAllowedToChangeDescriptions = true,
                memberAllowedToModifyNotesForUnassignedItems = true,
                memberAllowedToAddItems = true,
                memberAllowedToRemoveItems = true
            });

            // Assert
            response.EnsureSuccessStatusCode();
            var permissions = await response.Content.ReadFromJsonAsync<RunPermissionsDto>(IntegrationTestHelpers.JsonOptions);
            permissions!.MemberAllowedToCompleteUnassignedItems.Should().BeTrue();
            permissions.MemberAllowedToAddItems.Should().BeTrue();
            permissions.MemberAllowedToRemoveItems.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateRunPermissionsAsync_WhenUserIsNotOwner_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(run.Id, memberId);

            AuthenticateClient(memberId.ToString());

            // Act
            var response = await _client.PutAsJsonAsync($"api/runs/{run.Id}/permissions", new
            {
                memberAllowedToCompleteUnassignedItems = true,
                memberAllowedToMarkIncompleteUnassignedItems = true,
                memberAllowedToChangeDescriptions = true,
                memberAllowedToModifyNotesForUnassignedItems = true,
                memberAllowedToAddItems = true,
                memberAllowedToRemoveItems = true
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
