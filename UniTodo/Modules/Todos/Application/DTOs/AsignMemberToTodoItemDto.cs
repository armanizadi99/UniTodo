using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AsignMemberToTodoItemDto
    {
        [Required]
        public Guid? MemberId { get; set; } = null;
    }
}
