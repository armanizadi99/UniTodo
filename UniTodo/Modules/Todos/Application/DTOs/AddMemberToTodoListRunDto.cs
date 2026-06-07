using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AddMemberToTodoListRunDto
    {
        [Required]
        public Guid? UserId { get; set; } = null;
    }
}
