namespace DAL.Models
{
    public class Camera : BaseClass 
    {
        public string Name { get; set; } = default!;
        public string Host { get; set; } = default!;       // IP or DNS             
        public int Port { get; set; } = 554;
        public string Username { get; set; } = default!;
        public string RtspPath { get; set; } = default!;
        public string PasswordEnc { get; set; } = default!; // Encryption
        public bool Enabled { get; set; } = true;
        public string? HlsPublicUrl { get; set; }         
        public string? HlsLocalPath { get; set; } // wwwroot
        public string Status { get; set; } = "Unknown";    // Online/Offline/Starting
        public DateTime? LastHeartbeatUtc { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; } = null!;
    }
}
