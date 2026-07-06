namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    public readonly record struct RunPermissions
    {
        public readonly bool MemberAllowedToCompleteUnassignedItems { get; init; }
        public readonly bool MemberAllowedToMarkIncompleteUnassignedItems { get; init; }
        public readonly bool MemberAllowedToChangeDescriptions { get; init; }
        public readonly bool MemberAllowedToModifyNotesForUnassignedItems { get; init; }
        public readonly bool MemberAllowedToAddItems { get; init; }
        public readonly bool MemberAllowdToRemoveItems { get; init; }
    }
}
