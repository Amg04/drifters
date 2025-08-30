using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class LoginDTO
    {
        [Required]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
    }
}
