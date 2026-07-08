using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Request DTO for updating run member permissions.</summary>
    public class UpdateRunPermissionsDto
    {
        /// <summary>Whether members can mark unassigned items as complete.</summary>
        [Required]
        public bool? MemberAllowedToCompleteUnassignedItems { get; set; }

        /// <summary>Whether members can mark unassigned items as incomplete.</summary>
        [Required]
        public bool? MemberAllowedToMarkIncompleteUnassignedItems { get; set; }

        /// <summary>Whether members can change item descriptions.</summary>
        [Required]
        public bool? MemberAllowedToChangeDescriptions { get; set; }

        /// <summary>Whether members can modify notes for unassigned items.</summary>
        [Required]
        public bool? MemberAllowedToModifyNotesForUnassignedItems { get; set; }

        /// <summary>Whether members can add new items to the run.</summary>
        [Required]
        public bool? MemberAllowedToAddItems { get; set; }

        /// <summary>Whether members can remove items from the run.</summary>
        [Required]
        public bool? MemberAllowedToRemoveItems { get; set; }
    }
}
