using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class UpdateNotesForRunItemDto
    {
        /// <summary>The updated notes for the run item.</summary>
        [Required]
        [MaxLength(Constants.NotesMaxLength)]
        public string Notes { get; set; }
    }
}
