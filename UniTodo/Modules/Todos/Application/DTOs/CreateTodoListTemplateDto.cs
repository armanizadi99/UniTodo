using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class CreateTodoListTemplateDto
    {
        /// <summary>The name of the todo list template.</summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        /// <summary>Determines how and when the list resets (e.g. daily, weekly, never).</summary>
        [Required]
        [EnumDataType(typeof(ResetPolicy))]
        public ResetPolicy? ResetPolicy { get; set; }
    }
}
