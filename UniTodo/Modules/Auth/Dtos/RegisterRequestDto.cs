using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Auth.Dtos
{
    public class RegisterRequestDto
    {
        /// <summary>The email address used for registration.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        /// <summary>The password for the new account.</summary>
        [Required]
        public string Password { get; set; }
    }
}
