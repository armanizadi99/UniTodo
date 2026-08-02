using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunItemsTests : IntegrationTestsBase
    {
        public RunItemsTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetRunItems_ShouldReturnCurrentIterationItems()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}/items");

            // Assert
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().Description.Should().Be("Buy milk");
            items.Single().IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetRunItems_WhenUserIsNotAMember_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.AddItemAsync(run.Id, "Buy milk");
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}/items");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AddRunItem_ShouldReturnItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items", new { description = "Buy milk" });

            // Assert
            response.EnsureSuccessStatusCode();
            var item = await response.Content.ReadFromJsonAsync<RunItemDto>(IntegrationTestHelpers.JsonOptions);
            item!.Description.Should().Be("Buy milk");
            item.IsCompleted.Should().BeFalse();

            var items = await _client.GetRunItemsAsync(run.Id);
            items.Single().Id.Should().Be(item.Id);
            items.Single().Description.Should().Be("Buy milk");
        }

        [Fact]
        public async Task AddRunItem_WhenDuplicateDescription_ShouldReturnConflict()
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
        public async Task AddRunItem_WhenRunClosed_ShouldReturnBadRequest()
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
        public async Task DeleteRunItem_ShouldRemoveItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var item = await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.DeleteAsync($"api/runs/{run.Id}/items/{item.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/runs/{run.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Should().BeEmpty();
        }

        [Fact]
        public async Task MarkRunItemComplete_ShouldCompleteItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var item = await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.PostAsync($"api/runs/{run.Id}/items/{item.Id}/mark-complete", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/runs/{run.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().IsCompleted.Should().BeTrue();
            items.Single().CompletedAt.Should().NotBeNull();
            items.Single().CompletedBy.Should().NotBeNull();
        }

        [Fact]
        public async Task MarkRunItemComplete_WhenAlreadyComplete_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var item = await _client.AddItemAsync(run.Id, "Buy milk");
            (await _client.PostAsync($"api/runs/{run.Id}/items/{item.Id}/mark-complete", null)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.PostAsync($"api/runs/{run.Id}/items/{item.Id}/mark-complete", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MarkRunItemIncomplete_ShouldUncompleteItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var item = await _client.AddItemAsync(run.Id, "Buy milk");
            (await _client.PostAsync($"api/runs/{run.Id}/items/{item.Id}/mark-complete", null)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.PostAsync($"api/runs/{run.Id}/items/{item.Id}/mark-incomplete", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/runs/{run.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task MarkRunItemIncomplete_WhenNotComplete_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var item = await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.PostAsync($"api/runs/{run.Id}/items/{item.Id}/mark-incomplete", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateRunItemNotes_ShouldPersistNotes()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var item = await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items/{item.Id}/update-notes", new
            {
                notes = "Organic only"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/runs/{run.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().Notes.Should().Be("Organic only");
        }

        [Fact]
        public async Task ChangeRunItemDescription_ShouldChangeDescription()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var item = await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items/{item.Id}/change-description", new
            {
                description = "Buy almond milk"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/runs/{run.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().Description.Should().Be("Buy almond milk");
        }

        [Fact]
        public async Task ChangeRunItemDescription_WhenDuplicateOtherItem_ShouldReturnConflict()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            var first = await _client.AddItemAsync(run.Id, "Buy milk");
            await _client.AddItemAsync(run.Id, "Walk dog");

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items/{first.Id}/change-description", new
            {
                description = "Walk dog"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task AssignRunItemToMember_ShouldAssignItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(run.Id, memberId);
            var item = await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items/{item.Id}/assign-to", new
            {
                memberId
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/runs/{run.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().AsignedTo.Should().Be(memberId);
        }

        [Fact]
        public async Task AssignRunItemToNonMember_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var item = await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/items/{item.Id}/assign-to", new
            {
                memberId = Guid.NewGuid()
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MemberCannotModifyUnassignedItemsWithoutPermission_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var ownerRun = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(ownerRun.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(ownerRun.Id, memberId);
            var item = await _client.AddItemAsync(ownerRun.Id, "Buy milk");

            AuthenticateClient(memberId.ToString());

            // Act
            var response = await _client.PostAsync($"api/runs/{ownerRun.Id}/items/{item.Id}/mark-complete", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task MemberCanModifyAssignedItem_ShouldCompleteTheItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var ownerRun = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(ownerRun.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(ownerRun.Id, memberId);
            var item = await _client.AddItemAsync(ownerRun.Id, "Buy milk");
            await _client.PostAsJsonAsync($"api/runs/{ownerRun.Id}/items/{item.Id}/assign-to", new { memberId });

            AuthenticateClient(memberId.ToString());

            // Act
            var response = await _client.PostAsync($"api/runs/{ownerRun.Id}/items/{item.Id}/mark-complete", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/runs/{ownerRun.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().IsCompleted.Should().BeTrue();
            items.Single().CompletedBy.Should().Be(memberId);
        }
    }
}
