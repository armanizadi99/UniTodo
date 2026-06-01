using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class ChangeTodoItemDescriptionDto
    {
        [Required]
        [MaxLength(Constants.DescriptionMaxLength)]
        public string Description { get; set; }
    }
}
