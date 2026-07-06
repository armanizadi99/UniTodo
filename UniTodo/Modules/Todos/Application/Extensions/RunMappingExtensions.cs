using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RunMappingExtensions
    {
        public static RunDto ToDto(this Run run)
        {
            return new RunDto(
                run.Id,
    run.Name,
    run.ResetPolicy,
    run.ownerId.Value,
    run.Status,
    run.IsShared,
    run.ClosedAt,
    run.CreatedAt,
    run.UpdatedAt)
            {
                Settings = new RunSettingsDto
                {
                    TimeZone = run.Settings.TimeZone.Id,
                    EndOfWeekDay = run.Settings.EndOfWeekDay,
                    PreserveHystory = run.Settings.PreserveHystory
                },
                Permissions = new RunPermissionsDto
                {
                    MemberAllowedToCompleteUnassignedItems = run.Permissions.MemberAllowedToCompleteUnassignedItems,
                    MemberAllowedToMarkIncompleteUnassignedItems = run.Permissions.MemberAllowedToMarkIncompleteUnassignedItems,
                    MemberAllowedToChangeDescriptions = run.Permissions.MemberAllowedToChangeDescriptions,
                    MemberAllowedToModifyNotesForUnassignedItems = run.Permissions.MemberAllowedToModifyNotesForUnassignedItems,
                    MemberAllowedToAddItems = run.Permissions.MemberAllowedToAddItems,
                    MemberAllowdToRemoveItems = run.Permissions.MemberAllowdToRemoveItems
                }
            };
        }
    }
}
