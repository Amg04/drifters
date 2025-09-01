using DAL.Models;

namespace PL.Services.RtspUrlBuilder
{
    public class RtspUrlBuilder : IRtspUrlBuilder
    {
        public string Build(Camera cam, string pwd)
        {
            var user = Uri.EscapeDataString(cam.Username);
            var pass = Uri.EscapeDataString(pwd);
            var host = cam.Host.Trim();
            var path = cam.RtspPath.StartsWith("/") ? cam.RtspPath : "/" + cam.RtspPath;
            return $"rtsp://{user}:{pass}@{host}:{cam.Port}{path}";
        }
    }
}
