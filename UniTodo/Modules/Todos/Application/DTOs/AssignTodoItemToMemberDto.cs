using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AssignTodoItemToMemberDto
    {
        [Required]
        public Guid? MemberId { get; set; } = null;
    }
}
