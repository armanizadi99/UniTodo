using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AddTodoItemTemplateDto
    {
        /// <summary>The description of the new template item.</summary>
        [Required]
        [MaxLength(Constants.DescriptionMaxLength)]
        public string Description { get; set; }
    }
}
