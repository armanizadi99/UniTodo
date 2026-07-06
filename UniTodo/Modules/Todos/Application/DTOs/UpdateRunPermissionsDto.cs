namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Request DTO for updating run member permissions.</summary>
    public class UpdateRunPermissionsDto
    {
        /// <summary>Whether members can mark unassigned items as complete.</summary>
        public bool MemberAllowedToCompleteUnassignedItems { get; set; }

        /// <summary>Whether members can mark unassigned items as incomplete.</summary>
        public bool MemberAllowedToMarkIncompleteUnassignedItems { get; set; }

        /// <summary>Whether members can change item descriptions.</summary>
        public bool MemberAllowedToChangeDescriptions { get; set; }

        /// <summary>Whether members can modify notes for unassigned items.</summary>
        public bool MemberAllowedToModifyNotesForUnassignedItems { get; set; }

        /// <summary>Whether members can add new items to the run.</summary>
        public bool MemberAllowedToAddItems { get; set; }

        /// <summary>Whether members can remove items from the run.</summary>
        public bool MemberAllowdToRemoveItems { get; set; }
    }
}
