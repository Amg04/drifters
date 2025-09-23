using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class CreateEditCameraDto
    {
        public string? Host { get; set; } 
        public int Port { get; set; } = 554;
        public string? Username { get; set; } 
        public string? PasswordEnc { get; set; } 
        [Required]
        public string RtspPath { get; set; } = default!;
        public bool Enabled { get; set; } = true;
        public string? CameraLocation { get; set; }
    }
}
