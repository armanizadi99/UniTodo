using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AddTodoItemDto
    {
        /// <summary>The description of the new todo item.</summary>
        [Required]
        [MinLength(1), MaxLength(Constants.DescriptionMaxLength)]
        public string Description { get; set; } = null!;
    }
}
