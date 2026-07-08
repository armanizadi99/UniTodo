namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Permissions that control what non-owner members can do in a shared run.</summary>
    public record RunPermissionsDto
    {
        /// <summary>Whether members can mark unassigned items as complete.</summary>
        public bool MemberAllowedToCompleteUnassignedItems { get; init; }

        /// <summary>Whether members can mark unassigned items as incomplete.</summary>
        public bool MemberAllowedToMarkIncompleteUnassignedItems { get; init; }

        /// <summary>Whether members can change item descriptions.</summary>
        public bool MemberAllowedToChangeDescriptions { get; init; }

        /// <summary>Whether members can modify notes for unassigned items.</summary>
        public bool MemberAllowedToModifyNotesForUnassignedItems { get; init; }

        /// <summary>Whether members can add new items to the run.</summary>
        public bool MemberAllowedToAddItems { get; init; }

        /// <summary>Whether members can remove items from the run.</summary>
        public bool MemberAllowedToRemoveItems { get; init; }
    }
}
