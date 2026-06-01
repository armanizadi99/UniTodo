using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class RemoveMemberFromTodoListRunDto
    {
        [Required]
        public Guid? UserId { get; set; } = null;
    }
}
