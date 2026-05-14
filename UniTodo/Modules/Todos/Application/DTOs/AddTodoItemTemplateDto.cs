using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AddTodoItemTemplateDto
    {
[Required]
[Range(1, int.MaxValue)]
internal int? TodoListId { get; set; }

[Required]
[MaxLength(Constants.DescriptionMaxLength)]
internal string Description { get; set; }
    }
}
