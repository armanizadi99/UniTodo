using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Auth.Dtos
{
    /// <summary>The request to refresh an access token using a refresh token.</summary>
    public class RefreshAccessTokenRequestDto
    {
        /// <summary>The refresh token issued during login or a previous refresh.</summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
