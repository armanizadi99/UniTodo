using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class AssignRunItemToMemberDto
    {
        /// <summary>The ID of the member to assign the run item to.</summary>
        [Required]
        public Guid? MemberId { get; set; } = null;
    }
}
