using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Request to change the reset policy of a run.</summary>
    public class UpdateResetPolicyDto
    {
        /// <summary>The new reset policy to apply to the run.</summary>
        [Required]
        [EnumDataType(typeof(ResetPolicy))]
        public ResetPolicy ResetPolicy { get; set; }
    }
}
