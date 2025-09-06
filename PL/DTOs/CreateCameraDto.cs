using System.ComponentModel.DataAnnotations;

namespace PL.DTOs
{
    public class CreateCameraDto
    {
        [Required]
        public string Host { get; set; } = default!;
        [Required]
        public int Port { get; set; } = 554;
        [Required]
        public string Username { get; set; } = default!;
        [Required]
        public string PasswordEnc { get; set; } = default!;
        [Required]
        public string RtspPath { get; set; } = default!;
        public bool Enabled { get; set; } = true;
        [Required]
        public string CameraLocation { get; set; } = default!;
    }
}
