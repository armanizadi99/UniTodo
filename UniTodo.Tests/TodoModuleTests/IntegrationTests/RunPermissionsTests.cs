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
        public async Task GetRunPermissions_ShouldReturnDefaults()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}/permissions");

            // Assert
            response.EnsureSuccessStatusCode();
            var permissions = await response.Content.ReadFromJsonAsync<RunPermissionsDto>(IntegrationTestHelpers.JsonOptions);
            permissions!.MemberAllowedToCompleteUnassignedItems.Should().BeFalse();
            permissions.MemberAllowedToMarkIncompleteUnassignedItems.Should().BeFalse();
            permissions.MemberAllowedToChangeDescriptions.Should().BeFalse();
            permissions.MemberAllowedToModifyNotesForUnassignedItems.Should().BeFalse();
            permissions.MemberAllowedToAddItems.Should().BeFalse();
            permissions.MemberAllowedToRemoveItems.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateRunPermissions_ShouldUpdatePermissions()
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
                memberAllowedToModifyNotesForUnassignedItems = false,
                memberAllowedToAddItems = true,
                memberAllowedToRemoveItems = false
            });

            // Assert
            response.EnsureSuccessStatusCode();

            var permissionsResponse = await _client.GetAsync($"api/runs/{run.Id}/permissions");
            var permissions = await permissionsResponse.Content.ReadFromJsonAsync<RunPermissionsDto>(IntegrationTestHelpers.JsonOptions);
            permissions!.MemberAllowedToCompleteUnassignedItems.Should().BeTrue();
            permissions.MemberAllowedToMarkIncompleteUnassignedItems.Should().BeFalse();
            permissions.MemberAllowedToChangeDescriptions.Should().BeTrue();
            permissions.MemberAllowedToModifyNotesForUnassignedItems.Should().BeFalse();
            permissions.MemberAllowedToAddItems.Should().BeTrue();
            permissions.MemberAllowedToRemoveItems.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateRunPermissions_WhenUserIsNotOwner_ShouldReturnForbidden()
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
