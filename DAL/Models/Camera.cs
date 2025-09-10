namespace DAL.Models
{
    public class Camera : BaseClass 
    {
        public string Host { get; set; } = default!;       // IP or DNS             
        public int Port { get; set; } = 554;
        public string Username { get; set; } = default!;
        public string RtspPath { get; set; } = default!;
        public string PasswordEnc { get; set; } = default!; // Encryption
        public bool Enabled { get; set; } = true;
        public string CameraLocation { get; set; } = default!;
        public string? HlsPublicUrl { get; set; }         
        public string? HlsLocalPath { get; set; } // wwwroot
        public string Status { get; set; } = "Unknown";    // Online/Offline/Starting
        public DateTime? LastHeartbeatUtc { get; set; }
        public string Type { get; set; } = "normal";  // normal or apnormal   
        public int CriticalEvent { get; set; }
        public int MonitoredEntityId { get; set; } = default!;
        public MonitoredEntity MonitoredEntity { get; set; } = null!;
        public ICollection<CameraDetection> CameraDetections { get; set; } = new HashSet<CameraDetection>();
    }
}
