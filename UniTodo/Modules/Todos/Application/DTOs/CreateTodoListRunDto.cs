using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class CreateTodoListRunDto
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [EnumDataType(typeof(ResetPolicy))]
        public ResetPolicy? ResetPolicy { get; set; }
    }
}
