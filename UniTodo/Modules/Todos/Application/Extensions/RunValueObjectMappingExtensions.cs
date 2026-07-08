using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RunValueObjectMappingExtensions
    {
        public static RunSettingsDto ToDto(this RunSettings settings)
        {
            return new RunSettingsDto
            {
                TimeZone = settings.TimeZone.Id,
                EndOfWeekDay = settings.EndOfWeekDay,
                PreserveHistory = settings.PreserveHistory
            };
        }

        public static RunPermissionsDto ToDto(this RunPermissions permissions)
        {
            return new RunPermissionsDto
            {
                MemberAllowedToCompleteUnassignedItems = permissions.MemberAllowedToCompleteUnassignedItems,
                MemberAllowedToMarkIncompleteUnassignedItems = permissions.MemberAllowedToMarkIncompleteUnassignedItems,
                MemberAllowedToChangeDescriptions = permissions.MemberAllowedToChangeDescriptions,
                MemberAllowedToModifyNotesForUnassignedItems = permissions.MemberAllowedToModifyNotesForUnassignedItems,
                MemberAllowedToAddItems = permissions.MemberAllowedToAddItems,
                MemberAllowedToRemoveItems = permissions.MemberAllowedToRemoveItems
            };
        }
    }
}
