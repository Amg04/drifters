using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(256)]
        public string UserName { get; set; } 

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } 

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; } 

        public bool PhoneNumberConfirmed { get; set; }

        public string? Role { get; set; }

    }
}
