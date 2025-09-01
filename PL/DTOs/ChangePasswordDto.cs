using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{

    public class ChangePasswordDto
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
