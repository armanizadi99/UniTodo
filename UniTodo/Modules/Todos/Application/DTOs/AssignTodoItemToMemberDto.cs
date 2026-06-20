using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AssignTodoItemToMemberDto
    {
        /// <summary>The ID of the member to assign the todo item to.</summary>
        [Required]
        public Guid? MemberId { get; set; } = null;
    }
}
