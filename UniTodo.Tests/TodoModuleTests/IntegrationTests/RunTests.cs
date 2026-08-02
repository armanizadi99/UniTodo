using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunTests : IntegrationTestsBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        static RunTests()
        {
            JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public RunTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task test1()
        {
            AuthenticateClient(Guid.NewGuid().ToString());

            var createResponse = await _client.PostAsJsonAsync("api/runs", new
            {
                name = "Test Run",
                resetPolicy = "Daily"
            });
            createResponse.EnsureSuccessStatusCode();
            var created = await createResponse.Content.ReadFromJsonAsync<RunDto>(JsonOptions);

            var response = await _client.GetAsync($"api/runs/{created!.Id}");

            response.EnsureSuccessStatusCode();
            var fetched = await response.Content.ReadFromJsonAsync<RunDto>(JsonOptions);
            fetched!.Name.Should().Be("Test Run");
        }
    }
}
