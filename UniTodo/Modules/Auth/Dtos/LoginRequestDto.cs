using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Auth.Dtos
{
    /// <summary>Request to authenticate a user and receive JWT tokens.</summary>
    public class LoginRequestDto
    {
        /// <summary>The email address of the user.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        /// <summary>The user's password.</summary>
        [Required]
        public string Password { get; set; }
    }
}
