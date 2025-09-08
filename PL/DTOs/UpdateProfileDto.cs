using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class UpdateProfileDto
    {
        public string? ImageUrl { get; set; }
        [Required]
        public string Role { get; set; }
        public string? Name { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        [Required]
        public string Company { get; set; }
        public string Location { get; set; }
    }
}
