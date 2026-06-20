using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AddMemberToTodoListRunDto
    {
        /// <summary>The ID of the user to add as a member to the run.</summary>
        [Required]
        public Guid? UserId { get; set; } = null;
    }
}
