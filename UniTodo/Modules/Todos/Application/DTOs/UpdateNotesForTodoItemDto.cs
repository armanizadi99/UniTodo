using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class UpdateNotesForTodoItemDto
    {
        [Required]
        [MaxLength(Constants.NotesMaxLength)]
        public string Notes { get; set; }
    }
}
