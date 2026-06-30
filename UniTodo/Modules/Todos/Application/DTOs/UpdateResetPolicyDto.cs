using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class UpdateResetPolicyDto
    {
        /// <summary>The new reset policy to apply to the run.</summary>
        [Required]
        [EnumDataType(typeof(ResetPolicy))]
        public ResetPolicy ResetPolicy { get; set; }
    }
}
