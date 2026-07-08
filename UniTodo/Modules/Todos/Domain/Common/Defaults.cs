using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Common
{
    public static class Defaults
    {
        public static RunSettings DefaultRunSettings = new RunSettings
        {
            EndOfWeekDay = DayOfWeek.Friday,
            TimeZone = TimeZoneInfo.Utc,
            PreserveHistory = true
        };

        public static RunPermissions DefaultRunPermissions = new RunPermissions
        {
            MemberAllowedToRemoveItems = false,
            MemberAllowedToAddItems = false,
            MemberAllowedToChangeDescriptions = false,
            MemberAllowedToCompleteUnassignedItems = false,
            MemberAllowedToMarkIncompleteUnassignedItems = false,
            MemberAllowedToModifyNotesForUnassignedItems = false
        };
    }
}
