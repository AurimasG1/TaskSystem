using System.ComponentModel.DataAnnotations;

namespace TaskSystem.Common.DTO
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Role optional, default = "user"
        public string Role { get; set; } = "user";
    }
}
