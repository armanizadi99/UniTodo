using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class CreateTodoListTemplateDto
{
        [Required]
        [MaxLength(100)]
        internal string Name { get; set; }
        [Required]
            [EnumDataType(typeof(ResetPolicy))]
            internal ResetPolicy? ResetPolicy { get; set; }
    }
}
