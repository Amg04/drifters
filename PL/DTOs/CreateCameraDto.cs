using DAL;

namespace PL.DTOs
{
    public class CreateCameraDto
    {
        public string Name { get; set; } = default!;
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 554;
        public string Username { get; set; } = default!;
        public string PasswordEnc { get; set; } = default!;
        public string RtspPath { get; set; } = default!;
        public bool Enabled { get; set; } = true;
    }
}
