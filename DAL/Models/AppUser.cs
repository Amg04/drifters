using Microsoft.AspNetCore.Identity;

namespace DAL.Models
{
    public class AppUser : IdentityUser
    {
        public string? ManagerId { get; set; }
        public AppUser? Manager { get; set; }
        public ICollection<AppUser> Subordinates { get; set; } = new HashSet<AppUser>();
        public ICollection<Camera> Cameras { get; set; } = new HashSet<Camera>();
    }
}
