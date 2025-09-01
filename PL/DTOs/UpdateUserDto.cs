using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(256)]
        public string? UserName { get; set; }

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool? EmailConfirmed { get; set; }

        public bool? PhoneNumberConfirmed { get; set; }

        public bool? LockoutEnabled { get; set; }

        public string? ManagerId { get; set; }

        public List<string>? Roles { get; set; }
    }

}
