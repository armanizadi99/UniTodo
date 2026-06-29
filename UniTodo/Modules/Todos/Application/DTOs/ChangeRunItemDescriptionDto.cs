using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public class ChangeRunItemDescriptionDto
    {
        /// <summary>The new description for the run item.</summary>
        [Required]
        [MaxLength(Constants.DescriptionMaxLength)]
        public string Description { get; set; }
    }
}
