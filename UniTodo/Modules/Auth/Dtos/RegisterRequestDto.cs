using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Auth.Dtos
{
    public class RegisterRequestDto
    {
[Required]
[EmailAddress]
public string Email { get; set; }
[Required]
public string Password { get; set; }
    }
}
