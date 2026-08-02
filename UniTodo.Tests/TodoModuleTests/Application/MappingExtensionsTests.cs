using FluentAssertions;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Tests.TodoModuleTests.Application
{
    public class MappingExtensionsTests
    {
        private readonly UserId _ownerId = new(Guid.NewGuid());

        #region RunMappingExtensions
        [Fact]
        public void RunToDto_ShouldMapAllProperties()
        {
            // Arrange
            var run = new Run("My Run", ResetPolicy.Weekly, true, _ownerId);
            TestHelpers.SetId(run, 5);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, _ownerId);

            // Act
            var dto = run.ToDto();

            // Assert
            dto.Id.Should().Be(5);
            dto.Name.Should().Be("My Run");
            dto.ResetPolicy.Should().Be(ResetPolicy.Weekly);
            dto.OwnerId.Should().Be(_ownerId.Value);
            dto.Status.Should().Be(TodoListRunStatus.Active);
            dto.IsShared.Should().BeTrue();
            dto.Settings.Should().NotBeNull();
            dto.Permissions.Should().NotBeNull();
        }
        #endregion

        #region RunSettings / RunPermissions mapping
        [Fact]
        public void RunSettingsToDto_ShouldMapAllProperties()
        {
            // Arrange
            var settings = new RunSettings
            {
                TimeZone = TimeZoneInfo.Utc,
                EndOfWeekDay = DayOfWeek.Friday,
                PreserveHistory = true
            };

            // Act
            var dto = settings.ToDto();

            // Assert
            dto.TimeZone.Should().Be("UTC");
            dto.EndOfWeekDay.Should().Be(DayOfWeek.Friday);
            dto.PreserveHistory.Should().BeTrue();
        }

        [Fact]
        public void RunPermissionsToDto_ShouldMapAllProperties()
        {
            // Arrange
            var permissions = new RunPermissions
            {
                MemberAllowedToCompleteUnassignedItems = true,
                MemberAllowedToMarkIncompleteUnassignedItems = true,
                MemberAllowedToChangeDescriptions = true,
                MemberAllowedToModifyNotesForUnassignedItems = true,
                MemberAllowedToAddItems = true,
                MemberAllowedToRemoveItems = true
            };

            // Act
            var dto = permissions.ToDto();

            // Assert
            dto.MemberAllowedToCompleteUnassignedItems.Should().BeTrue();
            dto.MemberAllowedToMarkIncompleteUnassignedItems.Should().BeTrue();
            dto.MemberAllowedToChangeDescriptions.Should().BeTrue();
            dto.MemberAllowedToModifyNotesForUnassignedItems.Should().BeTrue();
            dto.MemberAllowedToAddItems.Should().BeTrue();
            dto.MemberAllowedToRemoveItems.Should().BeTrue();
        }
        #endregion

        #region RunItemMappingExtensions
        [Fact]
        public void RunItemToDto_ShouldMapAllProperties()
        {
            // Arrange
            var assignee = new UserId(Guid.NewGuid());
            var completer = new UserId(Guid.NewGuid());
            var item = new RunItem(new TodoItemDescription("Buy milk"));
            TestHelpers.SetId(item, 3);
            item.AssignTo(assignee);
            item.UpdateNotes(new TodoItemNotes("organic"));
            item.MarkComplete(completer);

            // Act
            var dto = item.ToRunItemDto();

            // Assert
            dto.Id.Should().Be(3);
            dto.Description.Should().Be("Buy milk");
            dto.IsCompleted.Should().BeTrue();
            dto.CompletedAt.Should().NotBeNull();
            dto.CompletedBy.Should().Be(completer.Value);
            dto.Notes.Should().Be("organic");
            dto.AsignedTo.Should().Be(assignee.Value);
        }

        [Fact]
        public void RunItemToDto_WhenOptionalValuesAreNull_ShouldMapAsNull()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Buy milk"));

            // Act
            var dto = item.ToRunItemDto();

            // Assert
            dto.IsCompleted.Should().BeFalse();
            dto.CompletedAt.Should().BeNull();
            dto.CompletedBy.Should().BeNull();
            dto.Notes.Should().BeNull();
            dto.AsignedTo.Should().BeNull();
        }
        #endregion

        #region RunIterationMappingExtensions
        [Fact]
        public void RunIterationToDto_ShouldMapItems()
        {
            // Arrange
            var iteration = new RunIteration();
            TestHelpers.SetId(iteration, 7);
            iteration.Close();
            var item = new RunItem(new TodoItemDescription("Buy milk"));
            iteration.AddItem(item).IsSuccess.Should().BeTrue();

            // Act
            var dto = iteration.ToRunIterationDto();

            // Assert
            dto.Id.Should().Be(7);
            dto.ClosedAt.Should().NotBeNull();
            dto.Items.Should().HaveCount(1);
            dto.Items.Single().Description.Should().Be("Buy milk");
        }
        #endregion

        #region RunMemberMappingExtensions
        [Fact]
        public void RunMemberToDto_ShouldMapUserAndId()
        {
            // Arrange
            var run = new Run("My Run", ResetPolicy.Daily, true, _ownerId);
            var member = run.Members.Single(m => m.UserId == _ownerId);
            TestHelpers.SetId(member, 11);

            // Act
            var dto = member.ToDto();

            // Assert
            dto.Id.Should().Be(11);
            dto.UserId.Should().Be(_ownerId.Value);
        }
        #endregion

        #region TodoListTemplateMappingExtensions
        [Fact]
        public void TodoListTemplateToDto_ShouldMapAllProperties()
        {
            // Arrange
            var template = new TodoListTemplate(_ownerId, "Chores", ResetPolicy.Monthly);
            TestHelpers.SetId(template, 13);

            // Act
            var dto = template.ToDto();

            // Assert
            dto.Id.Should().Be(13);
            dto.Name.Should().Be("Chores");
            dto.ResetPolicy.Should().Be(ResetPolicy.Monthly);
            dto.Status.Should().Be(TodoListStatus.Active);
        }
        #endregion

        #region TodoItemTemplateMappingExtensions
        [Fact]
        public void TodoItemTemplateToDto_ShouldMapAllProperties()
        {
            // Arrange
            var template = new TodoItemTemplate(13, new TodoItemDescription("Vacuum"));
            TestHelpers.SetId(template, 21);

            // Act
            var dto = template.ToDto();

            // Assert
            dto.Id.Should().Be(21);
            dto.Description.Should().Be("Vacuum");
        }
        #endregion
    }
}
