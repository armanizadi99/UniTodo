using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunLifecycleTests : IntegrationTestsBase
    {
        public RunLifecycleTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetActiveRunsAsync_ShouldOnlyReturnActiveRuns()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            await _client.CreateRunAsync("Active run");

            // Act
            var response = await _client.GetAsync("api/runs");

            // Assert
            response.EnsureSuccessStatusCode();
            var runs = await response.Content.ReadFromJsonAsync<List<RunDto>>(IntegrationTestHelpers.JsonOptions);
            runs!.Single().Name.Should().Be("Active run");
            runs.Single().Status.Should().Be(TodoListRunStatus.Active);
        }

        [Fact]
        public async Task GetClosedRunsAsync_ShouldOnlyReturnClosedRuns()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var activeRun = await _client.CreateRunAsync("Active run");
            var closedRun = await _client.CreateRunAsync("Closed run");
            (await _client.PostAsync($"api/runs/{closedRun.Id}/close", null)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.GetAsync("api/runs/closed");

            // Assert
            response.EnsureSuccessStatusCode();
            var closedRuns = await response.Content.ReadFromJsonAsync<List<RunDto>>(IntegrationTestHelpers.JsonOptions);
            closedRuns!.Select(r => r.Id).Should().NotContain(activeRun.Id);
            closedRuns.Select(r => r.Id).Should().Contain(closedRun.Id);
        }

        [Fact]
        public async Task CreateRunFromTemplateAsync_ShouldReturnCreatedWithLocationAndTemplateName()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync("Template run");
            await _client.AddTemplateItemAsync(template.Id, "Buy milk");
            await _client.AddTemplateItemAsync(template.Id, "Write report");

            // Act
            var response = await _client.PostAsync($"api/runs/from-template/{template.Id}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var run = await response.Content.ReadFromJsonAsync<RunDto>(IntegrationTestHelpers.JsonOptions);
            run!.Name.Should().Be("Template run");
            response.Headers.Location.ToString().Should().Contain($"api/runs/{run.Id}");
        }

        [Fact]
        public async Task CreateRunFromTemplateAsync_WhenRunCreated_ShouldContainCopiedItems()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var template = await _client.CreateTemplateAsync("Template run");
            await _client.AddTemplateItemAsync(template.Id, "Buy milk");

            // Act
            var runResponse = await _client.PostAsync($"api/runs/from-template/{template.Id}", null);
            runResponse.EnsureSuccessStatusCode();
            var run = await runResponse.Content.ReadFromJsonAsync<RunDto>(IntegrationTestHelpers.JsonOptions);

            // Assert
            var itemsResponse = await _client.GetAsync($"api/runs/{run!.Id}/items");
            var items = await itemsResponse.Content.ReadFromJsonAsync<List<RunItemDto>>(IntegrationTestHelpers.JsonOptions);
            items!.Single().Description.Should().Be("Buy milk");
        }

        [Fact]
        public async Task MakeRunSharedAsync_ShouldMarkRunAsShared()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync();

            // Act
            var response = await _client.PostAsync($"api/runs/{created.Id}/make-shared", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await _client.GetRunAsync(created.Id)).IsShared.Should().BeTrue();
        }

        [Fact]
        public async Task MakeRunSharedAsync_WhenAlreadyShared_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(created.Id);

            // Act
            var response = await _client.PostAsync($"api/runs/{created.Id}/make-shared", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MakeRunPrivateAsync_ShouldRemoveNonOwnerMembers()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(created.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(created.Id, memberId);

            // Act
            var response = await _client.PostAsync($"api/runs/{created.Id}/make-private", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var membersResponse = await _client.GetAsync($"api/runs/{created.Id}/members");
            var members = await membersResponse.Content.ReadFromJsonAsync<List<RunMemberDto>>(IntegrationTestHelpers.JsonOptions);
            members!.Select(m => m.UserId).Should().NotContain(memberId);
        }

        [Fact]
        public async Task CloseRunAsync_ShouldMarkRunAsClosed()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync();

            // Act
            var response = await _client.PostAsync($"api/runs/{created.Id}/close", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await _client.GetRunAsync(created.Id)).Status.Should().Be(TodoListRunStatus.Closed);
        }

        [Fact]
        public async Task CloseRunAsync_WhenAlreadyClosed_ShouldReturnBadRequest()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync();
            (await _client.PostAsync($"api/runs/{created.Id}/close", null)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.PostAsync($"api/runs/{created.Id}/close", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ResetRunAsync_WhenPolicyIsNone_ShouldSucceedAndCreateNewIteration()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync("Reset run", "none");
            await _client.AddItemAsync(created.Id, "Persist me");

            // Act
            var response = await _client.PostAsync($"api/runs/{created.Id}/reset", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetRunHistoryAsync_AfterReset_ShouldReturnClosedIteration()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync("Reset run", "none");
            await _client.AddItemAsync(created.Id, "Persist me");
            (await _client.PostAsync($"api/runs/{created.Id}/reset", null)).EnsureSuccessStatusCode();

            // Act
            var response = await _client.GetAsync($"api/runs/{created.Id}/history");

            // Assert
            response.EnsureSuccessStatusCode();
            var history = await response.Content.ReadFromJsonAsync<List<RunIterationDto>>(IntegrationTestHelpers.JsonOptions);
            history!.Should().HaveCount(1);
            history.Single().ClosedAt.Should().NotBeNull();
            history.Single().Items.Single().Description.Should().Be("Persist me");
        }

        [Fact]
        public async Task UpdateRunResetPolicyAsync_ShouldUpdatePolicy()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var created = await _client.CreateRunAsync();

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{created.Id}/reset-policy", new
            {
                resetPolicy = "monthly"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            (await _client.GetRunAsync(created.Id)).ResetPolicy.Should().Be(ResetPolicy.Monthly);
        }

        [Fact]
        public async Task UpdateRunResetPolicyAsync_WhenUserIsNotOwner_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(run.Id, memberId);
            AuthenticateClient(memberId.ToString());

            // Act
            var response = await _client.PostAsJsonAsync($"api/runs/{run.Id}/reset-policy", new
            {
                resetPolicy = "monthly"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RemoveRunAsync_WhenOwner_ShouldDeleteRun()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.AddItemAsync(run.Id, "Buy milk");

            // Act
            var response = await _client.DeleteAsync($"api/runs/{run.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var getResponse = await _client.GetAsync($"api/runs/{run.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
