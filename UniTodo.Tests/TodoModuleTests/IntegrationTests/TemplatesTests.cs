using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class TemplatesTests : IntegrationTestsBase
    {
        public TemplatesTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task CreateTodoListTemplateAsync_ShouldReturnCreatedWithLocation()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.PostAsJsonAsync("api/templates", new
            {
                name = "My Template",
                resetPolicy = "weekly"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var template = await response.Content.ReadFromJsonAsync<TodoListTemplateDto>(IntegrationTestHelpers.JsonOptions);
            template!.Id.Should().BeGreaterThan(0);
            template.Name.Should().Be("My Template");
            response.Headers.Location.ToString().Should().Contain($"api/templates/{template.Id}");
        }

        [Fact]
        public async Task GetAllTodoListTemplatesAsync_ShouldReturnOwnersTemplates()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            await _client.CreateTemplateAsync("Template one");
            await _client.CreateTemplateAsync("Template two");

            // Act
            var response = await _client.GetAsync("api/templates");

            // Assert
            response.EnsureSuccessStatusCode();
            var templates = await response.Content.ReadFromJsonAsync<List<TodoListTemplateDto>>(IntegrationTestHelpers.JsonOptions);
            templates!.Select(t => t.Name).Should().BeEquivalentTo("Template one", "Template two");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_WhenOwned_ShouldReturnTemplate()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync("My Template");

            // Act
            var response = await _client.GetAsync($"api/templates/{template.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var fetched = await response.Content.ReadFromJsonAsync<TodoListTemplateDto>(IntegrationTestHelpers.JsonOptions);
            fetched!.Name.Should().Be("My Template");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_WhenNotOwned_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync("Private template");
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync($"api/templates/{template.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_WhenNotFound_ShouldReturnNotFound()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync("api/templates/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteTodoListTemplateAsync_ShouldDeleteTemplate()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync("To delete");

            // Act
            var response = await _client.DeleteAsync($"api/templates/{template.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var getResponse = await _client.GetAsync($"api/templates/{template.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteTodoListTemplateAsync_WhenNotOwned_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync("Private template");
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.DeleteAsync($"api/templates/{template.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateTodoListTemplateAsync_WhenNameDuplicated_ShouldReturnConflict()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            await _client.CreateTemplateAsync("Same name");

            // Act
            var response = await _client.PostAsJsonAsync("api/templates", new
            {
                name = "Same name",
                resetPolicy = "daily"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldReturnItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync();

            // Act
            var response = await _client.PostAsJsonAsync($"api/templates/{template.Id}/items", new
            {
                description = "Buy milk"
            });

            // Assert
            response.EnsureSuccessStatusCode();
            var item = await response.Content.ReadFromJsonAsync<TodoItemTemplateDto>(IntegrationTestHelpers.JsonOptions);
            item!.Description.Should().Be("Buy milk");
        }

        [Fact]
        public async Task GetTodoItemTemplatesAsync_ShouldReturnItems()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync();
            await _client.AddTemplateItemAsync(template.Id, "Buy milk");
            await _client.AddTemplateItemAsync(template.Id, "Walk dog");

            // Act
            var response = await _client.GetAsync($"api/templates/{template.Id}/items");

            // Assert
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<TodoItemTemplateDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Select(i => i.Description).Should().BeEquivalentTo("Buy milk", "Walk dog");
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldDeleteItem()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync();
            var item = await _client.AddTemplateItemAsync(template.Id, "Buy milk");

            // Act
            var response = await _client.DeleteAsync($"api/templates/{template.Id}/items/{item.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var itemsResponse = await _client.GetAsync($"api/templates/{template.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<TodoItemTemplateDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTodoItemTemplatesAsync_WhenNotOwned_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync();
            await _client.AddTemplateItemAsync(template.Id, "Buy milk");
            AuthenticateClient(Guid.NewGuid().ToString());

            // Act
            var response = await _client.GetAsync($"api/templates/{template.Id}/items");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
