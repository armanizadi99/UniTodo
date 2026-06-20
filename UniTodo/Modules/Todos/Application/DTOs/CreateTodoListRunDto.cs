using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class CreateTodoListRunDto
    {
        /// <summary>The name of the todo list run.</summary>
        [Required]
        public string Name { get; set; } = null!;
        /// <summary>The reset policy that governs when items in this run reset.</summary>
        [Required]
        [EnumDataType(typeof(ResetPolicy))]
        public ResetPolicy? ResetPolicy { get; set; }
    }
}
