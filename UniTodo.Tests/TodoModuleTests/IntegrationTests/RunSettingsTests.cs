using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TimeZoneConverter;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunSettingsTests : IntegrationTestsBase
    {
        public RunSettingsTests(IntegrationTestsWebAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetRunSettings_ShouldReturnDefaults()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.GetAsync($"api/runs/{run.Id}/settings");

            // Assert
            response.EnsureSuccessStatusCode();
            var settings = await response.Content.ReadFromJsonAsync<RunSettingsDto>(IntegrationTestHelpers.JsonOptions);
            settings!.TimeZone.Should().Be("UTC");
            settings.PreserveHistory.Should().BeTrue();
            settings.EndOfWeekDay.Should().Be(DayOfWeek.Friday);
        }

        [Fact]
        public async Task UpdateRunSettings_ShouldUpdateSettings()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();

            // Act
            var response = await _client.PutAsJsonAsync($"api/runs/{run.Id}/settings", new
            {
                timeZone = "America/New_York",
                endOfWeekDay = "sunday",
                preserveHistory = false
            });

            // Assert
            response.EnsureSuccessStatusCode();

            var settingsResponse = await _client.GetAsync($"api/runs/{run.Id}/settings");
            var settings = await settingsResponse.Content.ReadFromJsonAsync<RunSettingsDto>(IntegrationTestHelpers.JsonOptions);
            settings!.TimeZone.Should().Be(TZConvert.GetTimeZoneInfo("America/New_York").Id);
            settings.EndOfWeekDay.Should().Be(DayOfWeek.Sunday);
            settings.PreserveHistory.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateRunSettings_WhenUserIsNotOwner_ShouldReturnForbidden()
        {
            // Arrange
            AuthenticateClient(Guid.NewGuid().ToString());
            var run = await _client.CreateRunAsync();
            await _client.MakeSharedAsync(run.Id);
            var memberId = Guid.NewGuid();
            await _client.AddMemberAsync(run.Id, memberId);

            AuthenticateClient(memberId.ToString());

            // Act
            var response = await _client.PutAsJsonAsync($"api/runs/{run.Id}/settings", new
            {
                timeZone = "America/New_York",
                endOfWeekDay = "sunday",
                preserveHistory = false
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
