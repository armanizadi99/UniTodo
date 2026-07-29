using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Request to update the notes text on a run item.</summary>
    public class UpdateNotesForRunItemDto
    {
        /// <summary>The updated notes for the run item.</summary>
        [Required]
        [MaxLength(Constants.NotesMaxLength)]
        public string Notes { get; set; }
    }
}
