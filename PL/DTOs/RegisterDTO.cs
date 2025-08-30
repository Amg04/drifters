using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string UserName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
