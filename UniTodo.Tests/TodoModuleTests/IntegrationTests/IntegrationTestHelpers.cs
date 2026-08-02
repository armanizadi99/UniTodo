using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    internal static class IntegrationTestHelpers
    {
        public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        static IntegrationTestHelpers()
        {
            JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public static async Task<RunDto> CreateRunAsync(this HttpClient client, string name = "Test Run", string resetPolicy = "daily")
        {
            var response = await client.PostAsJsonAsync("api/runs", new { name, resetPolicy });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<RunDto>(JsonOptions))!;
        }

        public static async Task<RunDto> GetRunAsync(this HttpClient client, int id)
        {
            var response = await client.GetAsync($"api/runs/{id}");
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<RunDto>(JsonOptions))!;
        }

        public static async Task<HttpResponseMessage> MakeSharedAsync(this HttpClient client, int runId)
        {
            return await client.PostAsync($"api/runs/{runId}/make-shared", null);
        }

        public static async Task<List<RunItemDto>> GetRunItemsAsync(this HttpClient client, int runId)
        {
            var response = await client.GetAsync($"api/runs/{runId}/items");
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<List<RunItemDto>>(JsonOptions))!;
        }

        public static async Task<RunItemDto> AddItemAsync(this HttpClient client, int runId, string description)
        {
            var response = await client.PostAsJsonAsync($"api/runs/{runId}/items", new { description });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<RunItemDto>(JsonOptions))!;
        }

        public static async Task<TodoListTemplateDto> CreateTemplateAsync(this HttpClient client, string name = "Test Template", string resetPolicy = "daily")
        {
            var response = await client.PostAsJsonAsync("api/templates", new { name, resetPolicy });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<TodoListTemplateDto>(JsonOptions))!;
        }

        public static async Task<TodoItemTemplateDto> AddTemplateItemAsync(this HttpClient client, int templateId, string description)
        {
            var response = await client.PostAsJsonAsync($"api/templates/{templateId}/items", new { description });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<TodoItemTemplateDto>(JsonOptions))!;
        }

        public static async Task<RunMemberDto> AddMemberAsync(this HttpClient client, int runId, Guid userId)
        {
            var response = await client.PostAsJsonAsync($"api/runs/{runId}/members", new { userId });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<RunMemberDto>(JsonOptions))!;
        }
    }
}
